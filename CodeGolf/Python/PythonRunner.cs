using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;
using System.Runtime.InteropServices;
using CodeGolf.Tools;

namespace CodeGolf.Python;

public sealed class PythonRunner : IDisposable
{
    private readonly ILogger<PythonRunner> _logger;
    private const string pythonContainer = @"docker.io/python:alpine";
    private static readonly TimeSpan RunnerTimeOut = TimeSpan.FromSeconds(60);
    private static readonly Encoding encoding = Encoding.UTF8;
    private readonly CancellationTokenSource _lifetimeTokenSource = new();
    private readonly DockerClient? _containerClient;
    private readonly DirectoryInfo submissionDirectory = Directory.CreateTempSubdirectory("CodeGolfSubmissions");

    public bool Loaded {get; private set;}

    public PythonRunner(ILogger<PythonRunner> logger)
    {
        _logger = logger;
        _containerClient = CreateContainerClient();
        LoadRunner();
    }

    public void Dispose(){
        _lifetimeTokenSource.Cancel();
        _lifetimeTokenSource.Dispose();
        _containerClient?.Dispose();
    }    

    private void LoadRunner(){
        Task.Run(async () => {
            await PullImage(_containerClient!, _lifetimeTokenSource.Token).ConfigureAwait(false);
            Loaded = true;
        }, _lifetimeTokenSource.Token);
    }

    public async Task<(long, string, string)> ExecutePythonCode(string pythonCode, string input = "", CancellationToken token = default)
    {
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token, _lifetimeTokenSource.Token);
        return await RunContainer(_containerClient!, pythonCode, input, combinedTokenSource.Token).ConfigureAwait(false);
    }

    private async Task PullImage(DockerClient client, CancellationToken token)
    {
        var config = new ImagesCreateParameters(){
            FromImage = pythonContainer,
        };
        AuthConfig? auth = null;
        var progress = new Progress<JSONMessage>();

        _logger.LoadImage(pythonContainer);
        await client.Images.CreateImageAsync(config, auth, progress, token).ConfigureAwait(false); 
        _logger.LoadImageDone(pythonContainer);
    }


    private async Task<(long, string, string)> RunContainer(DockerClient client, string pythonCode, string stdin, CancellationToken token)
    {
        var filePath = Path.Combine(submissionDirectory.FullName, Path.GetRandomFileName());
        await File.WriteAllTextAsync(filePath, pythonCode, token).ConfigureAwait(false);
        
        //TODO add more restricted hostConfig so to not allow dos
        var config = new CreateContainerParameters(){
            Image = pythonContainer,
            Cmd = ["python", "submission.py"],
            OpenStdin = true,
            StdinOnce = true,
            User = "nobody",
            HostConfig = new HostConfig()
            {
                NetworkMode = "none",
                Memory = 256 * 1024 * 1024, //256MB
                ReadonlyRootfs = true,
                NanoCPUs = 250_000_000, //0.25 CPNanoCPUs = 250_U
                PidsLimit = 64,
                CapDrop = ["ALL"],
                Binds = [$"{filePath}:/submission.py:ro"]
            }
        };
        
        var container = await client.Containers.CreateContainerAsync(config, token).ConfigureAwait(false);
        _logger.CreateContainer(pythonContainer, container.ID);


        try
        {   
            var attachParameters = new ContainerAttachParameters(){
                Stream = true,
                Stdin = true,
                Stdout = true,
                Stderr = true
            };

            var multiplexedStream = await client.Containers.AttachContainerAsync(container.ID, tty: false,attachParameters, token).ConfigureAwait(false);
            var buffer = encoding.GetBytes(stdin);
            await multiplexedStream.WriteAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
            multiplexedStream.CloseWrite();

            var result = await client.Containers.StartContainerAsync(container.ID, new(), token).ConfigureAwait(false);
            _logger.StartContainer(container.ID, stdin);
            
            var waitResult = await client.Containers.WaitContainerAsync(container.ID, token).WaitAsync(RunnerTimeOut, token).ConfigureAwait(false);
            var exitCode = waitResult.StatusCode;

            var logStream = await client.Containers.GetContainerLogsAsync(container.ID, tty: false, new(){
                ShowStdout = true,
                ShowStderr = true
            }, token).ConfigureAwait(false);

            (string stdout, string stderr) = await logStream.ReadOutputToEndAsync(token).ConfigureAwait(false);
            _logger.ContainerCommand(pythonCode, stdin, stdout, stderr);

            //Seems like the following doesn't report anything
            /*
            var statsHandler = new Progress<ContainerStatsResponse>((stats) => _logger.ContainerStats(stats.Name, JsonSerializer.Serialize(stats, jsonSerializerOptions)));

            await client.Containers.GetContainerStatsAsync(container.ID, new(){
                Stream = false,
                OneShot = true
            }, statsHandler, token).ConfigureAwait(false);
            */
            return (exitCode, stdout, stderr);
        }
        catch(TimeoutException ex){
            _logger.ContainerTimeout(container.ID, RunnerTimeOut, ex);
            throw;
        }
        finally
        {
            await client.Containers.RemoveContainerAsync(container.ID, new(){
                Force = true,
            }, token).ConfigureAwait(false);
            File.Delete(filePath);
            _logger.RemoveContainer(container.ID);
        }
    }

    private DockerClient? CreateContainerClient()
    {
        var containerHostUri = GetContainerClientUri();
        try{
            //Disposed by client
            #pragma warning disable CA2000 // Dispose objects before losing scope 
            var containerClientConfiguration = new DockerClientConfiguration(new Uri(containerHostUri));
            #pragma warning restore CA2000 // Dispose objects before losing scope
            return containerClientConfiguration.CreateClient();
        }
        catch(Exception ex){
            _logger.ContainerClientLoadFailed(containerHostUri, ex);
            throw;
        }
    }

    private static string GetContainerClientUri()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "npipe://./pipe/docker_engine";
        }
        else
        {
            [DllImport("libc")]
            static extern uint geteuid();           

            string podmanPath = $"/run/user/{geteuid()}/podman/podman.sock";
            if (File.Exists(podmanPath))
            {
                return $"unix://{podmanPath}";
            }

            string dockerPath = "/var/run/docker.sock";
            if(File.Exists(dockerPath)){
                return $"unix://{dockerPath}";
            }

            return "";
        }
    }
}
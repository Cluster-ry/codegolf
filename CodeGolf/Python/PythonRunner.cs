using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;
using CodeGolf.Tools;

namespace CodeGolf.Python;

//This needs more thought
// in its current form yes it runs the python succesfully but is really unsecure.
// for example person could write to a file and execute it or just delete the root.
// Maybe launch a docker for each submission?
public sealed class PythonRunner
{
    private readonly ILogger<PythonRunner> _logger;
    private static readonly Encoding encoding = Encoding.UTF8;

    public PythonRunner(ILogger<PythonRunner> logger)
    {
        _logger = logger;
    }    

    public async Task<(string, string)> ExecutePrintingPythonCode(string pythonCode, Stream? inputStream = null, CancellationToken token = default)
    {
        TimeSpan maxTimeToProcess = TimeSpan.FromSeconds(5);

        var engine = InitialiceEngine();
        var outputStream = new MemoryStream();
        var errorStream = new MemoryStream();

        if(inputStream is not null) engine.Runtime.IO.SetInput(inputStream, encoding);
        engine.Runtime.IO.SetOutput(outputStream, encoding);
        engine.Runtime.IO.SetErrorOutput(errorStream, encoding);

        var scope = engine.CreateScope();
        var source = engine.CreateScriptSourceFromString(pythonCode);

        await Task.Run(() => source.Execute(scope), token).WaitAsync(maxTimeToProcess, token);
        
        var outputTask = ReadFullStream(outputStream, encoding, token);
        var errorTask = ReadFullStream(errorStream, encoding, token);
        var results = await Task.WhenAll(outputTask, errorTask).ConfigureAwait(false);

        return (results[0], results[1]);
    }

    private static Task<string> ReadFullStream(Stream stream, Encoding encoding, CancellationToken token)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, encoding);
        
        return reader.ReadToEndAsync(token);        
    }


    private static ScriptEngine InitialiceEngine()
    {
        var engine = IronPython.Hosting.Python.CreateEngine();
        // Set up search paths to include your virtual environment's site-packages
        var paths = engine.GetSearchPaths();
        //paths.Add("path/to/your/virtualenv/Lib/site-packages"); // Adjust the path
        engine.SetSearchPaths(paths);

        return engine;
    }
}
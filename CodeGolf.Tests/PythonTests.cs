using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Xunit.Abstractions;

namespace CodeGolf.Tests;
using CodeGolf.Python;
using Microsoft.AspNetCore.Builder;

public class PythonTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    public PythonTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private PythonRunner CreateRunner() => new PythonRunner(_testOutputHelper.BuildLoggerFor<PythonRunner>());

    [Fact(Timeout = 60_000)]
    public async Task PythonRunner_LoadRunner_HappyPath()
    {
        using var runner = CreateRunner();
        while(!runner.Loaded) await Task.Delay(100);
        Assert.True(true);
    }

    [Fact(Timeout = 60_000)]
    public async Task PythonRunner_HelloWorld_Sanity()
    {
        string target = "Hello World";
        string pythonCode = $"""print("{target}")""";
        using var runner = CreateRunner();
        while(!runner.Loaded) await Task.Delay(100);
        (long errorCode, string output, string error) = await runner.ExecutePythonCode(pythonCode);
        Assert.True(errorCode == 0);
        Assert.NotEmpty(output);
        Assert.Empty(error);
        Assert.Equal($"{target}\n", output);
    }

    [Fact(Timeout = 60_000)]
    public async Task PythonRunner_pythonCode_Timeout()
    {
        string input = """
                       print("Hello world")
                       i = 0
                       while 1:
                           i + 1
                       print("Done")
                       """;

        await Assert.ThrowsAsync<TimeoutException>(async () => {
            using var runner = CreateRunner();
            (long errorCode, string outputString, string errorString) = await runner.ExecutePythonCode(input);
        });
    }

    [Fact(Timeout = 60_000)]
    public async Task PythonRunner_pythonCode_HandlesFailsGracefully()
    {
        string input = """
                     L = ["This is Delhi \n", "This is Paris \n", "This is London \n"]
                     
                     # Writing to file
                     with open("myfile.txt", "w") as file1:
                         # Writing data to a file
                         file1.write("Hello \n")
                         file1.writelines(L)
                     
                     with open("myfile.txt", "o") as file2:
                        print(file2.readlines())
                     """;
        using var runner = CreateRunner();
        (long errorCode, string outputString, string errorString) = await runner.ExecutePythonCode(input);
        Assert.True(errorCode != 0);
        Assert.Empty(outputString);
        Assert.NotEmpty(errorString);
    }

    //TODO FIX TEST
    [Fact(Timeout = 60_000)]
    public async Task PythonRunner_pythonCode_TryToCreateFiles()
    {
        string input = """
                     L = ["This is Delhi \n", "This is Paris \n", "This is London \n"]
                     
                     # Writing to file
                     with open("myfile.txt", "w") as file1:
                         # Writing data to a file
                         file1.write("Hello \n")
                         file1.writelines(L)
                     
                     with open("myfile.txt", "r") as file2:
                        print(file2.readlines())
                     """;
        
        using var runner = CreateRunner();
        (long errorCode, string outputString, string errorString) = await runner.ExecutePythonCode(input);
        Assert.True(errorCode == 0);
        Assert.NotEmpty(outputString);
        Assert.Empty(errorString);
    }

    [Fact(Timeout =  60_000)]
    public async Task PythonRunner_pythonCode_CannotInternet()
    {
        string python = """
                        import os
                        
                        res = os.popen("curl google.com").read()
                        print(res)
                        res = os.popen("ipconfig").read()
                        print(res)
                        res = os.popen("ip addr").read()
                        print(res)
                        """;
        using var runner = CreateRunner();
        (long errorCode, string outputString, string errorString) = await runner.ExecutePythonCode(python);
        Assert.NotEmpty(errorString);
    }
}
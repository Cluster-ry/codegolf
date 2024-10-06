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

    private void WriteResults(string pythonCode, string outputString, string errorString)
    {
        _testOutputHelper.WriteLine("Input:");
        _testOutputHelper.WriteLine(pythonCode);
        _testOutputHelper.WriteLine("Output:");
        _testOutputHelper.WriteLine(outputString);
        _testOutputHelper.WriteLine("Error:");
        _testOutputHelper.WriteLine(errorString);
    }

    //TODO FIX TEST
    [Fact(Timeout = 10000)]
    public async Task PythonRunner_pythonCode_Timeout()
    {
        string input = """
                       print("Hello world")
                       i = 0
                       while 1:
                           i + 1
                       print("Done")
                       """;
        try
        {
            var runner = CreateRunner();
            (string outputString, string errorString) = await runner.ExecutePrintingPythonCode(input);
            WriteResults(input, outputString, errorString);
        }
        catch (Exception ex)
        {
            Assert.IsType<TimeoutException>(ex);
            return;
        }
        Assert.Fail("Test should throw timeout error");
    }

    [Fact]
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
        var runner = CreateRunner();
        (string outputString, string errorString) = await runner.ExecutePrintingPythonCode(input);
        WriteResults(input, outputString, errorString);
        Assert.True(true);
    }

    //TODO FIX TEST
    [Fact]
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
        try
        {
            var runner = CreateRunner();
            (string outputString, string errorString) = await runner.ExecutePrintingPythonCode(input);
            WriteResults(input, outputString, errorString);
        }
        catch{
            Assert.Fail();
        }

        Assert.True(true);
    }

    [Fact]
    public async Task PythonRunner_pythonCode_sanity()
    {
        string target = "Hello World";
        string input = $"""print("{target}")""";
        var runner = CreateRunner();
        (string output, string error) = await runner.ExecutePrintingPythonCode(input);
        WriteResults(input, output, error);
        
        Assert.Equal($"{target}\n", output);
    }
}
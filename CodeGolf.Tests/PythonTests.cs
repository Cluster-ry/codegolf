using System.Diagnostics;
using Microsoft.CSharp.RuntimeBinder;
using Xunit.Abstractions;

namespace CodeGolf.Tests;
using CodeGolf.Python;
public class PythonTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PythonTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async void PythonRunner_pythonCode_sanity()
    {
        string input = "1 + 1";
        string output = await PythonRunner.ExecutePythonCode(input);
        _testOutputHelper.WriteLine($"Input: {input} => {output}");
        Assert.Equal("2", output);
    }

    [Fact]
    public async void PythonRunner_pythonCode_timeout()
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
            string output = await PythonRunner.ExecutePythonCode(input);
        }
        catch (Exception ex)
        {
            Assert.IsType<TimeoutException>(ex);
            return;
        }
        Assert.Fail("Test should throw timeout error");
    }

    [Fact]
    public async void PythonRunner_pythonCode_TryToCreateFiles()
    {
        string input = """
                     L = ["This is Delhi \n", "This is Paris \n", "This is London \n"]
                     
                     # Writing to file
                     with open("myfile.txt", "w") as file1:
                         # Writing data to a file
                         file1.write("Hello \n")
                         file1.writelines(L)
                     """;
        try
        {
            string output = await PythonRunner.ExecutePythonCode(input);
        }
        catch (Exception e)
        {
            Assert.IsType<RuntimeBinderException>(e);
            return;
        }

        Assert.Fail("Test should throw exception.");
    }

    [Fact]
    public async void PythonRunner_pythonCodePrintStr_returnsString()
    {
        string target = "Hello World";
        string input = $"""print("{target}")""";
        (string output, string error) = await PythonRunner.ExecutePrintingPythonCode(input);
        _testOutputHelper.WriteLine("From interop:");
        _testOutputHelper.WriteLine("Output:");
        _testOutputHelper.WriteLine(output);
        _testOutputHelper.WriteLine("Error");
        _testOutputHelper.WriteLine(error);
        Assert.Equal($"{target}\n", output);
    }
    
}
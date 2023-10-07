using System.Diagnostics;
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

    [Fact(Timeout = 6000)]
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
}
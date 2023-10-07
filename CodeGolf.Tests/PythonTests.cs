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
    public void PythonRunner_pythonCode_sanity()
    {
        string input = "1 + 1";
        PythonRunner runner = new PythonRunner();
        dynamic output = runner.ExecutePythonCode(input);
        _testOutputHelper.WriteLine($"Input: {input} => {output}");
        _testOutputHelper.WriteLine("typeof output {0}", output.GetType().ToString());
        Assert.Equal("2", output);
    }
}
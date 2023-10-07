using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace CodeGolf.Python;

public static class PythonRunner
{
    public static Task<string> ExecutePythonCode(string pythonCode, CancellationToken token = default)
    {
        TimeSpan maxTimeToProcess = TimeSpan.FromSeconds(5);
        return Task.Run(() => executePythonCode(pythonCode), token).WaitAsync(maxTimeToProcess, token);
    }

    private static Task<String> executePythonCode(String pythonCode)
    {
        var engine = IronPython.Hosting.Python.CreateEngine();

        // Set up search paths to include your virtual environment's site-packages
        var paths = engine.GetSearchPaths();
        //paths.Add("path/to/your/virtualenv/Lib/site-packages"); // Adjust the path
        engine.SetSearchPaths(paths);
        var scope = engine.CreateScope(); 
        
        dynamic result = engine.Execute(pythonCode, scope);
        return Task.FromResult(result.ToString());
    }
}
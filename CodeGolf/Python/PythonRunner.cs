using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace CodeGolf.Python;

public static class PythonRunner
{
    public static async Task<(string, string)> ExecutePrintingPythonCode(string pythonCode, CancellationToken token = default)
    {
        TimeSpan maxTimeToProcess = TimeSpan.FromSeconds(5);

        var engine = initialiceEngine();
        using var outputStream = new MemoryStream();
        using var errorStream = new MemoryStream();
        
        engine.Runtime.IO.SetOutput(outputStream, new StreamWriter(outputStream));
        engine.Runtime.IO.SetErrorOutput(errorStream, new StreamWriter(errorStream));
        var scope = engine.CreateScope();

        await Task.Run(() => engine.Execute(pythonCode, scope), token).WaitAsync(maxTimeToProcess, token);
        
        var outputReader = new StreamReader(outputStream);
        var errorReader = new StreamReader(errorStream);
        outputStream.Position = 0;
        errorStream.Position = 0;
    
        return (outputReader.ReadToEnd(), errorReader.ReadToEnd());
    }

    private static ScriptEngine initialiceEngine()
    {
        var engine = IronPython.Hosting.Python.CreateEngine();
        
        // Set up search paths to include your virtual environment's site-packages
        var paths = engine.GetSearchPaths();
        //paths.Add("path/to/your/virtualenv/Lib/site-packages"); // Adjust the path
        engine.SetSearchPaths(paths);

        return engine;
    }
}
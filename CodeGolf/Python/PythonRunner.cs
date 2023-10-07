using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Runtime;

namespace CodeGolf.Python;

public static class PythonRunner
{
    public static Task<dynamic> ExecutePythonCode(string pythonCode, CancellationToken token = default)
    {
        TimeSpan maxTimeToProcess = TimeSpan.FromSeconds(5);
        return Task.Run(() => executePythonCode(pythonCode), token).WaitAsync(maxTimeToProcess, token);
    }

    public static Task<(string, string)> ExecutePrintingPythonCode(string pythonCode, CancellationToken token = default)
    {
        TimeSpan maxTimeToProcess = TimeSpan.FromSeconds(5);
        return Task.Run(() => executePrintingPythonCode(pythonCode), token).WaitAsync(maxTimeToProcess, token
        );
    }
    
    private static Task<(String,String)> executePrintingPythonCode(String printingPythonCode)
    {
        var engine = initialiceEngine();
        using var outputStream = new MemoryStream();
        using var errorStream = new MemoryStream();
        
        
        engine.Runtime.IO.SetOutput(outputStream, new StreamWriter(outputStream));
        engine.Runtime.IO.SetErrorOutput(errorStream, new StreamWriter(errorStream));
        var scope = engine.CreateScope();
        
        _ = engine.Execute(printingPythonCode, scope);
        
        var outputReader = new StreamReader(outputStream);
        var errorReader = new StreamReader(errorStream);
        outputStream.Position = 0;
        errorStream.Position = 0;
        
        return Task.FromResult((outputReader.ReadToEnd(), errorReader.ReadToEnd()));
    }

    private static Task<dynamic> executePythonCode(String pythonCode)
    {
        var engine = initialiceEngine();
        var scope = engine.CreateScope(); 
        
        dynamic result = engine.Execute(pythonCode, scope);
        return Task.FromResult(result);
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
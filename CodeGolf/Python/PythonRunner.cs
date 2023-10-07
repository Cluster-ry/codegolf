using IronPython.Hosting;

namespace CodeGolf.Python;

public class PythonRunner
{
    public string ExecutePythonCode(string pythonCode)
    {
        var engine = IronPython.Hosting.Python.CreateEngine();

        // Set up search paths to include your virtual environment's site-packages
        var paths = engine.GetSearchPaths();
        //paths.Add("path/to/your/virtualenv/Lib/site-packages"); // Adjust the path
        engine.SetSearchPaths(paths);
        var scope = engine.CreateScope();

        try
        {
            dynamic result = engine.Execute(pythonCode, scope);
            return result.ToString();
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
}
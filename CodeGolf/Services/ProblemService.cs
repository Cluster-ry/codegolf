using System.Text.Json.Serialization;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using Newtonsoft.Json;

namespace CodeGolf.Services;

public class ProblemService
{
    private readonly DataBaseContext _dataBaseContext = new();

    private void AddProblem(Problem toAdd)
    {
        if (_dataBaseContext.Problems.Any(problem => problem.Name == toAdd.Name))
        {
            return;
        }
        
        Console.WriteLine($"Adding problem: {toAdd}");

        _dataBaseContext.Problems.Add(toAdd);
        _dataBaseContext.SaveChanges();
    }

    public void InitializeProblems()
    {
        using var file = new StreamReader("./problems.json");
        var json = file.ReadToEnd();
        var problems = JsonConvert.DeserializeObject<List<Problem>>(json);

        foreach (var problem in problems)
        {
            AddProblem(problem);
        }
    }
}
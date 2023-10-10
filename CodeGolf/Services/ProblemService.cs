using System.Text.Json.Serialization;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using Newtonsoft.Json;

namespace CodeGolf.Services;

public class ProblemService
{
    private readonly DataBaseContext _dataBaseContext = new();

    public List<Problem> GetAllProblems()
    {
        return _dataBaseContext.Problems.ToList();
    }

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

    public List<Answer> GetHighScores(int problemId)
    {
        return _dataBaseContext.Answers
            .Where(answer => answer.ProblemId == problemId)
            .OrderBy(answer => answer.Code.Length).Reverse() 
            .DistinctBy(answer => answer.GroupId).ToList();
    }
}
using System.Text.Json;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using CodeGolf.Tools;

namespace CodeGolf.Services;

public sealed class ProblemService
{
    private readonly DataBaseContext _dataBaseContext;
    private readonly ILogger<ProblemService> _logger;

    public ProblemService(DataBaseContext dataBaseContext, ILogger<ProblemService> logger)
    {
        _dataBaseContext = dataBaseContext;
        _logger = logger;
    }


    public List<Problem> GetAllProblems()
    {
        return _dataBaseContext.Problems.ToList();
    }

    public Problem? GetProblem(int problemId)
    {
        return _dataBaseContext.Problems.FirstOrDefault(problem => problem.Id == problemId);
    }

    private void AddProblem(Problem toAdd)
    {
        if (_dataBaseContext.Problems.Any(problem => problem.Name == toAdd.Name))
        {
            return;
        }
        
        _logger.AddProblem(toAdd);

        _dataBaseContext.Problems.Add(toAdd);
        _dataBaseContext.SaveChanges();
    }

    public void InitializeProblems()
    {
        using var file = new StreamReader("./problems.json");
        var json = file.ReadToEnd();
        var problems = JsonSerializer.Deserialize<List<Problem>>(json);

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

    public List<Answer> GetLowScores(int problemId)
    {
        return _dataBaseContext.Answers
                    .Where(answer => answer.ProblemId == problemId)
                    .OrderBy(answer => answer.Code.Length) 
                    .DistinctBy(answer => answer.GroupId).ToList();
    }
}
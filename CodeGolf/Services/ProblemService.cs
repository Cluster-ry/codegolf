using System.Text.Json;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using CodeGolf.Tools;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.Services;

public sealed class ProblemService
{
    private readonly IDbContextFactory<DataBaseContext> _dataBaseContextFactory;
    private readonly ILogger<ProblemService> _logger;

    public ProblemService(IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<ProblemService> logger)
    {
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
    }


    public IReadOnlyCollection<Problem> GetAllProblems()
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return [.. context.Problems];
    }

    public Problem? GetProblem(string problemName)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return context.Problems.FirstOrDefault(problem => problem.Name == problemName);
    }

    private void AddProblem(Problem toAdd)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        if (context.Problems.Any(problem => problem.Name == toAdd.Name))
        {
            return;
        }

        _logger.AddProblem(toAdd);
        
        context.Problems.Add(toAdd);
        context.SaveChanges();
    }

    public void InitializeProblems()
    {
        using var file = new StreamReader("./problems.json");
        var json = file.ReadToEnd();
        var problems = JsonSerializer.Deserialize<List<Problem>>(json);

        problems?.ForEach(AddProblem);
    }

    public IReadOnlyCollection<Answer> GetHighScores(int problemId, bool fux)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return context.Answers
            .Where(answer => answer.ProblemId == problemId)
            .Where(answer => answer.Group.IsFresher == fux)
            .OrderByDescending(answer => answer.Code.Length)
            .DistinctBy(answer => answer.GroupId)
            .ToList();
    }

    public IReadOnlyCollection<Answer> GetLowScores(int problemId, bool fux)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return context.Answers
            .Where(answer => answer.ProblemId == problemId)
            .Where(answer => answer.Group.IsFresher == fux)
            .OrderBy(answer => answer.Code.Length) 
            .DistinctBy(answer => answer.GroupId)
            .ToList();
    }
}
using System.Text.Json;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using CodeGolf.Tools;
using CodeGolf.Python;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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


    public IReadOnlyCollection<String> GetAllProblems()
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return [.. context.Problems.Select(problem => problem.Name)];
    }

    public Problem? GetProblem(string? problemName)
    {
        if(problemName is null) return null;

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

    public IReadOnlyCollection<(string, int)> GetHighScores(string problemName, bool fux)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return [.. context.Answers
            .Include(answer => answer.Group)
            .Include(answer => answer.Group.GroupMembers)
            .Include(answer => answer.Problem)
            .Where(answer => answer.Problem.Name == problemName && answer.Passed == true && answer.Group.IsFresher == fux)
            .GroupBy(answer => answer.GroupId)
            .Select(grouping => grouping.OrderBy(answer => answer.Code.Length).First())
            .AsEnumerable()
            .OrderByDescending(answer => answer.Code.Length)
            .Select(answer => new ValueTuple<string, int>(answer.Group.Name, answer.Code.Length))];
    }

    public IReadOnlyCollection<(string, int)> GetLowScores(string problemName, bool fux)
    {
        using var context =_dataBaseContextFactory.CreateDbContext();
        return [.. context.Answers
            .Include(answer => answer.Group)
            .Include(answer => answer.Group.GroupMembers)
            .Include(answer => answer.Problem)
            .Where(answer => answer.Problem.Name == problemName && answer.Passed == true && answer.Group.IsFresher == fux)
            .GroupBy(answer => answer.GroupId)
            .Select(grouping => grouping.OrderBy(answer => answer.Code.Length).First())
            .AsEnumerable()
            .OrderBy(answer => answer.Code.Length) 
            .Select(answer => new ValueTuple<string, int>(answer.Group.Name, answer.Code.Length))];
    }

    
}
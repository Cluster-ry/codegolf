using CodeGolf.Python;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using CodeGolf.Tools;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.Services;

public sealed class EvaluateService
{
    private readonly PythonRunner _pythonRunner;
    private readonly ProblemService _problemService;
    private readonly GroupService _groupService;
    private readonly IDbContextFactory<DataBaseContext> _dataBaseContextFactory;
    private readonly ILogger<ProblemService> _logger;

    public EvaluateService(PythonRunner pythonRunner, ProblemService problemService, GroupService groupService, IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<ProblemService> logger)
    {
        _pythonRunner = pythonRunner;
        _problemService = problemService;
        _groupService = groupService;
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
    }

    public async Task<(bool success, string exampleOutput, string proposedOutput)> TestSolution(string groupName, string problemName, string solution)
    {
        var problem = _problemService.GetProblem(problemName);
        ArgumentNullException.ThrowIfNull(problem);
        var group = _groupService.GetGroup(groupName);
        ArgumentNullException.ThrowIfNull(group);

        _logger.NewSubmission(groupName, problemName, solution);

        var proposedTask = _pythonRunner.ExecutePythonCode(solution, problem.Input ?? "");
        var exampleTask = _pythonRunner.ExecutePythonCode(problem.ExampleSolution, problem.Input ?? "");
        var results = await Task.WhenAll(proposedTask, exampleTask).ConfigureAwait(false);

        var (proposedCode, proposedOutput, proposedException) = results[0];
        var (exampleCode, exampleOutput, exampleException) = results[1];

        if(exampleCode != 0) throw new Exception("Error in example code");
        if(proposedCode != 0) return (false, exampleOutput, proposedException);

        var sameOutput = String.Equals(exampleOutput, proposedOutput, StringComparison.Ordinal);

        UpdateSubmission(solution, sameOutput, group.Id, problem.Id);
        return (sameOutput, exampleOutput, proposedOutput);
    }

    private void UpdateSubmission(string solutionCode, bool passed, int groupId, int problemId)
    {
        using var context = _dataBaseContextFactory.CreateDbContext();
        var answer = new Answer(){
            Code = solutionCode,
            Passed = passed,
            GroupId = groupId,
            ProblemId = problemId
        };

        context.Add<Answer>(answer);
        context.SaveChanges();
    }
}
using CodeGolf.Python;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
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
        ArgumentNullException.ThrowIfNull(group, nameof(problem));

        _logger.LogInformation("New submission from group {group} problem {problem}\n{solution}", groupName, problemName, solution);

        var (proposedCode, proposedOutput, proposedException) = await _pythonRunner.ExecutePythonCode(solution, problem.Input ?? "").ConfigureAwait(false);
        var (exampleCode, exampleOutput, exampleException) = await _pythonRunner.ExecutePythonCode(problem.ExampleSolution, problem.Input ?? "").ConfigureAwait(false);
        if(exampleCode != 0) throw new Exception("Error in example code");
        if(proposedCode != 0) return (false, exampleOutput, proposedException);

        var sameOutput = String.Equals(exampleOutput, proposedOutput, StringComparison.InvariantCulture);

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
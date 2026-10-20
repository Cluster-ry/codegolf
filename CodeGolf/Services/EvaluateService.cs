using System.Collections.Concurrent;
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
    private readonly ILogger<EvaluateService> _logger;
    private readonly ILogger<UserActivityLog> _userActivity;

    public EvaluateService(PythonRunner pythonRunner, ProblemService problemService, GroupService groupService, IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<EvaluateService> logger, ILogger<UserActivityLog> userActivity)
    {
        _pythonRunner = pythonRunner;
        _problemService = problemService;
        _groupService = groupService;
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
        _userActivity = userActivity;
    }

    public async Task LoadExampleResults(CancellationToken token = default)
    {
        await using var dbContext = await _dataBaseContextFactory.CreateDbContextAsync(token).ConfigureAwait(false);

        List<Task> tasks = [];
        await foreach (var problem in dbContext.Problems)
        {
            tasks.Add(Task.Run(async () =>
            {
                var (code, output, exception) = await _pythonRunner.ExecutePythonCode(problem.ExampleSolution, problem.Input ?? "", token).ConfigureAwait(false);
                if(code != 0 || !string.IsNullOrWhiteSpace(exception)) throw new Exception($"Failed to run example solutions Code={code} Exception={exception}");
                problem.ExampleOutput = output;
            }, token));
        }
        
        await Task.WhenAll(tasks).ConfigureAwait(false);
        await dbContext.SaveChangesAsync(token).ConfigureAwait(false);
    }

    public async Task<(bool success, string proposedOutput)> TestSolution(string groupName, string problemName, string solution, CancellationToken token = default)
    {
        var problem = _problemService.GetProblem(problemName);
        ArgumentNullException.ThrowIfNull(problem);

        if (_groupService.IsBlocked(groupName))
        {
            _userActivity.UserSubmissionRejected(groupName, problemName, "group is blocked", solution.Length);
            return (false, GroupService.BlockedSubmissionMessage);
        }

        var group = _groupService.GetGroup(groupName);
        if (group is null)
        {
            _userActivity.UserSubmissionWithoutGroup(problemName, solution.Length);
            return (false, "Register your group on the home page before submitting solutions.");
        }

        _logger.SubmissionCode(groupName, problemName, solution.Length, solution);
        var (proposedCode, proposedOutput, proposedException) = await _pythonRunner.ExecutePythonCode(solution, problem.Input ?? "", token).ConfigureAwait(false);
        
        if(proposedCode != 0)
        {
            _userActivity.UserSubmissionEvaluated(groupName, problemName, solution.Length, passed: false);
            return (false, proposedException);
        }

        var sameOutput = String.Equals(problem.ExampleOutput, proposedOutput, StringComparison.Ordinal);

        UpdateSubmission(solution, sameOutput, group.Id, problem.Id);
        _userActivity.UserSubmissionEvaluated(groupName, problemName, solution.Length, sameOutput);
        return (sameOutput, proposedOutput);
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
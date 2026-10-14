using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.ComponentModel.DataAnnotations;
using CodeGolf.Services;


namespace CodeGolf.Pages;

public sealed class ProblemHandler
{
    private readonly EvaluateService evaluateService;
    private readonly ProblemService problemService;
    private readonly GroupHandler groupHandler;
    private readonly ProtectedLocalStorage localStorage;
    private readonly ILogger<ProblemHandler> logger;

    public ProblemHandler(EvaluateService evaluateService, ProblemService problemService, GroupHandler groupHandler, ProtectedLocalStorage localStorage, ILogger<ProblemHandler> logger)
    {
        this.evaluateService = evaluateService;
        this.problemService = problemService;
        this.groupHandler = groupHandler;
        this.localStorage = localStorage;
        this.logger = logger;
    }

    public ProblemDefinition? LoadDefinition(string problemName)
    {
        var problem = problemService.GetProblem(problemName);

        if(problem is null) {
            logger.LogWarning("Failed to load problem with {Key}. Is the key faulty?", problemName);
            return null;
        }

        logger.LogInformation("Loaded problem {Problem}", problem);
        return new ProblemDefinition(problem);
    }

    public async Task<string> LoadSubmission(string problemName)
    {
        var value = await ProtectedLocalStorageHelpers.TryGetAsync<string>(
            localStorage, StorageAnswerKey(problemName), logger);
        return value ?? string.Empty;
    }

    public async Task SaveSubmission(string problemName, string submission)
    {
        await localStorage.SetAsync(StorageAnswerKey(problemName), submission);
    }

    public async Task<(bool Success, string Output)> EvaluateSubmission(string problemName, string submission)
    {
        var groupInfo = await groupHandler.Load();
        if (groupInfo is null || string.IsNullOrWhiteSpace(groupInfo.Name))
        {
            return (false, "Register your group on the home page before submitting solutions.");
        }

        return await evaluateService.TestSolution(groupInfo.Name, problemName, submission);
    } 

    private static string StorageAnswerKey(string problemName) => $"RecentProblemAnswer_{problemName}";    
}


public sealed record ProblemSubmission
{
    public required string ProblemName {get; init;} 
    
    public string CurrentSolution {get; set;} = "";
    public string CurrentOutput {get; set;} = "";
    public bool Success {get; set;}
    public int SubmissionLength => CurrentSolution.Length;
}

public sealed record ProblemDefinition
{
    public string ProblemName {get; init;}
    public string Title {get; private set;} = "Problem Title";
    public string Explanation {get; private set;} = "No problem loaded";
    public string ExampleOutput {get; private set;} = "";


    public ProblemDefinition(DataBase.Entities.Problem dbProblem)
    {
        ArgumentNullException.ThrowIfNull(dbProblem);

        ProblemName = dbProblem.Name;
        Title = dbProblem.Name;
        Explanation = dbProblem.Explanation;
        ExampleOutput = dbProblem.ExampleOutput;
    }
}

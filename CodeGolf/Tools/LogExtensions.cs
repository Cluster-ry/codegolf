using CodeGolf.DataBase.Entities;
using Docker.DotNet.Models;

namespace CodeGolf.Tools;

public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Adding problem: `{Problem}`")]
    public static partial void AddProblem(this ILogger logger, Problem problem);

    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = """
                                                                  Command:
                                                                  `{Command}`
                                                                  StdIn:
                                                                  `{Stdin}`
                                                                  StdOut:
                                                                  `{Stdout}`
                                                                  StdErr:
                                                                  `{Stderr}`
                                                                  """)]
    public static partial void ContainerCommand(this ILogger logger, string command, string stdin, string stdout, string stderr);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Loading image `{ImageUri}`")]
    public static partial void LoadImage(this ILogger logger, string imageUri);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Loaded image `{ImageUri}`")]
    public static partial void LoadImageDone(this ILogger logger, string imageUri);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Created Container `{ImageUri}` Id=`{ContainerId}`")]
    public static partial void CreateContainer(this ILogger logger, string imageUri, string containerId);
    
    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "StartedContainer `{containerId}` with stdin=`{stdin}`")]
    public static partial void StartContainer(this ILogger logger, string containerId, string stdin);

    [LoggerMessage(EventId = 6, Level = LogLevel.Debug, Message = "Container `{containerId}` Stats:\n`{stats}`")]
    public static partial void ContainerStats(this ILogger logger, string containerId, string stats);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Removing container `{containerId}`")]
    public static partial void RemoveContainer(this ILogger logger, string containerId);
    
    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Failed to load container client `{uri}`")]
    public static partial void ContainerClientLoadFailed(this ILogger logger, string uri, Exception ex);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Container `{containerId}` timed out `{maxTime}`")]
    public static partial void ContainerTimeout(this ILogger logger, string containerId, TimeSpan maxTime, TimeoutException ex);
    
    [LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Submission code from {Group} on {Problem} ({Length} chars):\n{Solution}")]
    public static partial void SubmissionCode(this ILogger logger, string group, string problem, int length, string solution);

    [LoggerMessage(EventId = 100, Level = LogLevel.Information, Message = "[User] Navigated to {Path}")]
    public static partial void UserPageView(this ILogger logger, string path);

    [LoggerMessage(EventId = 101, Level = LogLevel.Information, Message = "[User] Restored saved group session: {GroupName} (fresher={IsFresher})")]
    public static partial void UserSessionRestored(this ILogger logger, string groupName, bool isFresher);

    [LoggerMessage(EventId = 102, Level = LogLevel.Information, Message = "[User] Registering group {GroupName} (fresher={IsFresher}, members={MemberCount})")]
    public static partial void UserRegisteringGroup(this ILogger logger, string groupName, bool isFresher, int memberCount);

    [LoggerMessage(EventId = 103, Level = LogLevel.Information, Message = "[User] Group registration succeeded: {GroupName}")]
    public static partial void UserGroupRegistered(this ILogger logger, string groupName);

    [LoggerMessage(EventId = 104, Level = LogLevel.Information, Message = "[User] Group registration failed for {GroupName}: {Reason}")]
    public static partial void UserGroupRegistrationFailed(this ILogger logger, string groupName, string reason);

    [LoggerMessage(EventId = 105, Level = LogLevel.Information, Message = "[User] {GroupName} submitted solution for {Problem}: {Length} chars, passed={Passed}")]
    public static partial void UserSubmissionEvaluated(this ILogger logger, string groupName, string problem, int length, bool passed);

    [LoggerMessage(EventId = 106, Level = LogLevel.Information, Message = "[User] Submission rejected (not registered): problem={Problem}, {Length} chars")]
    public static partial void UserSubmissionWithoutGroup(this ILogger logger, string problem, int length);

    [LoggerMessage(EventId = 107, Level = LogLevel.Information, Message = "[User] Viewing leaderboards ({ProblemCount} problems tracked)")]
    public static partial void UserViewingLeaderboards(this ILogger logger, int problemCount);
}

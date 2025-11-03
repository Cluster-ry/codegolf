using CodeGolf.DataBase.Entities;
using Docker.DotNet.Models;

namespace CodeGolf.Tools;

public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Adding problem: `{Problem}`")]
    public static partial void AddProblem(this ILogger logger, Problem problem);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = """
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

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Created Container `{ImageUri}` Id=`{ContainerId}`")]
    public static partial void CreateContainer(this ILogger logger, string imageUri, string containerId);
    
    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "StartedContainer `{containerId}` with stdin=`{stdin}`")]
    public static partial void StartContainer(this ILogger logger, string containerId, string stdin);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Container `{containerId}` Stats:\n`{stats}`")]
    public static partial void ContainerStats(this ILogger logger, string containerId, string stats);

    [LoggerMessage(EventId = 7, Level = LogLevel.Information, Message = "Removing container `{containerId}`")]
    public static partial void RemoveContainer(this ILogger logger, string containerId);
    
    [LoggerMessage(EventId = 8, Level = LogLevel.Error, Message = "Failed to load container client `{uri}`")]
    public static partial void ContainerClientLoadFailed(this ILogger logger, string uri, Exception ex);

    [LoggerMessage(EventId = 9, Level = LogLevel.Information, Message = "Container `{containerId}` timed out `{maxTime}`")]
    public static partial void ContainerTimeout(this ILogger logger, string containerId, TimeSpan maxTime, TimeoutException ex);
    
    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "New submission from group {Group} problem {Problem}\n{Solution}")]
    public static partial void NewSubmission(this ILogger logger, string group, string problem, string solution);
}

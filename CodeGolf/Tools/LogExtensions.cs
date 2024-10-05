using CodeGolf.DataBase.Entities;

namespace CodeGolf.Tools;

public static partial class Log
{
    [LoggerMessage(EventId = 0, Level = LogLevel.Information, Message = "Adding problem: `{Problem}`")]
    public static partial void AddProblem(this ILogger logger, Problem problem);


}

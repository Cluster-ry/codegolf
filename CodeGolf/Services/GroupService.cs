using CodeGolf.Pages;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using CodeGolf.Tools;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.Services;

public enum GroupRegistrationStatus
{
    Success,
    AlreadyExists,
    Blocked,
    Failed,
}

public enum GroupRemovalStatus
{
    Removed,
    NotFound,
    Failed,
}

public readonly record struct GroupRegistrationResult(GroupRegistrationStatus Status, string Message);

public readonly record struct GroupRemovalResult(GroupRemovalStatus Status, string Message);

public sealed record GroupSummary(int Id, string Name, bool IsFresher, IReadOnlyList<string> Members, int PassingAnswers);

public sealed class GroupService
{
    public const string BlockedSubmissionMessage = "This group has been removed and can no longer submit solutions.";

    private readonly IDbContextFactory<DataBaseContext> _dataBaseContextFactory;
    private readonly ILogger<GroupService> _logger;
    private readonly ILogger<UserActivityLog> _userActivity;
    private readonly GroupBlockList _blockList;

    public GroupService(IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<GroupService> logger, ILogger<UserActivityLog> userActivity, GroupBlockList blockList)
    {
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
        _userActivity = userActivity;
        _blockList = blockList;
    }

    public static string BlockedRegistrationMessage(string name)
        => $"The group name \"{name}\" has been removed and cannot be registered again.";


    public GroupRegistrationResult RegisterGroup(GroupSubmission groupSubmission)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(groupSubmission?.Name);

        var name = groupSubmission.Name.Trim();

        if (_blockList.IsBlocked(name))
        {
            _userActivity.UserGroupRegistrationFailed(name, "name is blocked");
            return new GroupRegistrationResult(GroupRegistrationStatus.Blocked, BlockedRegistrationMessage(name));
        }

        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        if (databaseContext.Groups.Any(group => group.Name == name))
        {
            _userActivity.UserGroupRegistrationFailed(name, "name already registered");
            return new GroupRegistrationResult(
                GroupRegistrationStatus.AlreadyExists,
                $"A group named \"{name}\" is already registered. Choose a different name.");
        }

        var group = new Group()
        {
            Name = name,
            IsFresher = groupSubmission.IsFresher,
        };

        if (groupSubmission.GroupMember0 is not null) group.GroupMembers.Add(new GroupMember()
        {
            Name = groupSubmission.GroupMember0,
        });

        if (groupSubmission.GroupMember1 is not null) group.GroupMembers.Add(new GroupMember()
        {
            Name = groupSubmission.GroupMember1,
        });

        if (groupSubmission.GroupMember2 is not null) group.GroupMembers.Add(new GroupMember()
        {
            Name = groupSubmission.GroupMember2,
        });

        try
        {
            databaseContext.Add(group);
            databaseContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register group {GroupName}", name);
            _userActivity.UserGroupRegistrationFailed(name, "save failed");
            return new GroupRegistrationResult(
                GroupRegistrationStatus.Failed,
                "Something went wrong while saving your group. Please try again.");
        }

        _userActivity.UserGroupRegistered(name);
        return new GroupRegistrationResult(
            GroupRegistrationStatus.Success,
            $"Group \"{name}\" is registered. You can start submitting solutions.");
    }


    public Group? GetGroup(int id)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups.Find(id);
    }

    public Group? GetGroup(string name)
    {

        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups.FirstOrDefault(group => group.Name == name);
    }

    public bool IsBlocked(string name) => _blockList.IsBlocked(name);

    public IReadOnlyList<GroupSummary> ListGroups()
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups
            .AsNoTracking()
            .Include(group => group.GroupMembers)
            .Include(group => group.Answers)
            .AsEnumerable()
            .Select(group => new GroupSummary(
                group.Id,
                group.Name,
                group.IsFresher,
                group.GroupMembers
                    .Select(member => member.Name)
                    .Where(memberName => !string.IsNullOrWhiteSpace(memberName))
                    .ToArray(),
                group.Answers.Count(answer => answer.Passed)))
            .OrderBy(summary => summary.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<string> ListBlockedNames() => _blockList.Names();

    public GroupRemovalResult RemoveGroup(int id)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        var group = databaseContext.Groups
            .Include(registered => registered.GroupMembers)
            .Include(registered => registered.Answers)
            .FirstOrDefault(registered => registered.Id == id);

        if (group is null)
        {
            return new GroupRemovalResult(GroupRemovalStatus.NotFound, "That group is no longer registered.");
        }

        var name = group.Name;
        try
        {
            databaseContext.Answers.RemoveRange(group.Answers);
            databaseContext.GroupMembers.RemoveRange(group.GroupMembers);
            databaseContext.Groups.Remove(group);
            databaseContext.SaveChanges();
        }
        catch (DbUpdateException ex)
        {
            return RemovalFailed(name, ex);
        }
        catch (InvalidOperationException ex)
        {
            return RemovalFailed(name, ex);
        }

        _userActivity.UserGroupRemoved(name);

        try
        {
            _blockList.Block(name);
        }
        catch (IOException ex)
        {
            return BlockSaveFailed(name, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BlockSaveFailed(name, ex);
        }

        return new GroupRemovalResult(
            GroupRemovalStatus.Removed,
            $"Group \"{name}\" is removed. That name cannot be registered again.");
    }

    public bool Unblock(string name)
    {
        var unblocked = _blockList.Unblock(name);
        if (unblocked)
        {
            _userActivity.UserGroupUnblocked(name.Trim());
        }

        return unblocked;
    }

    private GroupRemovalResult RemovalFailed(string name, Exception exception)
    {
        _logger.GroupRemovalFailed(exception, name);
        return new GroupRemovalResult(
            GroupRemovalStatus.Failed,
            "Something went wrong while removing the group. Please try again.");
    }

    private GroupRemovalResult BlockSaveFailed(string name, Exception exception)
    {
        _logger.GroupBlockAfterRemovalFailed(exception, name);
        return new GroupRemovalResult(
            GroupRemovalStatus.Failed,
            $"Group \"{name}\" was removed, but the name could not be saved to the block list.");
    }
}
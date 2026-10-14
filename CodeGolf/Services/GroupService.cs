using CodeGolf.Pages;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.Services;

public enum GroupRegistrationStatus
{
    Success,
    AlreadyExists,
    Failed,
}

public readonly record struct GroupRegistrationResult(GroupRegistrationStatus Status, string Message);

public sealed class GroupService
{
    private readonly IDbContextFactory<DataBaseContext> _dataBaseContextFactory;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<GroupService> logger)
    {
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
    }


    public GroupRegistrationResult RegisterGroup(GroupSubmission groupSubmission)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(groupSubmission?.Name);

        var name = groupSubmission.Name.Trim();

        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        if (databaseContext.Groups.Any(group => group.Name == name))
        {
            _logger.LogInformation("Group registration rejected: name {GroupName} already exists", name);
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
            return new GroupRegistrationResult(
                GroupRegistrationStatus.Failed,
                "Something went wrong while saving your group. Please try again.");
        }

        _logger.LogInformation("Registered group {GroupName}", name);
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
}
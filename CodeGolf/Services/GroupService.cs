using CodeGolf.Pages;
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.Services;

public sealed class GroupService
{
    private readonly IDbContextFactory<DataBaseContext> _dataBaseContextFactory;
    private readonly ILogger<GroupService> _logger;

    public GroupService(IDbContextFactory<DataBaseContext> dataBaseContextFactory, ILogger<GroupService> logger)
    {
        _dataBaseContextFactory = dataBaseContextFactory;
        _logger = logger;
    }


    public void RegisterGroup(GroupSubmission groupSubmission)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        var group = new Group(){
            Name = groupSubmission.Name,
            IsFresher = groupSubmission.IsFresher,
        };

        if(groupSubmission.GroupMember0 is not null) group.GroupMembers.Add(new GroupMember(){
            Name = groupSubmission.GroupMember0,
        });

        if(groupSubmission.GroupMember1 is not null) group.GroupMembers.Add(new GroupMember(){
            Name = groupSubmission.GroupMember1,
        });

        if(groupSubmission.GroupMember2 is not null) group.GroupMembers.Add(new GroupMember(){
            Name = groupSubmission.GroupMember2,
        });

        databaseContext.Add(group);
        databaseContext.SaveChanges();
    }


    public Group? GetGroup(int id)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups.Find(id);
    }

    public Group? GetGroup(string name)
    {

        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups.First((group) => group.Name == name);
    }
}
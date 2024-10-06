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


    public Group RegisterGroup(Group group)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        databaseContext.Add(group);
        databaseContext.SaveChanges();
        return group;
    }

    public Group? GetGroup(int id)
    {
        using var databaseContext = _dataBaseContextFactory.CreateDbContext();
        return databaseContext.Groups.Find(id);
    }
}
using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;

namespace CodeGolf.Services;

public sealed class GroupService
{
    private readonly DataBaseContext _dataBaseContext;
    private readonly ILogger<GroupService> _logger;

    public GroupService(DataBaseContext dataBaseContext, ILogger<GroupService> logger)
    {
        _dataBaseContext = dataBaseContext;
        _logger = logger;
    }


    public Group RegisterGroup(Group group)
    {
        _dataBaseContext.Add(group);
        _dataBaseContext.SaveChanges();
        return group;
    }

    public Group? GetGroup(int id)
    {
        return _dataBaseContext.Groups.Find(id);
    }
}
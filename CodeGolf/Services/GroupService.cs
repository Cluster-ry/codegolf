using CodeGolf.DataBase;
using CodeGolf.DataBase.Entities;

namespace CodeGolf.Services;

public class GroupService
{
    private readonly DataBaseContext _dataBaseContext = new();

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
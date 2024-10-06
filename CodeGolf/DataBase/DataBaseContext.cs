using CodeGolf.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.DataBase;

public class DataBaseContext : DbContext
{
    public DbSet<Group> Groups { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Problem> Problems { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }

    public DataBaseContext(DbContextOptions<DataBaseContext> options): base(options)
    {
    }

}
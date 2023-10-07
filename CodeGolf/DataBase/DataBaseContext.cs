using CodeGolf.DataBase.Entities;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.DataBase;

public class DataBaseContext : DbContext
{
    private string DbPath { get; }
    
    public DbSet<Group> Groups { get; set; }
    public DbSet<Answer> Answers { get; set; }
    public DbSet<Problem> Problems { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }

    public DataBaseContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "main.db");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite($"Data Source={DbPath}");
}
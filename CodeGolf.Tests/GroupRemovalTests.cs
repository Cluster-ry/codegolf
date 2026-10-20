using CodeGolf.DataBase;
using CodeGolf.Pages;
using CodeGolf.Services;
using CodeGolf.Tools;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace CodeGolf.Tests;

public sealed class GroupRemovalTests : IDisposable
{
    private readonly string _directory;
    private readonly string _blockFile;
    private readonly SqliteConnection _connection;
    private readonly GroupBlockList _blockList;
    private readonly GroupService _groups;
    private readonly SqliteContextFactory _factory;

    public GroupRemovalTests()
    {
        _directory = Path.Combine(Path.GetTempPath(), "codegolf-group-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_directory);
        _blockFile = Path.Combine(_directory, "banned-groups.json");
        _blockList = new GroupBlockList(_blockFile, NullLogger<GroupBlockList>.Instance);

        var databaseName = $"groups-{Guid.NewGuid():N}";
        _connection = new SqliteConnection($"Data Source={databaseName};Mode=Memory;Cache=Shared");
        _connection.Open();
        var options = new DbContextOptionsBuilder<DataBaseContext>().UseSqlite(_connection).Options;
        using (var setup = new DataBaseContext(options))
        {
            setup.Database.EnsureCreated();
        }

        _factory = new SqliteContextFactory(options);
        _groups = new GroupService(
            _factory,
            NullLogger<GroupService>.Instance,
            NullLogger<UserActivityLog>.Instance,
            _blockList);
    }

    [Fact]
    public void RemoveGroup_DeletesScoresAndBlocksTheName()
    {
        var registered = _groups.RegisterGroup(Sample("North", fresher: true));
        Assert.Equal(GroupRegistrationStatus.Success, registered.Status);

        var summary = Assert.Single(_groups.ListGroups());
        AddPassingAnswer(summary.Id);

        var removed = _groups.RemoveGroup(summary.Id);

        Assert.Equal(GroupRemovalStatus.Removed, removed.Status);
        Assert.Empty(_groups.ListGroups());
        Assert.Equal("North", Assert.Single(_groups.ListBlockedNames()));
        Assert.Null(_groups.GetGroup("North"));
        Assert.True(_groups.IsBlocked("north"));
        using (var context = _factory.CreateDbContext())
        {
            Assert.Empty(context.Answers);
            Assert.Empty(context.GroupMembers);
        }

        var again = _groups.RegisterGroup(Sample(" north "));
        Assert.Equal(GroupRegistrationStatus.Blocked, again.Status);
        Assert.Equal(GroupService.BlockedRegistrationMessage("north"), again.Message);

        var reloaded = new GroupBlockList(_blockFile, NullLogger<GroupBlockList>.Instance);
        Assert.Equal("North", Assert.Single(reloaded.Names()));
    }

    [Fact]
    public void Unblock_AllowsTheNameToRegisterAgain()
    {
        var summary = Register("West");
        Assert.Equal(GroupRemovalStatus.Removed, _groups.RemoveGroup(summary.Id).Status);

        Assert.True(_groups.Unblock(" west "));
        Assert.Empty(_groups.ListBlockedNames());

        var again = _groups.RegisterGroup(Sample("West"));
        Assert.Equal(GroupRegistrationStatus.Success, again.Status);
    }

    [Fact]
    public void RemoveGroup_UnknownId_DoesNotBlockAnything()
    {
        var result = _groups.RemoveGroup(404);

        Assert.Equal(GroupRemovalStatus.NotFound, result.Status);
        Assert.Empty(_groups.ListBlockedNames());
    }

    [Fact]
    public void BlockList_CorruptFile_Throws()
    {
        File.WriteAllText(_blockFile, "{not-json");

        var exception = Assert.Throws<InvalidDataException>(() => new GroupBlockList(_blockFile, NullLogger<GroupBlockList>.Instance));
        Assert.Contains(_blockFile, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AdminAccess_AcceptsOnlyTheConfiguredPassword()
    {
        var access = new AdminAccess(Config("secret"));

        Assert.True(access.IsConfigured);
        Assert.True(access.Check("secret"));
        Assert.False(access.Check("Secret"));
        Assert.False(access.Check(""));
        Assert.False(new AdminAccess(Config("  ")).IsConfigured);
    }

    public void Dispose()
    {
        _connection.Dispose();
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private GroupSummary Register(string name)
    {
        var result = _groups.RegisterGroup(Sample(name));
        Assert.Equal(GroupRegistrationStatus.Success, result.Status);
        return Assert.Single(_groups.ListGroups());
    }

    private void AddPassingAnswer(int groupId)
    {
        using var context = _factory.CreateDbContext();
        var problem = new CodeGolf.DataBase.Entities.Problem
        {
            Name = "Hello",
            Explanation = "Say hello",
            ExampleSolution = "print(1)",
            Input = "",
        };
        context.Problems.Add(problem);
        context.SaveChanges();
        context.Answers.Add(new CodeGolf.DataBase.Entities.Answer
        {
            Code = "print(1)",
            Passed = true,
            GroupId = groupId,
            ProblemId = problem.Id,
        });
        context.SaveChanges();
    }

    private static GroupSubmission Sample(string name, bool fresher = false) => new()
    {
        Name = name,
        GroupMember0 = "Ada",
        GroupMember1 = "Bea",
        IsFresher = fresher,
    };

    private static IConfiguration Config(string password)
        => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Admin:Password"] = password,
        }).Build();

    private sealed class SqliteContextFactory(DbContextOptions<DataBaseContext> options) : IDbContextFactory<DataBaseContext>
    {
        public DataBaseContext CreateDbContext() => new(options);
    }
}

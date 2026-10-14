using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.ComponentModel.DataAnnotations;

namespace CodeGolf.Pages;

public sealed class GroupHandler
{
    private readonly ProtectedLocalStorage localStorage;
    private readonly ILogger<GroupHandler> logger;

    private const string StorageKey = "groupSubmission";

    public GroupHandler(ProtectedLocalStorage localStorage, ILogger<GroupHandler> logger){
        this.localStorage = localStorage;
        this.logger = logger;
    }

    public Task<GroupSubmission?> Load()
        => ProtectedLocalStorageHelpers.TryGetAsync<GroupSubmission>(localStorage, StorageKey, logger);

    public async Task Save(GroupSubmission group)
    {
        await localStorage.SetAsync(StorageKey, group);
    }

    public async Task Clear()
    {
        await localStorage.DeleteAsync(StorageKey);
    }
}


public sealed class GroupSubmission
{
    [Required]
    public string Name { get; set; }
    [Required] 
    public string GroupMember0 { get; set; }
    public string? GroupMember1 { get; set; }
    public string? GroupMember2 { get; set; }
    [Required]
    public bool IsFresher { get; set; }

    public override string ToString()
        => $"Name: {Name}, Fux:{IsFresher}, Members: {GroupMember0}, {GroupMember1}, {GroupMember2}";
}
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.DataBase.Entities;

public class Group
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsFresher { get; set; }

    public List<GroupMember> GroupMembers { get; set; } = new();
    public List<Answer> Answers { get; set; } = new();

    public override string ToString()
    {
        var memberPrint = GroupMembers.Aggregate("", (current, groupMember) => current + $" \"{groupMember.Name}\"");
        return $"Group \"{Name}\", Members: {memberPrint}";
    }
}
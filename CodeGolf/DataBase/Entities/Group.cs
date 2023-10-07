using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeGolf.DataBase.Entities;

public class Group
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }

    public List<GroupMember> GroupMembers { get; } = new List<GroupMember>();
    public List<Answer> Answers { get; } = new List<Answer>();

    public override string ToString()
    {
        var memberPrint = GroupMembers.Aggregate("", (current, groupMember) => current + $" \"{groupMember.Name}\"");
        return $"Group \"{Name}\", Members: {memberPrint}";
    }
}
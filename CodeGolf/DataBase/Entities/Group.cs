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

    public IEnumerable<GroupMember> GroupMembers { get; } = new List<GroupMember>();
    public IEnumerable<Answer> Answers { get; } = new List<Answer>();
}
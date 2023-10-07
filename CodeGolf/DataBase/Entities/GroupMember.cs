using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeGolf.DataBase.Entities;

public class GroupMember
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    
    public int GroupId { get; set; }
    public Group Group { get; set; }

    public override string ToString()
    {
        return $"Member \"{Name}\" of group \"{Group.Name}\"";
    }
}
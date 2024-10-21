using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeGolf.DataBase.Entities;

public class Answer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Code { get; set; }
    public bool Passed { get; set; } = false;

    [ForeignKey(nameof(Group))]
    public int GroupId { get; set; }
    public Group Group { get; set; }

    [ForeignKey(nameof(Problem))]
    public int ProblemId { get; set; }
    public Problem Problem { get; set; }

    public override string ToString()
    {
        return $"Answer to problem \"{Problem.Name}\" by group \"{Group.Name}\" with length of {Code.Length} id {Id}";
    }
}
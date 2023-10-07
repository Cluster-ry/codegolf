using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CodeGolf.DataBase.Entities;

public class Answer
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public bool Passed { get; set; } = false;

    public int GroupId { get; set; }
    public Group Group { get; set; }

    public int ProblemId { get; set; }
    public Problem Problem { get; set; }
}
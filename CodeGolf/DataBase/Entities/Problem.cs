using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CodeGolf.DataBase.Entities;

public class Problem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Explanation { get; set; }
    public string ExampleSolution { get; set; }
    public string Input {get; set;}

    public List<Answer> Answers { get; } = new();

    public override string ToString()
    {
        return $"Problem \"{Name}\"";
    }
}
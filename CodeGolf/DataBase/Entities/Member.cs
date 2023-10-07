namespace CodeGolf.DataBase.Entities;

public class Member
{
    public string Name { get; set; }
    public int Id { get; set; }
    
    public int GroupId { get; set; }
    public Group Group { get; set; }
}
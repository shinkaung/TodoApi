namespace TodoApi.Models
{
    public class AdminLevel{
    public int Id { get; set; }
    public string AdminLevelName { get; set; } = string.Empty;

    public bool IsAdministrator { get; set; } 
}
}
namespace TodoApi.Models
{
    public class Employee
    {
        public long id { get; set; }
        public string? empName { get; set; }
        public string? empAddress { get; set; }
        public bool IsComplete { get; set; }
    }
}
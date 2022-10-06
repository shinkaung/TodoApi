namespace TodoApi.Models
{
    public class AdminMenu{
    public int AdminMenuId { get; set; } 
    public int ParentID { get; set; } 

    public string AdminMenuName { get; set; } 

    public int SrNo { get; set; } 

    public string ControllerName { get; set; } 

    public string Icon{ get; set; } 
}
}
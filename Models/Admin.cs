using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class Admin
    {
        public int Id { get; set; }
        public int AdminLevelId { get; set; } 
        public string? AdminName { get; set; } = string.Empty;
         public string?Email { get; set; } = string.Empty;
        public string? LoginName { get; set; } = string.Empty;
         public int LoginFailCount { get; set; }
        public string? Password { get; set; } = string.Empty;
        public string? Salt { get; set; } = string.Empty;
        public bool Inactive { get; set; }

        public bool IsBlock { get; set; }
        public DateTime CreateDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public DateTime LastLoginDate { get; set; }

        [ForeignKey("AdminLevelId")]
       public AdminLevel? AdminLevel { get; set; }

        [NotMapped]
        public string? AdminPhoto {get; set; } //Not Mapped attribute,not to store in actual database column
    }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApi.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerAddress { get; set; }
        public int CustomerTypeId { get; set; } 

        [NotMapped]
        public string? CustomerPhoto {get; set; } //Not Mapped attribute,not to store in actual database column
    }
}

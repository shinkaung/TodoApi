using System.ComponentModel.DataAnnotations.Schema;
namespace TodoApi.Models
{
    public class Supplier
    {
        public long Id { get; set; }
        public DateTime? RegisterDate { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierAddress { get; set; }
        public int SupplierTypeId { get; set; } 

        [NotMapped]
        public string? SupplierPhoto {get; set; } //Not Mapped attribute,not to store in actual database column
    }
}

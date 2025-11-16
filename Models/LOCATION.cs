using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prim_Kruskal_Web.Models
{
    [Table("LOCATION")]
    public class LOCATION
    {
        [Key]
        public int ID { get; set; }
        [Required]
        public int ProvinceId { get; set; }
        [Required, MaxLength(255)]
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [MaxLength(50)]
        public string Source { get; set; } // Overpass / Seed / Estimated
    }
}

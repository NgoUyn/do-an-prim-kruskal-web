using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prim_Kruskal_Web.Models
{
    [Table("KHOANG_CACH")]
    public class KHOANG_CACH
    {
        [Key]
        public int ID { get; set; }
        
        [Required]
        public int ID_TINH_A { get; set; }
        
        [Required]
        public int ID_TINH_B { get; set; }
        
        public double KHOANG_CACH_VALUE { get; set; }

        [ForeignKey("ID_TINH_A")]
        public virtual TINH_THANH TINH_THANH { get; set; }

        [ForeignKey("ID_TINH_B")] 
        public virtual TINH_THANH TINH_THANH1 { get; set; }
    }
}
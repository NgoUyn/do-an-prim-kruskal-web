using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prim_Kruskal_Web.Models
{
    [Table("TINH_THANH")]
    public class TINH_THANH
    {
        public TINH_THANH()
        {
            KHOANG_CACH = new HashSet<KHOANG_CACH>();
            KHOANG_CACH1 = new HashSet<KHOANG_CACH>();
        }

        [Key]
        [Column("ID_TINH")]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string TEN_TINH { get; set; }

        public virtual ICollection<KHOANG_CACH> KHOANG_CACH { get; set; }
        public virtual ICollection<KHOANG_CACH> KHOANG_CACH1 { get; set; }
    }
}
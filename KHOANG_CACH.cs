using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prim_Kruskal_Web.Models
{
    public class KHOANG_CACH
    {
        [Key]
        public int ID { get; set; }

        public int ID_TINH_A { get; set; }
        public int ID_TINH_B { get; set; }
        public double Distance { get; set; }

        [ForeignKey("ID_TINH_A")]
        public virtual TINH_THANH TINH_THANH { get; set; }

        [ForeignKey("ID_TINH_B")]
        public virtual TINH_THANH TINH_THANH1 { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Prim_Kruskal_Web.Models
{
    public class TINH_THANH
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public virtual ICollection<KHOANG_CACH> KHOANG_CACH { get; set; }
        public virtual ICollection<KHOANG_CACH> KHOANG_CACH1 { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class GraphInputModel
    {
        public List<EdgeInput> Edges { get; set; } = new List<EdgeInput>();
    }

    public class EdgeInput
    {
        public string Src { get; set; }
        public string Dest { get; set; }
        public int Weight { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Edge // Cạnh

    {

        public int SourceId { get; set; }
        public int DestinationId { get; set; }
        public Node Src { get; set; }
        public Node Destination { get; set; }

        public double? KhoangCach { get; set; }
        public double? Cost { get; set; }
    }
}
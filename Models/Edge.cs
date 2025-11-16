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

        // Unified weight used by algorithms (fallback: KhoangCach -> Cost -> 0)
        public double Weight
        {
            get { return KhoangCach ?? Cost ?? 0d; }
        }

        public Edge() { }

        // Convenience ctor used by MoPhong and other controllers
        public Edge(Node src, Node dest, double weight)
        {
            Src = src;
            Destination = dest;
            SourceId = src != null ? src.Id : 0;
            DestinationId = dest != null ? dest.Id : 0;
            KhoangCach = weight;
            Cost = null;
        }
    }
}
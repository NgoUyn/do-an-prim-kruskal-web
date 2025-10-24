using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Edge // Cạnh

    {
        public Node Src { get; set; } // đâù cạnh
        public Node Destination { get; set; } // cuốì cạnh
        public int Weight { get; set; } // trọng số cạnh = value
        public Edge(Node src, Node destination, int weight) // contructor
        {
            Src = src;
            Destination = destination;
            Weight = weight;
        } 
        public override string ToString()
        {
            return $"{Src.Name} - {Destination.Name} : {Weight}";
        }
    }

   
}

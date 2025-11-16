using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Node // Đỉnh
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Added convenience constructor used by controllers
        public Node(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public Node() { }
    }
}
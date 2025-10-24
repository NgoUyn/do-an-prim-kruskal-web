using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Node // Đỉnh
    {
        public int Id { get; set; } //ID đỉnh
        public string Name { get; set; } // Tên đỉnh
        public Node (int id,string name) // contructor
        {
            Id = id;
            Name = name;
        }

    }
}
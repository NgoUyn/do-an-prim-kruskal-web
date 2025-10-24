using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Graph // Đồ thị
    {
        public List<Node > Nodes { get; set; } //list các đỉnh 
        public List<Edge> Edges { get; set; } // list các cạnh
        public void AddNode(Node node) // hàm thêm đỉnh mới 
        {
            if (Nodes == null)
            {
                Nodes = new List<Node>();
            }
            Nodes.Add(node);
        }
        public void AddEdge(Node Src,Node Destination,int weight) // hàm thêm cạnh mới 
        {
            if (Edges == null)
            {
                Edges = new List<Edge>();
            }
            Edges.Add(new Edge(Src,Destination,weight));
        }

    }
}
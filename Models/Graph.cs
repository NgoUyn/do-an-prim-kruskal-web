using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Graph // Đồ thị
    {

        public List<Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        // Constructor to initialize collections
        public Graph()
        {
            Nodes = new List<Node>();
            Edges = new List<Edge>();
        }

        public void AddNode(Node node)
        {
            if (Nodes.All(n => n.Id != node.Id))
            {
                Nodes.Add(node);
            }
        }

        public void AddEdge(int sourceId, int destinationId, double? khoangCach, double? cost)
        {
            var src = Nodes.FirstOrDefault(n => n.Id == sourceId);
            var dest = Nodes.FirstOrDefault(n => n.Id == destinationId);

            if (src == null || dest == null) return;

            if (Edges.Any(e =>
                (e.SourceId == sourceId && e.DestinationId == destinationId) ||
                (e.SourceId == destinationId && e.DestinationId == sourceId)))
                return;

            var edge = new Edge
            {
                SourceId = sourceId,
                DestinationId = destinationId,
                Src = src,
                Destination = dest,
                KhoangCach = khoangCach,
                Cost = cost
            };

            Edges.Add(edge);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace Prim_Kruskal_Web.Models
{
    public class Prim
    {
        public static List<Edge> FindMST(Graph graph)
        {
            var mst = new List<Edge>();
            var visited = new HashSet<Node>();

            if (graph.Nodes == null || graph.Nodes.Count == 0)
                return mst;

            // Bắt đầu từ đỉnh đầu tiên
            var start = graph.Nodes.First();
            visited.Add(start);

            while (visited.Count < graph.Nodes.Count)
            {
                // chọn cạnh nhỏ nhất nối đỉnh đã thăm với đỉnh chưa thăm
                var edges = graph.Edges
                    .Where(e => (visited.Contains(e.Src) && !visited.Contains(e.Destination)) ||
                                (visited.Contains(e.Destination) && !visited.Contains(e.Src)))
                    .OrderBy(e => e.Weight)
                    .ToList();

                if (edges.Count == 0)
                    break; // đồ thị không liên thông

                var minEdge = edges.First();
                mst.Add(minEdge);
                visited.Add(visited.Contains(minEdge.Src) ? minEdge.Destination : minEdge.Src);
            }

            return mst;
        }
    }
}

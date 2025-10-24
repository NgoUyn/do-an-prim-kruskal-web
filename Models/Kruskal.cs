using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class Kruskal
    {
        public static List<Edge> FindMST(Graph graph)
        {
            var mst = new List<Edge>(); // xem nó như biến KQ giống lý thuyết đi 
            var ds = new DisjoinSet(); 
            ds.MakeSet(graph.Nodes);

            var sortedEdges = graph.Edges.OrderBy(e => e.Weight).ToList(); // sắp xếp các cạnh theo trọng số tăng dần (THAM LAM : GREEDY  ĐÓ !!!!)
            foreach (var edge in sortedEdges)
            {
                if (!ds.Connected(edge.Src.Id, edge.Destination.Id)) // nếu 2 đỉnh chưa nối thì chọn cạnh này rồi gộp 2 nó lại  /// nếu đã thì bỏ qua tránh tọa chu trình
                {
                    mst.Add(edge);
                    ds.Union(edge.Src.Id, edge.Destination.Id);
                }
            }
            return mst;
        }
    }
}
using System.Collections.Generic;
namespace Prim_Kruskal_Web.Models
{
    public class CompareResult
    {
        public Graph Graph { get; set; }
        public int NodeCount { get; set; }
        // Kruskal
        public List<Edge> KruskalEdges { get; set; }
        public long KruskalTime { get; set; }
        public double KruskalCost { get; set; }
        public long KruskalMemory { get; set; }
        // Prim
        public List<Edge> PrimEdges { get; set; }
        public long PrimTime { get; set; }
        public double PrimCost { get; set; }
        public long PrimMemory { get; set; }
    }
}
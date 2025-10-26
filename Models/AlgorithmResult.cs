using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class AlgorithmResult
    {
        public string AlgorithmName { get; set; }
        public List<Edge> MSTEdges { get; set; } = new List<Edge>();
        public double TotalCost { get; set; }
        public List<AlgorithmStep> Steps { get; set; } = new List<AlgorithmStep>();
        public double ExecutionTimeMs { get; set; }
        public int StepCount { get; set; }
        public string TimeComplexity { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Prim_Kruskal_Web.Models
{
    public class AlgorithmStep
    {
        public int StepNumber { get; set; }
        public string Description { get; set; }
        public List<Edge> CurrentMSTEdges { get; set; } = new List<Edge>();
        public List<int> VisitedNodes { get; set; } = new List<int>();
    }
}
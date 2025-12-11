using System.Collections.Generic;

namespace Prim_Kruskal_Web.Models
{
    // --- CLASS CHO MÔ PHỎNG (SIMULATION) ---
    // Đã bỏ namespace con để tránh lỗi "not exist"

    public class SimNode
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class SimEdge
    {
        public int SourceId { get; set; }
        public int TargetId { get; set; }
        public double Weight { get; set; }
        public string Description { get; set; }
    }

    public enum StepStatus { Checking, Accepted, Rejected }

    public class SimStep
    {
        public int StepIndex { get; set; }
        public SimEdge CurrentEdge { get; set; }
        public StepStatus Status { get; set; }
        public string Message { get; set; }
        public List<int> ConnectedNodes { get; set; }
    }

    public class SimResult
    {
        public List<SimEdge> MSTEdges { get; set; }
        public double TotalCost { get; set; }
        public double ExecutionTime { get; set; }
        public List<SimStep> Steps { get; set; }
    }
}
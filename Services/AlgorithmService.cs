using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Prim_Kruskal_Web.Models; // Sử dụng đúng namespace

namespace Prim_Kruskal_Web.Services
{
    public class AlgorithmService
    {
        // Tính khoảng cách pixel trên màn hình
        public double CalculateDistance(SimNode n1, SimNode n2)
        {
            return Math.Round(Math.Sqrt(Math.Pow(n2.X - n1.X, 2) + Math.Pow(n2.Y - n1.Y, 2)), 0);
        }

        // --- KRUSKAL (Dùng SimNode/SimEdge) ---
        public AlgorithmResult RunKruskal(List<SimNode> nodes, List<SimEdge> edges)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new AlgorithmResult { MSTEdges = new List<SimEdge>(), Steps = new List<AlgorithmStep>() };

            var sortedEdges = edges.OrderBy(e => e.Weight).ToList();
            var dsu = new DisjointSet(nodes.Count + 100);
            int stepIdx = 1;

            foreach (var edge in sortedEdges)
            {
                var step = new AlgorithmStep
                {
                    StepIndex = stepIdx++,
                    CurrentEdge = edge,
                    Status = StepStatus.Checking,
                    Message = $"Xét cạnh {edge.Weight}..."
                };

                if (dsu.Find(edge.SourceId) != dsu.Find(edge.TargetId))
                {
                    dsu.Union(edge.SourceId, edge.TargetId);
                    result.MSTEdges.Add(edge);
                    result.TotalCost += edge.Weight;
                    step.Status = StepStatus.Accepted; step.Message = "Hợp lệ. Chọn.";
                }
                else
                {
                    step.Status = StepStatus.Rejected; step.Message = "Bỏ qua (Tạo chu trình).";
                }

                step.ConnectedNodes = result.MSTEdges.SelectMany(e => new[] { e.SourceId, e.TargetId }).Distinct().ToList();
                result.Steps.Add(step);
            }
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            return result;
        }

        // --- PRIM (Dùng SimNode/SimEdge) ---
        public AlgorithmResult RunPrim(List<SimNode> nodes, List<SimEdge> edges, int startId)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new AlgorithmResult { MSTEdges = new List<SimEdge>(), Steps = new List<AlgorithmStep>() };
            var visited = new HashSet<int> { startId };
            int stepIdx = 1;

            // Loop an toàn
            int maxLoop = nodes.Count * nodes.Count;

            while (visited.Count < nodes.Count && maxLoop-- > 0)
            {
                var candidates = edges.Where(e =>
                    (visited.Contains(e.SourceId) && !visited.Contains(e.TargetId)) ||
                    (visited.Contains(e.TargetId) && !visited.Contains(e.SourceId))
                ).OrderBy(e => e.Weight).ToList();

                if (!candidates.Any()) break;

                var best = candidates.First();
                result.MSTEdges.Add(best);
                result.TotalCost += best.Weight;

                int newNode = visited.Contains(best.SourceId) ? best.TargetId : best.SourceId;
                visited.Add(newNode);

                result.Steps.Add(new AlgorithmStep
                {
                    StepIndex = stepIdx++,
                    CurrentEdge = best,
                    Status = StepStatus.Accepted,
                    Message = $"Mở rộng sang đỉnh {newNode}",
                    ConnectedNodes = visited.ToList()
                });
            }
            stopwatch.Stop();
            result.ExecutionTime = stopwatch.Elapsed.TotalMilliseconds;
            return result;
        }

        // Class DSU nội bộ
        private class DisjointSet
        {
            private int[] p;
            public DisjointSet(int n) { p = Enumerable.Range(0, n).ToArray(); }
            public int Find(int i) { if (i >= p.Length) return i; return p[i] == i ? i : (p[i] = Find(p[i])); }
            public void Union(int i, int j) => p[Find(i)] = Find(j);
        }
    }
}
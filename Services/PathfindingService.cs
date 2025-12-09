using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Prim_Kruskal_Web.Services
{
    public class PathfindingService
    {
        // Result chuyên dụng cho tìm đường
        public class PathResult
        {
            public List<Node> VisitedNodes { get; set; } = new List<Node>(); // Các ô đã duyệt (để vẽ loang)
            public List<Node> Path { get; set; } = new List<Node>();         // Đường đi ngắn nhất
            public double ExecutionTime { get; set; }
            public bool Found { get; set; }
        }

        // 1. Dijkstra (Tìm đường)
        public PathResult RunDijkstra(Graph graph, Node start, Node end)
        {
            var sw = Stopwatch.StartNew();
            var res = new PathResult();

            var dist = new Dictionary<int, double>();
            var prev = new Dictionary<int, Node>();
            var pq = new SortedSet<(double distance, int id)>(); // Priority Queue giả lập

            foreach (var n in graph.Nodes) dist[n.Id] = double.MaxValue;
            dist[start.Id] = 0;
            pq.Add((0, start.Id));

            while (pq.Count > 0)
            {
                var currentId = pq.Min.id;
                pq.Remove(pq.Min);
                var current = graph.Nodes.First(n => n.Id == currentId);

                res.VisitedNodes.Add(current); // Ghi nhận để vẽ loang
                if (current.Id == end.Id) { res.Found = true; break; }

                var neighbors = GetNeighbors(graph, current);
                foreach (var neighbor in neighbors)
                {
                    double alt = dist[current.Id] + neighbor.Weight;
                    if (alt < dist[neighbor.Node.Id])
                    {
                        pq.Remove((dist[neighbor.Node.Id], neighbor.Node.Id));
                        dist[neighbor.Node.Id] = alt;
                        prev[neighbor.Node.Id] = current;
                        pq.Add((alt, neighbor.Node.Id));
                    }
                }
            }
            sw.Stop(); res.ExecutionTime = sw.Elapsed.TotalMilliseconds;
            if (res.Found) ReconstructPath(res, prev, end);
            return res;
        }

        // 2. A* (A-Star) - Thông minh hơn Dijkstra
        public PathResult RunAStar(Graph graph, Node start, Node end)
        {
            var sw = Stopwatch.StartNew();
            var res = new PathResult();

            var gScore = new Dictionary<int, double>(); // Chi phí từ Start
            var fScore = new Dictionary<int, double>(); // Chi phí ước tính (g + h)
            var prev = new Dictionary<int, Node>();
            var openSet = new HashSet<int> { start.Id }; // Tập đang xét

            foreach (var n in graph.Nodes) { gScore[n.Id] = double.MaxValue; fScore[n.Id] = double.MaxValue; }
            gScore[start.Id] = 0;
            fScore[start.Id] = Heuristic(start, end);

            while (openSet.Count > 0)
            {
                // Lấy node có fScore nhỏ nhất
                var currentId = openSet.OrderBy(id => fScore[id]).First();
                var current = graph.Nodes.First(n => n.Id == currentId);

                if (current.Id == end.Id) { res.Found = true; break; }

                openSet.Remove(currentId);
                res.VisitedNodes.Add(current);

                foreach (var neighbor in GetNeighbors(graph, current))
                {
                    double tentativeG = gScore[current.Id] + neighbor.Weight;
                    if (tentativeG < gScore[neighbor.Node.Id])
                    {
                        prev[neighbor.Node.Id] = current;
                        gScore[neighbor.Node.Id] = tentativeG;
                        fScore[neighbor.Node.Id] = tentativeG + Heuristic(neighbor.Node, end);
                        if (!openSet.Contains(neighbor.Node.Id)) openSet.Add(neighbor.Node.Id);
                    }
                }
            }
            sw.Stop(); res.ExecutionTime = sw.Elapsed.TotalMilliseconds;
            if (res.Found) ReconstructPath(res, prev, end);
            return res;
        }

        // 3. BFS (Loang đều - Không quan tâm trọng số)
        public PathResult RunBFS(Graph graph, Node start, Node end)
        {
            var sw = Stopwatch.StartNew();
            var res = new PathResult();
            var queue = new Queue<Node>();
            var visited = new HashSet<int>();
            var prev = new Dictionary<int, Node>();

            queue.Enqueue(start);
            visited.Add(start.Id);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                res.VisitedNodes.Add(current);
                if (current.Id == end.Id) { res.Found = true; break; }

                foreach (var neighbor in GetNeighbors(graph, current))
                {
                    if (!visited.Contains(neighbor.Node.Id))
                    {
                        visited.Add(neighbor.Node.Id);
                        prev[neighbor.Node.Id] = current;
                        queue.Enqueue(neighbor.Node);
                    }
                }
            }
            sw.Stop(); res.ExecutionTime = sw.Elapsed.TotalMilliseconds;
            if (res.Found) ReconstructPath(res, prev, end);
            return res;
        }

        // Helpers
        private double Heuristic(Node a, Node b) => Math.Abs(a.Latitude - b.Latitude) + Math.Abs(a.Longitude - b.Longitude); // Manhattan distance for grid

        private List<(Node Node, double Weight)> GetNeighbors(Graph g, Node n)
        {
            // Tìm các cạnh nối với node n
            return g.Edges.Where(e => e.SourceId == n.Id).Select(e => (e.Destination, e.Weight))
                   .Concat(g.Edges.Where(e => e.DestinationId == n.Id).Select(e => (e.Src, e.Weight)))
                   .ToList();
        }

        private void ReconstructPath(PathResult res, Dictionary<int, Node> prev, Node end)
        {
            var curr = end;
            while (prev.ContainsKey(curr.Id))
            {
                res.Path.Add(curr);
                curr = prev[curr.Id];
            }
            res.Path.Add(curr); // Add start
            res.Path.Reverse();
        }
    }
}
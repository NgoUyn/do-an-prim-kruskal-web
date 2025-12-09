using Newtonsoft.Json;
using Prim_Kruskal_Web.Models;
using Prim_Kruskal_Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class MoPhongController : Controller
    {
        // --- KHAI BÁO SERVICE ---
        private readonly PrimAlgorithm_Services _primService = new PrimAlgorithm_Services();
        private readonly KruskalAlgorithm_Service _kruskalService = new KruskalAlgorithm_Service();
        private readonly PathfindingService _pathService = new PathfindingService();

        // --- PHẦN 1: MÔ PHỎNG TỪNG BƯỚC ---
        [HttpGet]
        public ActionResult MoPhong()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Result(string nodes, string[] src, string[] dest, int[] weight, string algorithm)
        {
            if (string.IsNullOrWhiteSpace(nodes) || src == null || dest == null || weight == null)
            {
                ModelState.AddModelError("Input", "Thiếu dữ liệu đầu vào");
                return View("MoPhong");
            }

            var graph = new Graph();
            var nodeList = nodes.Split(',')
                .Select((name, index) => new Node(index + 1, name.Trim()))
                .Where(n => !string.IsNullOrWhiteSpace(n.Name))
                .ToList();
            graph.Nodes = nodeList;
            var nodeDict = nodeList.ToDictionary(n => n.Name, n => n);

            var edgeList = new List<Edge>();
            for (int i = 0; i < src.Length && i < dest.Length && i < weight.Length; i++)
            {
                var sName = src[i]?.Trim();
                var dName = dest[i]?.Trim();
                if (string.IsNullOrEmpty(sName) || string.IsNullOrEmpty(dName)) continue;
                if (!nodeDict.ContainsKey(sName) || !nodeDict.ContainsKey(dName)) continue;
                if (sName == dName) continue;
                edgeList.Add(new Edge(nodeDict[sName], nodeDict[dName], weight[i]));
            }
            graph.Edges = edgeList;

            if (!graph.Nodes.Any() || !graph.Edges.Any())
            {
                ViewBag.Error = "Đồ thị không hợp lệ (cần >=1 cạnh).";
                return View("MoPhong");
            }

            var primSteps = BuildPrimSteps(graph);
            var kruskalSteps = BuildKruskalSteps(graph);

            List<Edge> result = algorithm == "Kruskal" ? Kruskal.FindMST(graph) : Prim.FindMST(graph);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            sw.Stop();

            ViewBag.Nodes = nodes;
            ViewBag.Src = src;
            ViewBag.Dest = dest;
            ViewBag.Weight = weight;
            ViewBag.Algorithm = algorithm;
            ViewBag.Result = result;
            ViewBag.Time = sw.Elapsed.TotalMilliseconds.ToString("F3");

            ViewBag.PrimStepsJson = JsonConvert.SerializeObject(primSteps);
            ViewBag.KruskalStepsJson = JsonConvert.SerializeObject(kruskalSteps);
            ViewBag.NodesJson = JsonConvert.SerializeObject(graph.Nodes);
            ViewBag.AllEdgesJson = JsonConvert.SerializeObject(graph.Edges);
            ViewBag.ResultJson = JsonConvert.SerializeObject(result);

            return View("MoPhong");
        }

        // --- PHẦN 2: THE RACE (LOANG DẦU CŨ) ---
        [HttpGet]
        public ActionResult Compare(int nodeCount = 15)
        {
            return RunRace(nodeCount);
        }

        [HttpPost]
        public ActionResult TheRace(int? nodeCount)
        {
            return RunRace(nodeCount ?? 15);
        }

        private ActionResult RunRace(int n)
        {
            int size = (int)Math.Sqrt(n);
            if (size < 5) size = 5;
            if (size > 100) size = 100;
            int realN = size * size;

            var graph = GenerateGridGraph(size, size);

            GC.Collect(); GC.WaitForPendingFinalizers();
            long startMemPrim = GC.GetTotalMemory(true);
            var primResult = _primService.FindMST(graph, 0);
            long endMemPrim = GC.GetTotalMemory(false);

            GC.Collect(); GC.WaitForPendingFinalizers();
            long startMemKrus = GC.GetTotalMemory(true);
            var kruskalResult = _kruskalService.FindMST(graph);
            long endMemKrus = GC.GetTotalMemory(false);

            var model = new CompareResult
            {
                Graph = graph,
                NodeCount = realN,
                PrimEdges = primResult.MSTEdges,
                PrimTime = (long)primResult.ExecutionTimeMs,
                PrimCost = primResult.TotalCost,
                PrimMemory = Math.Max(0, endMemPrim - startMemPrim),
                KruskalEdges = kruskalResult.MSTEdges,
                KruskalTime = (long)kruskalResult.ExecutionTimeMs,
                KruskalCost = kruskalResult.TotalCost,
                KruskalMemory = Math.Max(0, endMemKrus - startMemKrus)
            };

            return View("Compare", model);
        }

        // --- PHẦN 3: ARENA (ĐẤU TRƯỜNG TÌM ĐƯỜNG MỚI) ---
        [HttpGet]
        public ActionResult Arena()
        {
            return View();
        }

        [HttpPost]
        public ActionResult RunArena(int nodeCount, string leftAlgo, string rightAlgo)
        {
            // 1. Tạo Map Lưới
            int size = (int)Math.Sqrt(nodeCount);
            if (size < 10) size = 10;
            var graph = GenerateGridGraph(size, size);

            // 2. Chọn điểm Start (Góc trái trên) và End (Góc phải dưới)
            var startNode = graph.Nodes.First();
            var endNode = graph.Nodes.Last();

            // 3. Chạy thuật toán (Trả về PathResult cụ thể, không dùng dynamic)
            PathfindingService.PathResult resLeft = RunPathAlgo(leftAlgo, graph, startNode, endNode);
            PathfindingService.PathResult resRight = RunPathAlgo(rightAlgo, graph, startNode, endNode);

            // 4. Trả về JSON (Bây giờ .Select sẽ hoạt động bình thường vì đã biết kiểu dữ liệu)
            return Json(new
            {
                success = true,
                gridSize = size,
                start = new { x = startNode.Longitude, y = startNode.Latitude },
                end = new { x = endNode.Longitude, y = endNode.Latitude },
                left = new
                {
                    name = leftAlgo,
                    time = resLeft.ExecutionTime,
                    visited = resLeft.VisitedNodes.Select(node => new { x = node.Longitude, y = node.Latitude }),
                    path = resLeft.Path.Select(node => new { x = node.Longitude, y = node.Latitude })
                },
                right = new
                {
                    name = rightAlgo,
                    time = resRight.ExecutionTime,
                    visited = resRight.VisitedNodes.Select(node => new { x = node.Longitude, y = node.Latitude }),
                    path = resRight.Path.Select(node => new { x = node.Longitude, y = node.Latitude })
                }
            });
        }

        // --- HELPERS ---

        // Fix lỗi dynamic: Trả về kiểu cụ thể PathfindingService.PathResult
        private PathfindingService.PathResult RunPathAlgo(string name, Graph g, Node s, Node e)
        {
            switch (name)
            {
                case "Dijkstra": return _pathService.RunDijkstra(g, s, e);
                case "A*": return _pathService.RunAStar(g, s, e);
                case "BFS": return _pathService.RunBFS(g, s, e);
                case "Prim":
                    // Prim là tìm cây khung, không phải tìm đường, nên ta map kết quả sang PathResult giả lập
                    var pRes = _primService.FindMST(g, 0);
                    return new PathfindingService.PathResult
                    {
                        ExecutionTime = pRes.ExecutionTimeMs,
                        VisitedNodes = g.Nodes, // Prim duyệt hết các đỉnh
                        Path = new List<Node>(), // Không có đường đi cụ thể
                        Found = true
                    };
                case "Kruskal":
                    var kRes = _kruskalService.FindMST(g);
                    return new PathfindingService.PathResult
                    {
                        ExecutionTime = kRes.ExecutionTimeMs,
                        VisitedNodes = g.Nodes,
                        Path = new List<Node>(),
                        Found = true
                    };
                default:
                    return new PathfindingService.PathResult();
            }
        }

        private Graph GenerateGridGraph(int rows, int cols)
        {
            var g = new Graph();
            var rand = new Random();
            var nodes = new Node[rows, cols];
            int idCounter = 1;

            // Tạo Node
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    var node = new Node(idCounter++, $"{r},{c}") { Latitude = r, Longitude = c };
                    nodes[r, c] = node;
                    g.AddNode(node);
                }

            // Tạo Edge
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                {
                    if (c < cols - 1) g.AddEdge(nodes[r, c], nodes[r, c + 1], rand.Next(1, 100));
                    if (r < rows - 1) g.AddEdge(nodes[r, c], nodes[r + 1, c], rand.Next(1, 100));
                }
            return g;
        }

        private List<AlgorithmStep> BuildPrimSteps(Graph graph)
        {
            var steps = new List<AlgorithmStep>();
            var visited = new HashSet<Node>();
            if (graph.Nodes.Count == 0) return steps;
            var start = graph.Nodes.First();
            visited.Add(start);
            steps.Add(new AlgorithmStep
            {
                StepNumber = 1,
                Description = $"Bắt đầu từ đỉnh {start.Name}",
                VisitedNodes = visited.Select(n => n.Id).ToList(),
                CurrentMSTEdges = new List<Edge>()
            });
            int step = 2;
            while (visited.Count < graph.Nodes.Count)
            {
                var candidate = graph.Edges
                    .Where(e => (visited.Contains(e.Src) && !visited.Contains(e.Destination)) || (visited.Contains(e.Destination) && !visited.Contains(e.Src)))
                    .OrderBy(e => e.Weight)
                    .FirstOrDefault();
                if (candidate == null) break;
                var newNode = visited.Contains(candidate.Src) ? candidate.Destination : candidate.Src;
                visited.Add(newNode);
                var prevEdges = steps.Last().CurrentMSTEdges.ToList();
                prevEdges.Add(candidate);
                steps.Add(new AlgorithmStep
                {
                    StepNumber = step++,
                    Description = $"Chọn cạnh {candidate.Src.Name}-{candidate.Destination.Name} trọng số {candidate.Weight}, thăm {newNode.Name}",
                    CurrentMSTEdges = prevEdges,
                    VisitedNodes = visited.Select(n => n.Id).ToList()
                });
            }
            return steps;
        }

        private List<AlgorithmStep> BuildKruskalSteps(Graph graph)
        {
            var steps = new List<AlgorithmStep>();
            var ds = new DisjoinSet();
            ds.MakeSet(graph.Nodes);
            int step = 1;
            var current = new List<Edge>();
            foreach (var edge in graph.Edges.OrderBy(e => e.Weight))
            {
                bool connected = ds.Connected(edge.Src.Id, edge.Destination.Id);
                if (!connected)
                {
                    current.Add(edge);
                    ds.Union(edge.Src.Id, edge.Destination.Id);
                }
                steps.Add(new AlgorithmStep
                {
                    StepNumber = step++,
                    Description = connected ? $"Bỏ qua cạnh {edge.Src.Name}-{edge.Destination.Name} (tạo chu trình)" : $"Chọn cạnh {edge.Src.Name}-{edge.Destination.Name} trọng số {edge.Weight}",
                    CurrentMSTEdges = current.ToList(),
                    VisitedNodes = current.SelectMany(e => new[] { e.Src.Id, e.Destination.Id }).Distinct().ToList()
                });
            }
            return steps;
        }
    }
}
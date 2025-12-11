using Newtonsoft.Json;
using Prim_Kruskal_Web.Models;
using Prim_Kruskal_Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            var sw = System.Diagnostics.Stopwatch.StartNew();
            List<Edge> result = algorithm == "Kruskal" ? Kruskal.FindMST(graph) : Prim.FindMST(graph);
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

        // Thêm vào MoPhongController.cs
        [HttpPost]
        public ActionResult RunDual(string nodes, string[] src, string[] dest, int[] weight)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(nodes) || src == null || dest == null || weight == null)
            {
                ViewBag.Error = "Vui lòng vẽ hoặc nhập đồ thị trước!";
                return View("MoPhong", new CompareResult());
            }

            // 2. Xây dựng đồ thị từ Input
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
                if (nodeDict.ContainsKey(sName) && nodeDict.ContainsKey(dName) && sName != dName)
                {
                    // Quan trọng: AddEdge 1 chiều cho logic tính toán, nhưng hiển thị sẽ là vô hướng
                    edgeList.Add(new Edge(nodeDict[sName], nodeDict[dName], weight[i]));
                }
            }
            graph.Edges = edgeList;

            // 3. Chạy PRIM (và đo thời gian)
            // Lưu ý: Prim cần danh sách kề đầy đủ (vô hướng) để chạy đúng, ta tạm clone graph nếu cần
            // Ở đây ta giả định service Prim xử lý được danh sách cạnh.
            var swPrim = System.Diagnostics.Stopwatch.StartNew();
            var primResult = _primService.FindMST(graph, 0); // Chạy Prim từ đỉnh đầu tiên
            swPrim.Stop();

            // 4. Chạy KRUSKAL (và đo thời gian)
            var swKruskal = System.Diagnostics.Stopwatch.StartNew();
            var kruskalResult = _kruskalService.FindMST(graph);
            swKruskal.Stop();

            // 5. Đóng gói kết quả vào Model CompareResult
            var model = new CompareResult
            {
                Graph = graph,
                NodeCount = graph.Nodes.Count,

                // Dữ liệu Prim
                PrimEdges = primResult.MSTEdges,
                PrimTime = swPrim.ElapsedMilliseconds, // Lấy thời gian thực (long)
                PrimCost = primResult.TotalCost,
                PrimMemory = primResult.StepCount, // Tạm dùng StepCount để hiển thị số bước

                // Dữ liệu Kruskal
                KruskalEdges = kruskalResult.MSTEdges,
                KruskalTime = swKruskal.ElapsedMilliseconds,
                KruskalCost = kruskalResult.TotalCost,
                KruskalMemory = kruskalResult.StepCount
            };

            // Truyền lại dữ liệu Input để điền lại vào Form (giữ trạng thái)
            ViewBag.Nodes = nodes;
            ViewBag.Src = src;
            ViewBag.Dest = dest;
            ViewBag.Weight = weight;

            // Serialize JSON cho JS vẽ
            ViewBag.NodesJson = JsonConvert.SerializeObject(graph.Nodes);
            ViewBag.AllEdgesJson = JsonConvert.SerializeObject(graph.Edges);
            ViewBag.PrimEdgesJson = JsonConvert.SerializeObject(primResult.MSTEdges);
            ViewBag.KruskalEdgesJson = JsonConvert.SerializeObject(kruskalResult.MSTEdges);

            return View("MoPhong", model);
        }

        [HttpPost]
        public ActionResult RunDualApi(string nodes, string[] src, string[] dest, int[] weight)
        {
            try
            {
                // 1. Validate
                if (string.IsNullOrWhiteSpace(nodes) || src == null || dest == null || weight == null)
                {
                    return Json(new { success = false, message = "Dữ liệu đầu vào bị thiếu!" });
                }

                // 2. Tạo Graph từ dữ liệu Client gửi lên
                var graph = new Graph();

                // a. Tạo Node
                // Client gửi ID dạng số (0, 1, 2...), ta dùng nó làm Name để khớp dữ liệu
                var nodeList = nodes.Split(',')
                    .Select((name, index) => new Node(index + 1, name.Trim()))
                    .ToList();

                graph.Nodes = nodeList;
                var nodeDict = nodeList.ToDictionary(n => n.Name, n => n);

                // b. Tạo Edge
                // Dùng Dictionary để lọc cạnh trùng (Vô hướng: A-B giống B-A)
                var addedEdges = new HashSet<string>();

                for (int i = 0; i < src.Length; i++)
                {
                    var sName = src[i];
                    var dName = dest[i];

                    if (!nodeDict.ContainsKey(sName) || !nodeDict.ContainsKey(dName)) continue;
                    if (sName == dName) continue; // Bỏ khuyên

                    // Tạo key để kiểm tra trùng
                    var key = string.Compare(sName, dName) < 0 ? $"{sName}-{dName}" : $"{dName}-{sName}";

                    if (!addedEdges.Contains(key))
                    {
                        // Dùng hàm AddEdge có sẵn trong Graph.cs của bạn
                        graph.AddEdge(nodeDict[sName], nodeDict[dName], weight[i]);
                        addedEdges.Add(key);
                    }
                }

                if (!graph.Nodes.Any() || !graph.Edges.Any())
                {
                    return Json(new { success = false, message = "Đồ thị rỗng hoặc không hợp lệ." });
                }

                // 3. Chạy thuật toán
                var sw = new System.Diagnostics.Stopwatch();

                // Prim
                sw.Start();
                // Prim cần đỉnh bắt đầu, lấy đỉnh đầu tiên
                var primRes = _primService.FindMST(graph, 0);
                sw.Stop();
                double primTime = sw.Elapsed.TotalMilliseconds;

                // Kruskal
                sw.Restart();
                var kruskalRes = _kruskalService.FindMST(graph);
                sw.Stop();
                double kruskalTime = sw.Elapsed.TotalMilliseconds;

                // 4. Trả về kết quả (Dùng cấu trúc Anonymous Object để tránh lỗi vòng lặp JSON)
                var responseData = new
                {
                    success = true,
                    graph = new
                    {
                        // Chỉ lấy dữ liệu cần thiết để vẽ
                        nodes = graph.Nodes.Select(n => new { id = n.Name }),
                        edges = graph.Edges.Select(e => new { source = e.Src.Name, target = e.Destination.Name, weight = e.Weight })
                    },
                    prim = new
                    {
                        cost = primRes.TotalCost,
                        time = primTime,
                        steps = primRes.StepCount, // Số bước
                        edges = primRes.MSTEdges.Select(e => new { u = e.Src.Name, v = e.Destination.Name, w = e.Weight })
                    },
                    kruskal = new
                    {
                        cost = kruskalRes.TotalCost,
                        time = kruskalTime,
                        steps = kruskalRes.StepCount,
                        edges = kruskalRes.MSTEdges.Select(e => new { u = e.Src.Name, v = e.Destination.Name, w = e.Weight })
                    }
                };

                // QUAN TRỌNG: Serialize bằng Newtonsoft.Json để xử lý lỗi Circular Reference
                var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                return Content(JsonConvert.SerializeObject(responseData, settings), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult RunBenchmark(int maxNodes, int step, string mode)
        {
            var results = new List<object>();
            var rand = new Random();

            // Chạy vòng lặp server-side (Nhanh và ổn định hơn client gọi nhiều lần)
            for (int n = step; n <= maxNodes; n += step)
            {
                var graph = new Graph();
                // 1. Tạo Node
                var nodes = Enumerable.Range(0, n).Select(i => new Node(i, i.ToString())).ToList();
                graph.Nodes = nodes;

                var edges = new List<Edge>();

                // 2. Tạo Cạnh dựa trên MODE (Logic an toàn, không while true)
                if (mode == "sparse")
                {
                    // THƯA: Tạo cây khung (n-1 cạnh) + 50% cạnh ngẫu nhiên
                    for (int i = 0; i < n - 1; i++)
                    {
                        edges.Add(new Edge(nodes[i], nodes[i + 1], rand.Next(1, 100)));
                    }
                    // Thêm ngẫu nhiên (dùng for giới hạn số lần thử để không bị treo)
                    int extra = (int)(n * 0.5);
                    for (int k = 0; k < extra; k++)
                    {
                        int u = rand.Next(0, n);
                        int v = rand.Next(0, n);
                        if (u != v) edges.Add(new Edge(nodes[u], nodes[v], rand.Next(1, 100)));
                    }
                }
                else if (mode == "dense")
                {
                    // DÀY: Duyệt mọi cặp đỉnh, nối với xác suất 70%
                    // Prim sẽ thắng ở đây vì E ~ N^2
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = i + 1; j < n; j++)
                        {
                            if (rand.NextDouble() < 0.7) // 70%
                            {
                                edges.Add(new Edge(nodes[i], nodes[j], rand.Next(1, 100)));
                            }
                        }
                    }
                }
                else // fair
                {
                    // VỪA: Xác suất 20%
                    for (int i = 0; i < n; i++)
                    {
                        for (int j = i + 1; j < n; j++)
                        {
                            if (rand.NextDouble() < 0.2) // 20%
                            {
                                edges.Add(new Edge(nodes[i], nodes[j], rand.Next(1, 100)));
                            }
                        }
                    }
                }

                graph.Edges = edges;

                // 3. Đo Prim (Tắt log trong Service nếu N > 100 để nhanh)
                GC.Collect();
                var sw = System.Diagnostics.Stopwatch.StartNew();
                _primService.FindMST(graph, 0);
                sw.Stop();
                double tPrim = sw.Elapsed.TotalMilliseconds;

                // 4. Đo Kruskal
                GC.Collect();
                sw.Restart();
                _kruskalService.FindMST(graph);
                sw.Stop();
                double tKruskal = sw.Elapsed.TotalMilliseconds;

                results.Add(new { n = n, prim = tPrim, kruskal = tKruskal });
            }

            return Json(new { success = true, data = results });
        }
    }
}
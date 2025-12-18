using Newtonsoft.Json;
using Prim_Kruskal_Web.Models;
using Prim_Kruskal_Web.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly GeminiService _geminiService = new GeminiService();

        // --- PHẦN 1: MÔ PHỎNG TỪNG BƯỚC ---
        [HttpGet]
        public ActionResult MoPhong()
        {
            return View();
        }

        // Action cũ (Legacy) - Giữ lại để đảm bảo tương thích
        [HttpPost]
        public ActionResult Result(string nodes, string[] src, string[] dest, int[] weight, string algorithm)
        {
            if (string.IsNullOrWhiteSpace(nodes) || src == null || dest == null || weight == null)
            {
                ModelState.AddModelError("Input", "Thiếu dữ liệu đầu vào");
                return View("MoPhong");
            }

            var graph = BuildGraphFromStrings(nodes, src, dest, weight);
            if (!graph.Nodes.Any() || !graph.Edges.Any())
            {
                ViewBag.Error = "Đồ thị không hợp lệ (cần ít nhất 1 cạnh).";
                return View("MoPhong");
            }

            var sw = Stopwatch.StartNew();
            List<Edge> result = algorithm == "Kruskal" ?
                _kruskalService.FindMST(graph).MSTEdges :
                _primService.FindMST(graph, 0).MSTEdges;
            sw.Stop();

            ViewBag.Result = result;
            ViewBag.Time = sw.Elapsed.TotalMilliseconds.ToString("F3");
            ViewBag.Algorithm = algorithm;

            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            ViewBag.ResultJson = JsonConvert.SerializeObject(result, settings);
            ViewBag.NodesJson = JsonConvert.SerializeObject(graph.Nodes, settings);
            ViewBag.AllEdgesJson = JsonConvert.SerializeObject(graph.Edges, settings);

            return View("MoPhong");
        }

        // --- API MỚI: CHẠY SO SÁNH (GỌI AI PHÂN TÍCH RIÊNG) ---
        [HttpPost]
        public async Task<ActionResult> RunDualApi(string nodes, string[] src, string[] dest, int[] weight, string primVariant)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nodes) || src == null)
                    return Json(new { success = false, message = "Dữ liệu đầu vào bị thiếu!" });

                var graph = BuildGraphFromStrings(nodes, src, dest, weight);
                int V = graph.Nodes.Count;
                int E = graph.Edges.Count;

                // --- 1. Chạy Prim (Variant) ---
                AlgorithmResult primRes;
                string primAlgoName = "Prim Standard";
                long primTheory = 0;

                switch (primVariant)
                {
                    case "Matrix":
                        primRes = _primService.FindMST_Matrix(graph, 0);
                        primTheory = (long)V * V;
                        primAlgoName = "Prim Matrix";
                        break;
                    case "Heap":
                        primRes = _primService.FindMST_Heap(graph, 0);
                        primTheory = (long)(E * Math.Log(V));
                        primAlgoName = "Prim Heap";
                        break;
                    default:
                        primRes = _primService.FindMST(graph, 0);
                        primTheory = (long)V * V;
                        break;
                }

                // --- 2. Chạy Kruskal ---
                var kruskalRes = _kruskalService.FindMST(graph);
                long kruskalTheory = (long)(E * Math.Log(E));

                // --- 3. Gọi AI Phân tích Simulation ---
                string aiAnalysis = "Đang chờ phân tích...";
                try
                {
                    if (V <= 150) // Giới hạn kích thước để AI phản hồi nhanh
                        aiAnalysis = await _geminiService.AnalyzeSimulation(
                            primAlgoName, primRes.ExecutionTimeMs, primRes.StepCount, primTheory,
                            kruskalRes.ExecutionTimeMs, kruskalRes.StepCount, kruskalTheory,
                            V, E
                        );
                    else
                        aiAnalysis = "Đồ thị quá lớn, bỏ qua phân tích chi tiết của AI.";
                }
                catch (Exception ex)
                {
                    aiAnalysis = "Lỗi kết nối AI: " + ex.Message;
                }

                // --- 4. Trả về JSON ---
                return Content(JsonConvert.SerializeObject(new
                {
                    success = true,
                    graph = new
                    {
                        nodes = graph.Nodes.Select(n => new { id = n.Name }),
                        edges = graph.Edges.Select(e => new { source = e.Src.Name, target = e.Destination.Name, weight = e.Weight })
                    },
                    prim = new
                    {
                        cost = primRes.TotalCost,
                        time = primRes.ExecutionTimeMs,
                        steps = primRes.StepCount,
                        edges = primRes.MSTEdges.Select(e => new { u = e.Src.Name, v = e.Destination.Name }),
                        theoryScore = primTheory,
                        complexity = primAlgoName
                    },
                    kruskal = new
                    {
                        cost = kruskalRes.TotalCost,
                        time = kruskalRes.ExecutionTimeMs,
                        steps = kruskalRes.StepCount,
                        edges = kruskalRes.MSTEdges.Select(e => new { u = e.Src.Name, v = e.Destination.Name }),
                        theoryScore = kruskalTheory,
                        complexity = "O(E log E)"
                    },
                    analysis = aiAnalysis
                }, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }), "application/json");
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi Server: " + ex.Message });
            }
        }

        // --- PHẦN 2: BENCHMARK (ĐÃ FIX: AVERAGE & GỌI AI BENCHMARK RIÊNG) ---
        [HttpPost]
        public async Task<ActionResult> RunBenchmark(int startN, int endN, int step, string mode)
        {
            try
            {
                var results = new List<object>();
                var primTimes = new List<double>();
                var kruskalTimes = new List<double>();
                var rand = new Random();

                if (startN <= 0) startN = 10;
                if (endN < startN) endN = startN + 100;
                if (step <= 0) step = 10;

                for (int n = startN; n <= endN; n += step)
                {
                    var graph = GenerateRandomGraph(n, mode, rand);
                    int E = graph.Edges.Count;

                    // Tính điểm Lý thuyết
                    long scorePrim = (long)n * n;
                    long scoreKruskal = (long)(E * Math.Log(E));

                    // Warm-up
                    _primService.FindMST(graph, 0);

                    // Chạy 3 lần lấy trung bình để chính xác
                    double tP = 0, tK = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        GC.Collect();
                        var sw = Stopwatch.StartNew(); _primService.FindMST(graph, 0); sw.Stop(); tP += sw.Elapsed.TotalMilliseconds;
                        sw.Restart(); _kruskalService.FindMST(graph); sw.Stop(); tK += sw.Elapsed.TotalMilliseconds;
                    }
                    tP /= 3; tK /= 3;

                    primTimes.Add(tP);
                    kruskalTimes.Add(tK);

                    results.Add(new
                    {
                        n = n,
                        e = E,
                        primTime = tP,
                        kruskalTime = tK,
                        primScore = scorePrim,
                        kruskalScore = scoreKruskal
                    });
                }

                // Gọi AI phân tích riêng cho Benchmark
                string aiBenchmark = "Đang chờ phân tích...";
                try
                {
                    aiBenchmark = await _geminiService.AnalyzeBenchmark(startN, endN, mode, primTimes, kruskalTimes);
                }
                catch (Exception ex)
                {
                    aiBenchmark = "Lỗi gọi AI: " + ex.Message;
                }

                return Json(new { success = true, data = results, analysis = aiBenchmark });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Server Error: " + ex.Message });
            }
        }

        // --- HELPER METHODS ---
        private Graph BuildGraphFromStrings(string nodes, string[] src, string[] dest, int[] weight)
        {
            var graph = new Graph();
            var nodeList = nodes.Split(',')
                .Select((name, index) => new Node(index + 1, name.Trim()))
                .Where(n => !string.IsNullOrWhiteSpace(n.Name))
                .ToList();
            graph.Nodes = nodeList;
            var nodeDict = nodeList.ToDictionary(n => n.Name, n => n);

            var addedEdges = new HashSet<string>();
            for (int i = 0; i < src.Length && i < dest.Length && i < weight.Length; i++)
            {
                var sName = src[i]?.Trim();
                var dName = dest[i]?.Trim();
                if (string.IsNullOrEmpty(sName) || string.IsNullOrEmpty(dName)) continue;
                if (!nodeDict.ContainsKey(sName) || !nodeDict.ContainsKey(dName)) continue;
                if (sName == dName) continue; // Bỏ khuyên (self-loop)

                var key = string.Compare(sName, dName) < 0 ? $"{sName}-{dName}" : $"{dName}-{sName}";
                if (addedEdges.Add(key))
                {
                    graph.AddEdge(nodeDict[sName], nodeDict[dName], weight[i]);
                }
            }
            return graph;
        }

        private Graph GenerateRandomGraph(int n, string mode, Random rand)
        {
            var g = new Graph { Nodes = Enumerable.Range(0, n).Select(i => new Node(i, i.ToString())).ToList() };
            var edges = new List<Edge>();
            var nodes = g.Nodes;

            // Tạo cây khung để đảm bảo liên thông
            for (int i = 0; i < n - 1; i++)
                edges.Add(new Edge(nodes[i], nodes[i + 1], rand.Next(1, 100)));

            // Thêm cạnh ngẫu nhiên dựa trên mode
            int extra = mode == "sparse" ? (int)(n * 0.2) : (mode == "fair" ? (int)(n * 1.5) : (int)(n * n * 0.1));
            for (int k = 0; k < extra; k++)
            {
                int u = rand.Next(n), v = rand.Next(n);
                if (u != v) edges.Add(new Edge(nodes[u], nodes[v], rand.Next(1, 100)));
            }
            g.Edges = edges;
            return g;
        }

        // Trang Arena (Optional)
        [HttpGet] public ActionResult Arena() { return View(); }
    }
}
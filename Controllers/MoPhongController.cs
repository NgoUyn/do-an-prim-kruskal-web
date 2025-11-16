using Newtonsoft.Json;
using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class MoPhongController : Controller
    {
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
                if (sName == dName) continue; // bỏ cạnh tự nối
                edgeList.Add(new Edge(nodeDict[sName], nodeDict[dName], weight[i]));
            }
            graph.Edges = edgeList;

            // Nếu đồ thị rỗng -> trả về thông báo
            if (!graph.Nodes.Any() || !graph.Edges.Any())
            {
                ViewBag.Error = "Đồ thị không hợp lệ (cần >=1 cạnh).";
                return View("MoPhong");
            }

            var primSteps = BuildPrimSteps(graph);
            var kruskalSteps = BuildKruskalSteps(graph);

            List<Edge> result = algorithm == "Kruskal" ? Kruskal.FindMST(graph) : Prim.FindMST(graph);
            var sw = Stopwatch.StartNew(); // simple timing placeholder
            sw.Stop();

            // ViewBags for form repopulation
            ViewBag.Nodes = nodes;
            ViewBag.Src = src;
            ViewBag.Dest = dest;
            ViewBag.Weight = weight;
            ViewBag.Algorithm = algorithm;
            ViewBag.Result = result;
            ViewBag.Time = sw.Elapsed.TotalMilliseconds.ToString("F3");

            // JSON for client visualization
            ViewBag.PrimStepsJson = JsonConvert.SerializeObject(primSteps);
            ViewBag.KruskalStepsJson = JsonConvert.SerializeObject(kruskalSteps);
            ViewBag.NodesJson = JsonConvert.SerializeObject(graph.Nodes);
            ViewBag.AllEdgesJson = JsonConvert.SerializeObject(graph.Edges);
            ViewBag.ResultJson = JsonConvert.SerializeObject(result);

            return View("MoPhong");
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

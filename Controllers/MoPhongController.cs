using Newtonsoft.Json;
using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics; // 🔹 thêm dòng này
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
            var graph = new Graph();

            // Tạo danh sách đỉnh
            var nodeList = nodes.Split(',')
                .Select((name, index) => new Node(index + 1, name.Trim()))
                .ToList();
            graph.Nodes = nodeList;

            var nodeDict = nodeList.ToDictionary(n => n.Name, n => n);

            // Tạo danh sách cạnh
            var edgeList = new List<Edge>();
            for (int i = 0; i < src.Length; i++)
            {
                if (nodeDict.ContainsKey(src[i].Trim()) && nodeDict.ContainsKey(dest[i].Trim()))
                {
                    edgeList.Add(new Edge(nodeDict[src[i].Trim()], nodeDict[dest[i].Trim()], weight[i]));
                }
            }
            graph.Edges = edgeList;

            // 🕒 Bắt đầu đo thời gian
            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<Edge> result = algorithm == "Kruskal"
                ? Kruskal.FindMST(graph)
                : Prim.FindMST(graph);

            sw.Stop();
            double elapsedMs = sw.Elapsed.TotalMilliseconds;

            // ✅ Đếm số bước (ví dụ: số cạnh trong MST)
            int steps = result.Count;

            // ✅ Lưu dữ liệu người dùng
            ViewBag.Nodes = nodes;
            ViewBag.Src = src;
            ViewBag.Dest = dest;
            ViewBag.Weight = weight;
            ViewBag.Algorithm = algorithm;
            ViewBag.Result = result;

            // ✅ Thông tin so sánh
            ViewBag.Time = elapsedMs.ToString("0.###");
            ViewBag.Steps = steps;

            // ✅ JSON cho mô phỏng
            ViewBag.ResultJson = JsonConvert.SerializeObject(result);
            ViewBag.AllEdgesJson = JsonConvert.SerializeObject(graph.Edges);
            ViewBag.NodesJson = JsonConvert.SerializeObject(graph.Nodes);

            return View("MoPhong");
        }
    }
}

using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class PrimController : Controller
    {
        public ActionResult Index()
        {
            // hiển thị form nhập dữ liệu
            var model = new GraphInputModel();
            model.Edges.Add(new EdgeInput()); // ít nhất 1 dòng nhập
            return View(model);
        }

        [HttpPost]
        public ActionResult RunPrim(GraphInputModel input)
        {
            try
            {
                var graph = BuildGraph(input);
                var mst = Prim.FindMST(graph);
                return View("Result", mst);
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi chạy Prim: " + ex.Message);
            }
        }

        private Graph BuildGraph(GraphInputModel input)
        {
            var graph = new Graph();
            var nodeDict = new Dictionary<string, Node>();

            foreach (var edge in input.Edges)
            {
                if (string.IsNullOrWhiteSpace(edge.Src) || string.IsNullOrWhiteSpace(edge.Dest))
                    continue;

                if (!nodeDict.ContainsKey(edge.Src))
                    nodeDict[edge.Src] = new Node(nodeDict.Count + 1, edge.Src);
                if (!nodeDict.ContainsKey(edge.Dest))
                    nodeDict[edge.Dest] = new Node(nodeDict.Count + 1, edge.Dest);

                graph.AddNode(nodeDict[edge.Src]);
                graph.AddNode(nodeDict[edge.Dest]);
                graph.AddEdge(nodeDict[edge.Src], nodeDict[edge.Dest], edge.Weight);
            }
            return graph;
        }
    }
}
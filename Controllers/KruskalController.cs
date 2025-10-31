using Prim_Kruskal_Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Prim_Kruskal_Web.Models;

namespace Prim_Kruskal_Web.Controllers
{
    public class KruskalController : Controller
    {

        public ActionResult Index()
        {
            var model = new GraphInputModel();
            model.Edges.Add(new EdgeInput());
            return View(model);
        }

        [HttpPost]
        public ActionResult RunKruskal(GraphInputModel input)
        {
            try
            {
                var graph = BuildGraph(input);
                var mst = Kruskal.FindMST(graph);
                return View("Result", mst);
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi chạy Kruskal: " + ex.Message);
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
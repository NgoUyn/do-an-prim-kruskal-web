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
        
        // GET: Kruskal: gọi giải thuật kruskal từ service, trả về kết quả
        
        DatabaseContext db = new DatabaseContext();
        public ActionResult Index()

        {
            var graphs = db.LoadGraph();
            return View(graphs);
        }

        public ActionResult RunKruskal()
        {
            try
            {
                var graph = db.LoadGraph();
                if (graph == null || graph.Nodes.Count == 0)
                    return Content("khong co du lieu trong do thi.");

                var mst = Kruskal.FindMST(graph);
                return View("Result", mst);
            }
            catch (Exception ex)
            {
                return Content("Loi khi chay Kruskal: " + ex.Message);
            }
        }
    }

    
}
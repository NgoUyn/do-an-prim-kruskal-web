using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class PrimController : Controller
    {
        // GET: Prim: gọi giải thuật prim từ service, trả về kết quả
        public ActionResult Index()
        {
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class HomeController : Controller
    {
        
        public ActionResult Index() // giới thiệu về giải thuật tham lam, prim and kruskal
        {
            return View();
        }

        public ActionResult MoPhong() // mô phỏng trực quan
        {
            return View();
        }
         
        public ActionResult UngDung()
        {
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        
        }
}
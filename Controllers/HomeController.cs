using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class HomeController : Controller
    {
<<<<<<< HEAD
        
        public ActionResult Index() // giới thiệu về giải thuật tham lam, prim and kruskal
=======
        public ActionResult Index()
>>>>>>> 7a5034054b259649d0fb9ed2b9c787325844cee4
        {
            return View();
        }

<<<<<<< HEAD
        public ActionResult MoPhong() // mô phỏng trực quan
        {
            return View();
        }
         
        public ActionResult UngDung()
        {
=======
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

>>>>>>> 7a5034054b259649d0fb9ed2b9c787325844cee4
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
<<<<<<< HEAD

        
        }
=======
    }
>>>>>>> 7a5034054b259649d0fb9ed2b9c787325844cee4
}
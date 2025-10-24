using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Prim_Kruskal_Web.Controllers
{
    public class TourController : Controller
    {
        // GET: Tour: Nhận yêu cầu về tour du lịch, gọi dịch vụ tối ưu tour (sử dụng cả DB và API), trả về kết quả
        public ActionResult Index()
        {
            return View();
        }
    }
}
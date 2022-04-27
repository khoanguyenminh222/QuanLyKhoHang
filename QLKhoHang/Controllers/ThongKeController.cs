using QLKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QLKhoHang.Controllers
{
    public class ThongKeController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();
        // GET: ThongKe
        public ActionResult Index()
        {
            if (Session["Username"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        [HttpPost]
        public ActionResult Index(DateTime dateFrom, DateTime dateTo)
        {
            dateTo = dateTo.AddDays(1);
            var hoadon = db.HoaDons.Include(m=>m.user);

            return View(hoadon.Where(m=>m.ngayTao>=dateFrom && m.ngayTao<= dateTo).ToList());
        }
    }
}

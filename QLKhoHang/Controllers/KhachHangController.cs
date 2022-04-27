using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QLKhoHang.Models;

namespace QLKhoHang.Controllers
{
    public class KhachHangController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();

        // GET: KhachHang
        public ActionResult Index(int? page, string searchString)
        {
            if (Session["Username"] != null)
            {
                var khachhang = db.KhachHangs.ToList();
                if (!String.IsNullOrEmpty(searchString))
                {
                    khachhang = khachhang.Where(s => s.tenKH.ToLower().Contains(searchString.ToLower())).ToList();
                }
                if (page > 0)
                    page = page;
                else
                    page = 1; //set default page=1
                int limit = 6;
                int start = (int)(page - 1) * limit;
                int totalProduct = khachhang.Count();
                ViewBag.totalProduct = totalProduct;
                ViewBag.pageCurrent = page;
                float numberPage = (float)totalProduct / limit;
                ViewBag.numberPage = (int)Math.Ceiling(numberPage);
                var dataKhachHang = khachhang.OrderByDescending(s => s.maKH).Skip(start).Take(limit);
                return View(dataKhachHang.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // GET: KhachHang/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // GET: KhachHang/Create
        public ActionResult Create()
        {
            if (Session["Username"] != null)
            {
                ViewBag.maMH = new SelectList(db.KhachHangs, "maKH");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // POST: KhachHang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "maKH,tenKH,diaChiKH,dienThoaiKH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.KhachHangs.Add(khachHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(khachHang);
        }

        // GET: KhachHang/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            ViewBag.maKH = new SelectList(db.KhachHangs, "maKH", "tenKH", khachHang.maKH);
            return View(khachHang);
        }

        // POST: KhachHang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "maKH,tenKH,diaChiKH,dienThoaiKH")] KhachHang khachHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(khachHang);
        }

        // GET: KhachHang/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            KhachHang khachHang = db.KhachHangs.Find(id);
            if (khachHang == null)
            {
                return HttpNotFound();
            }
            return View(khachHang);
        }

        // POST: KhachHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var hoadons = db.HoaDons.ToList();
            
            KhachHang khachHang = db.KhachHangs.Find(id);
            foreach (HoaDon hoadon in hoadons)
            {
                if (hoadon.maKH == khachHang.maKH)
                {
                    ViewBag.tontaiHD = "Khách hàng đang tồn tại trong hoá đơn";
                    return View(khachHang);
                }
            }
            db.KhachHangs.Remove(khachHang);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

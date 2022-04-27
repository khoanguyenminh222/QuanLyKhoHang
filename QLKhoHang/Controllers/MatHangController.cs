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
    public class MatHangController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();

        // GET: MatHang
        public ActionResult Index(int? page)
        {
            if (Session["Username"] != null)
            {
                var mathang = db.MatHangs.ToList();
                if (page > 0)
                    page = page;
                else
                    page = 1; //set default page=1
                int limit = 6;
                int start = (int)(page - 1) * limit;
                int totalProduct = mathang.Count();
                ViewBag.totalProduct = totalProduct;
                ViewBag.pageCurrent = page;
                float numberPage = (float)totalProduct / limit;
                ViewBag.numberPage = (int)Math.Ceiling(numberPage);
                var dataMatHang = mathang.OrderByDescending(s => s.maMH).Skip(start).Take(limit);
                return View(dataMatHang.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // GET: MatHang/Create
        public ActionResult Create()
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

        // POST: MatHang/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "maMH,loaiMH")] MatHang matHang)
        {
            if (ModelState.IsValid)
            {
                db.MatHangs.Add(matHang);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(matHang);
        }

        // GET: MatHang/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            MatHang matHang = db.MatHangs.Find(id);
            if (matHang == null)
            {
                return RedirectToAction("Index");
            }
            return View(matHang);
        }

        // POST: MatHang/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "maMH,loaiMH")] MatHang matHang)
        {
            if (ModelState.IsValid)
            {
                db.Entry(matHang).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(matHang);
        }

        // GET: MatHang/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            MatHang matHang = db.MatHangs.Find(id);
            if (matHang == null)
            {
                return RedirectToAction("Index");
            }
            return View(matHang);
        }

        // POST: MatHang/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MatHang matHang = db.MatHangs.Find(id);
            var sanPham = db.SanPhams.ToList();
            foreach (var s in sanPham)
            {
                if (s.maMH == matHang.maMH)
                {
                    ViewBag.tontaima = "Đang tồn tại sản phẩm chứa loại mặt hàng này";
                    return View(matHang);
                }
            }
            db.MatHangs.Remove(matHang);
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

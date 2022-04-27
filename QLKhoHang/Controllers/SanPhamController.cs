using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using QLKhoHang.Models;

namespace QLKhoHang.Controllers
{
    public class SanPhamController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();

        // GET: SanPham
        public ActionResult Index(int? page, string searchString)
        {
            if (Session["Username"] != null)
            {
                var sanPhams = db.SanPhams.Include(s => s.MatHang);
                if (!String.IsNullOrEmpty(searchString))
                {
                    sanPhams = sanPhams.Where(s => s.tenSP.Contains(searchString) || s.MatHang.loaiMH.Contains(searchString));
                }
                if (page > 0)
                    page = page;
                else
                    page = 1; //set default page=1
                int limit = 6;
                int start = (int)(page - 1) * limit;
                int totalProduct = sanPhams.Count();
                ViewBag.totalProduct = totalProduct;
                ViewBag.pageCurrent = page;
                float numberPage = (float)totalProduct / limit;
                ViewBag.numberPage = (int)Math.Ceiling(numberPage);
                var dataSanPham = sanPhams.OrderByDescending(s => s.maSP).Skip(start).Take(limit);
                return View(dataSanPham.ToList());
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
            
        }

        // GET: SanPham/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // GET: SanPham/Create
        public ActionResult Create()
        {
            if (Session["Username"] != null)
            {
                ViewBag.maMH = new SelectList(db.MatHangs, "maMH", "loaiMH");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // POST: SanPham/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "maSP,tenSP,maMH,noiNhap,ngayNhap,giaTien,tonKho")] SanPham sanPham, HttpPostedFileBase image)
        {
            if (sanPham.tonKho <= 0 || sanPham.giaTien <= 0)
            {
                ViewBag.error = "Lỗi nhập số";
            }
            
            if (ModelState.IsValid)
            {
                if (image != null)
                {
                    sanPham.hinhAnh = new byte[image.ContentLength];
                }
                else
                {
                    ViewBag.hinhanh = "Chưa có hình ảnh";
                    ViewBag.maMH = new SelectList(db.MatHangs, "maMH", "loaiMH", sanPham.maMH);
                    return View(sanPham);
                }
                    
                image.InputStream.Read(sanPham.hinhAnh,0,image.ContentLength);
                db.SanPhams.Add(sanPham);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            // lấy ra hình ảnh được gửi

            ViewBag.maMH = new SelectList(db.MatHangs, "maMH", "loaiMH", sanPham.maMH);
            return View(sanPham);
        }
        // GET: SanPham/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            ViewBag.maMH = new SelectList(db.MatHangs, "maMH", "loaiMH", sanPham.maMH);
            return View(sanPham);
        }

        // POST: SanPham/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "maSP,tenSP,maMH,noiNhap,ngayNhap,giaTien,tonKho")] SanPham sanPham, HttpPostedFileBase image)
        {
            if (sanPham.tonKho <= 0 || sanPham.giaTien <= 0)
            {
                ViewBag.error = "Lỗi nhập số";
            }
            if (ModelState.IsValid)
            {
                var sp = db.SanPhams.Find(sanPham.maSP);
                if (image != null)
                {
                    sanPham.hinhAnh = new byte[image.ContentLength];
                    image.InputStream.Read(sanPham.hinhAnh, 0, image.ContentLength);
                    sp.hinhAnh = sanPham.hinhAnh;
                }
                
                sp.tenSP = sanPham.tenSP;
                sp.maMH = sanPham.maMH;
                sp.noiNhap = sanPham.noiNhap;
                sp.ngayNhap = sanPham.ngayNhap;
                sp.giaTien = sanPham.giaTien;
                sp.tonKho = sanPham.tonKho;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.maMH = new SelectList(db.MatHangs, "maMH", "loaiMH", sanPham.maMH);
            return View(sanPham);
        }

        // GET: SanPham/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            SanPham sanPham = db.SanPhams.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }
            return View(sanPham);
        }

        // POST: SanPham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SanPham sanPham = db.SanPhams.Find(id);
            var chitietHD = db.ChiTietHoaDons.ToList();
            foreach(var c in chitietHD)
            {
                if (c.maSP == sanPham.maSP)
                {
                    ViewBag.tontaiSP = "Tồn tại hoá đơn chứa sản phẩm";
                    return View(sanPham);
                }
            }
            db.SanPhams.Remove(sanPham);
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

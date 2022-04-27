using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;
using QLKhoHang.Models;

namespace QLKhoHang.Controllers
{
    public class HoaDonController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();

        // GET: HoaDon
        public ActionResult Index(int? page1, int? page2, string SearchString1, string SearchString2)
        {
            if (Session["Username"] != null)
            {
                var hoaDons = db.HoaDons.Include(h => h.KhachHang);
                var hoadon1 = hoaDons.Where(s => s.trangthai == "Chưa thanh toán").ToList();
                if (!String.IsNullOrEmpty(SearchString1))
                {
                    hoadon1 = hoadon1.Where(s => s.KhachHang.tenKH.ToLower().Contains(SearchString1.ToLower()) || s.KhachHang.dienThoaiKH.Contains(SearchString1)).ToList();
                }

                if (page1 > 0)
                    page1 = page1;
                else
                    page1 = 1; //set default page=1
                int limit1 = 5;
                int start1 = (int)(page1 - 1) * limit1;
                int totalProduct1 = hoadon1.Count();
                ViewBag.pageCurrent1 = page1;
                float numberPage1 = (float)totalProduct1 / limit1;
                ViewBag.numberPage1 = (int)Math.Ceiling(numberPage1);
                var dataHoaDon1 = hoadon1.OrderByDescending(s => s.maHD).Skip(start1).Take(limit1);

                var hoadon2 = hoaDons.Include(h=>h.user).Where(s => s.trangthai == "Đã thanh toán").ToList();
                if (!String.IsNullOrEmpty(SearchString2))
                {
                    hoadon2 = hoadon2.Where(s => s.KhachHang.tenKH.ToLower().Contains(SearchString2.ToLower()) || s.KhachHang.dienThoaiKH.Contains(SearchString2)).ToList();
                }
                if (page2 > 0)
                    page2 = page2;
                else
                    page2 = 1; //set default page=1
                int limit2 = 5;
                int start2 = (int)(page2 - 1) * limit2;
                int totalProduct2 = hoadon2.Count();
                ViewBag.pageCurrent2 = page2;
                float numberPage2 = (float)totalProduct2 / limit2;
                ViewBag.numberPage2 = (int)Math.Ceiling(numberPage2);
                var dataHoaDon2 = hoadon2.OrderByDescending(s => s.maHD).Skip(start2).Take(limit2).ToList();

                HoaDonPagination hoaDonPagination = new HoaDonPagination
                {
                    page1 = dataHoaDon1.ToList(),
                    page2 = dataHoaDon2.ToList()
                };

                return View(hoaDonPagination);
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
            
        }

        // GET: HoaDon/Create
        public ActionResult Create()
        {
            if (Session["Username"] != null)
            {
                ViewBag.maKH = new SelectList(db.KhachHangs, "maKH", "tenKH");
                return View();
            }
            else
            {
                return RedirectToAction("Login", "Users");
            }
        }

        // POST: HoaDon/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "maHD,maKH,ngayTao,giamgia,tongTien,tiencantra,trangthai")] HoaDon hoaDon)
        {
            hoaDon.tongTien = 0;
            hoaDon.tiencantra = 0;
            hoaDon.trangthai="Chưa thanh toán";
            hoaDon.giamgia = 0;
            hoaDon.ngayTao = DateTime.Now;
            if (ModelState.IsValid)
            {
                db.HoaDons.Add(hoaDon);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.maKH = new SelectList(db.KhachHangs, "maKH", "tenKH", hoaDon.maKH);
            return View(hoaDon);
        }

        // GET: HoaDon/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            ViewBag.maKH = new SelectList(db.KhachHangs, "maKH", "tenKH", hoaDon.maKH);
            return View(hoaDon);
        }

        // POST: HoaDon/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "maHD,maKH,ngayTao,giamgia,tongTien,tiencantra,trangthai")] HoaDon hoaDon)
        {
            if (ModelState.IsValid)
            {
                db.Entry(hoaDon).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.maKH = new SelectList(db.KhachHangs, "maKH", "tenKH", hoaDon.maKH);
            return View(hoaDon);
        }

        // GET: HoaDon/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            HoaDon hoaDon = db.HoaDons.Find(id);
            if (hoaDon == null)
            {
                return HttpNotFound();
            }
            return View(hoaDon);
        }

        // POST: HoaDon/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            HoaDon hoaDon = db.HoaDons.Find(id);
            var chiTietHoaDon = db.ChiTietHoaDons.Where(m => m.maHD == id);
            if (hoaDon.trangthai == "Chưa thanh toán")
            {
                foreach (var chitiet in chiTietHoaDon)
                {
                    var sanpham = db.SanPhams.Find(chitiet.maSP);
                    var tonkho = sanpham.tonKho + chitiet.soLuong;
                    sanpham.tonKho = tonkho;

                    db.ChiTietHoaDons.Remove(chitiet);
                }
                db.HoaDons.Remove(hoaDon);
                db.SaveChanges();
            }
            else
            {
                foreach (var chitiet in chiTietHoaDon)
                {
                    db.ChiTietHoaDons.Remove(chitiet);
                }
                db.HoaDons.Remove(hoaDon);
                db.SaveChanges();
            }
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

        [HttpGet]
        public ActionResult Export(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            HoaDon hoadon = db.HoaDons.Include(h=>h.user).SingleOrDefault(h=>h.maHD==id);
            KhachHang khachHang = db.KhachHangs.Find(hoadon.maKH);
            var chitiet = db.ChiTietHoaDons.Where(m => m.maHD == id).ToList();
            HoaDonViewModel hoaDonViewModel = new HoaDonViewModel()
            {
                KhachHang = khachHang,
                HoaDons = hoadon,
                ChiTietHoaDons = chitiet
            };
            return View(hoaDonViewModel);
        }
        [HttpPost]
        public ActionResult Export(int id)
        {
            PdfDocument document = new PdfDocument();
            document.Info.Title = "Table Example";
            for (int p = 0; p < 1; p++)
            {
                // Page Options
                PdfPage pdfPage = document.AddPage();
                pdfPage.Height = 842;//842
                pdfPage.Width = 590;

                // Get an XGraphics object for drawing
                XGraphics graph = XGraphics.FromPdfPage(pdfPage);

                // Text format
                XStringFormat format = new XStringFormat();
                format.LineAlignment = XLineAlignment.Near;
                format.Alignment = XStringAlignment.Near;
                var tf = new XTextFormatter(graph);

                XFont fontParagraph = new XFont("Verdana", 8, XFontStyle.Regular);

                // Row elements
                int el1_width = 80;
                int el2_width = 380;

                // page structure options
                double lineHeight = 20;
                int marginLeft = 20;
                int marginTop = 80;

                int el_height = 30;
                int rect_height = 17;

                int interLine_X_1 = 2;
                int interLine_X_2 = 2 * interLine_X_1;

                int offSetX_1 = el1_width;
                int offSetX_2 = el1_width + el2_width;

                XSolidBrush rect_style1 = new XSolidBrush(XColors.LightGray);
                XSolidBrush rect_style2 = new XSolidBrush(XColors.DarkGreen);
                XSolidBrush rect_style3 = new XSolidBrush(XColors.Red);

                graph.DrawString("Hoá đơn", new XFont("Verdana", 16, XFontStyle.Regular), XBrushes.Black, new XRect(marginLeft, 20, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopCenter);
                graph.DrawString("Mã hoá đơn:" + (id).ToString(), fontParagraph, XBrushes.Black, new XRect(marginLeft, 40, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                graph.DrawString("Ngày tạo:" + db.HoaDons.Find(id).ngayTao.ToString(), fontParagraph, XBrushes.Black, new XRect(marginLeft+80, 40, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                graph.DrawString("Nhân viên:" + db.users.Find(db.HoaDons.Find(id).tenNV).tenNV.ToString(), fontParagraph, XBrushes.Black, new XRect(marginLeft, 50, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                graph.DrawString("Tên khách hàng: " + db.KhachHangs.Find(db.HoaDons.Find(id).maKH).tenKH.ToString(), fontParagraph, XBrushes.Black, new XRect(marginLeft, 60, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                graph.DrawString("Số điện thoại: " + db.KhachHangs.Find(db.HoaDons.Find(id).maKH).dienThoaiKH.ToString(), fontParagraph, XBrushes.Black, new XRect(marginLeft, 70, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);
                int i = 0;

                foreach (var chitiet in db.ChiTietHoaDons.Where(m => m.maHD == id))
                {
                    double dist_Y = lineHeight * (i + 1);
                    double dist_Y2 = dist_Y - 2;
                    if (i == 0)
                    {
                        graph.DrawRectangle(rect_style2, marginLeft, marginTop, pdfPage.Width - 2 * marginLeft, rect_height);
                        tf.DrawString("Số thứ tự", fontParagraph, XBrushes.White,
                                        new XRect(marginLeft, marginTop, el1_width, el_height), format);
                        tf.DrawString("Tên sản phẩm", fontParagraph, XBrushes.White,
                                        new XRect(marginLeft + offSetX_1, marginTop, el1_width, el_height), format);
                        tf.DrawString("Đơn giá", fontParagraph, XBrushes.White,
                                        new XRect(marginLeft + offSetX_1 * 3, marginTop, el1_width, el_height), format);
                        tf.DrawString("Số lượng", fontParagraph, XBrushes.White,
                                            new XRect(marginLeft + offSetX_1 * 4, marginTop, el1_width, el_height), format);
                        tf.DrawString("Thanh tiền", fontParagraph, XBrushes.White,
                                            new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, marginTop, el1_width, el_height), format);
                        // stampo il primo elemento insieme all'header
                        graph.DrawRectangle(rect_style1, marginLeft, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString((i + 1).ToString(), fontParagraph, XBrushes.Black,
                                        new XRect(marginLeft, dist_Y + marginTop, el1_width, el_height), format);

                        //ELEMENT 2 - BIG 380
                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1, dist_Y2 + marginTop, el1_width * 2 - 5, rect_height);
                        tf.DrawString(
                            chitiet.SanPham.tenSP,
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1, dist_Y + marginTop, el2_width, el_height),
                            format);
                        //ELEMENT 3 - BIG 380
                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + el1_width + 76, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString(
                            chitiet.giaTien.ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1 + 160, dist_Y + marginTop, el2_width, el_height),
                            format);


                        //ELEMENT 4 - SMALL 80

                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + 239, dist_Y2 + marginTop, el1_width + 60, rect_height);
                        tf.DrawString(
                            chitiet.soLuong.ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1 + 240, dist_Y + marginTop, el1_width, el_height),
                            format);

                        //ELEMENT 5 - SMALL 80

                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString(
                            (chitiet.soLuong * chitiet.giaTien).ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, dist_Y + marginTop, el1_width, el_height),
                            format);

                    }
                    else
                    {
                        // stampo il primo elemento insieme all'header
                        graph.DrawRectangle(rect_style1, marginLeft, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString((i + 1).ToString(), fontParagraph, XBrushes.Black,
                                        new XRect(marginLeft, dist_Y + marginTop, el1_width, el_height), format);

                        //ELEMENT 2 - BIG 380
                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1, dist_Y2 + marginTop, el1_width * 2 - 5, rect_height);
                        tf.DrawString(
                            chitiet.SanPham.tenSP,
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1, dist_Y + marginTop, el2_width, el_height),
                            format);
                        //ELEMENT 3 - BIG 380
                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + el1_width + 76, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString(
                            chitiet.giaTien.ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1 + 160, dist_Y + marginTop, el2_width, el_height),
                            format);


                        //ELEMENT 4 - SMALL 80

                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + 239, dist_Y2 + marginTop, el1_width + 60, rect_height);
                        tf.DrawString(
                            chitiet.soLuong.ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_1 + interLine_X_1 + 240, dist_Y + marginTop, el1_width, el_height),
                            format);

                        //ELEMENT 5 - SMALL 80

                        graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop, el1_width, rect_height);
                        tf.DrawString(
                            (chitiet.soLuong * chitiet.giaTien).ToString(),
                            fontParagraph,
                            XBrushes.Black,
                            new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, dist_Y + marginTop, el1_width, el_height),
                            format);
                    }

                    i += 1;
                    //ELEMENT 4 - SMALL 80
                    graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + 239, dist_Y2 + marginTop + 20, el1_width + 60, rect_height);
                    tf.DrawString(
                        "Giảm giá",
                        fontParagraph,
                        XBrushes.Black,
                        new XRect(marginLeft + offSetX_1 + interLine_X_1 + 240, dist_Y + marginTop + 20, el1_width, el_height),
                        format);

                    //ELEMENT 5 - SMALL 80
                    graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop + 20, el1_width, rect_height);
                    tf.DrawString(
                        "-" + db.HoaDons.Find(id).giamgia.ToString(),
                        fontParagraph,
                        XBrushes.Black,
                        new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, dist_Y + marginTop + 20, el1_width, el_height),
                        format);

                    //ELEMENT 4 - SMALL 80
                    graph.DrawRectangle(rect_style1, marginLeft + offSetX_1 + interLine_X_1 + 239, dist_Y2 + marginTop + 40, el1_width + 60, rect_height);
                    tf.DrawString(
                        "Thành tiền",
                        fontParagraph,
                        XBrushes.Black,
                        new XRect(marginLeft + offSetX_1 + interLine_X_1 + 240, dist_Y + marginTop + 40, el1_width, el_height),
                        format);

                    //ELEMENT 5 - SMALL 80
                    graph.DrawRectangle(rect_style1, marginLeft + offSetX_2 + interLine_X_2, dist_Y2 + marginTop + 40, el1_width, rect_height);
                    tf.DrawString(
                        db.HoaDons.Find(id).tiencantra.ToString(),
                        fontParagraph,
                        XBrushes.Black,
                        new XRect(marginLeft + offSetX_2 + 2 * interLine_X_2, dist_Y + marginTop + 40, el1_width, el_height),
                        format);

                }

            }
            DateTime date = DateTime.Now;
            string filename = db.KhachHangs.Find(db.HoaDons.Find(id).maKH).tenKH.ToString() + String.Concat(date.ToString("dddd, dd MMMM yyyy HH:mm:ss"), "export.pdf").Replace(":", "-");
            ViewBag.xuathoadon = "Xuất hoá đơn thành công";
            HoaDon hoadon = db.HoaDons.Include(h => h.user).SingleOrDefault(h => h.maHD == id);
            KhachHang khachHang = db.KhachHangs.Find(hoadon.maKH);
            var chitiethd = db.ChiTietHoaDons.Where(m => m.maHD == id).ToList();
            HoaDonViewModel hoaDonViewModel = new HoaDonViewModel()
            {
                KhachHang = khachHang,
                HoaDons = hoadon,
                ChiTietHoaDons = chitiethd
            };
            var path = Path.Combine(Server.MapPath("~/Export"), filename);
            document.Save(path);
            return View(hoaDonViewModel);
        }

        [HttpGet]
        public ActionResult Details(int? id, int? page, string searchString)
        {
            if (id == null)
            {
                return Redirect("Index");
            }
            var hoadon = db.HoaDons.Find(id);
            var khachhang = db.KhachHangs.Find(hoadon.maKH);
            var sanPham = db.SanPhams.Include(s => s.MatHang);

            if (!String.IsNullOrEmpty(searchString))
            {
                sanPham = sanPham.Where(s => s.tenSP.Contains(searchString) || s.MatHang.loaiMH.Contains(searchString));
            }
            if (page > 0)
                page = page;
            else
                page = 1; //set default page=1
            int limit = 5;
            int start = (int)(page - 1) * limit;
            int totalProduct = sanPham.Count();
            ViewBag.totalProduct = totalProduct;
            ViewBag.pageCurrent = page;
            float numberPage = (float)totalProduct / limit;
            ViewBag.numberPage = (int)Math.Ceiling(numberPage);
            var dataSanPham = sanPham.OrderByDescending(s => s.maSP).Skip(start).Take(limit);

            var chitiet = db.ChiTietHoaDons.Where(m => m.maHD == id).ToList();
            HoaDonViewModel hoaDonViewModel = new HoaDonViewModel()
            {
                HoaDons = hoadon,
                KhachHang = khachhang,
                SanPhams = dataSanPham.ToList(),
                ChiTietHoaDons = chitiet
            };
            return View(hoaDonViewModel);
        }
        [HttpPost]
        public ActionResult Details(int id)
        {
            db.HoaDons.SingleOrDefault(m => m.maHD == id).trangthai = "Đã thanh toán";
            db.HoaDons.SingleOrDefault(m => m.maHD == id).tenNV = (string)Session["Username"];
            db.SaveChanges();
            return RedirectToAction("Export/" + id);
        }

        [HttpPost]
        public JsonResult AddColumnProduct(int id, int maHD, int soluong, string count)
        {
            var sp = db.SanPhams.Find(id);
            // tăng biến đếm
            int dem;
            if (count == null)
            {
                dem = 1;
            }
            else
            {
                dem = Int32.Parse(count);
                dem = dem + 1;
            }

            if (sp == null)
            {
                return Json(new { isvalid = false, msg = "Không thành công" });
            }
            // kiểm tra số lượng mua với tồn kho
            if (soluong > sp.tonKho || sp.tonKho == 0)
            {
                return Json(new { isvalid = false, msg = "Số lượng không đủ" });
            }
            if (soluong <= 0)
            {
                return Json(new { isvalid = false, msg = "Số lượng không hợp lệ" });
            }
            ChiTietHoaDon chiTietHoaDon = new ChiTietHoaDon()
            {
                maSP = sp.maSP,
                maMH = sp.MatHang.maMH,
                maHD = maHD,
                giaTien = sp.giaTien,
                soLuong = soluong
            };
            db.ChiTietHoaDons.Add(chiTietHoaDon);
            var tonkho = sp.tonKho - soluong;
            db.SanPhams.SingleOrDefault(m => m.maSP == id).tonKho = tonkho;
            db.SaveChanges();
            return Json(new { isvalid = true, msg = "Thêm thành công", tenSP = chiTietHoaDon.SanPham.tenSP, loaiMH = chiTietHoaDon.MatHang.loaiMH, giaTien = chiTietHoaDon.SanPham.giaTien, soluong = chiTietHoaDon.soLuong, maCTHD = chiTietHoaDon.maCTHD, dem = dem });
        }

        [HttpPost]
        public JsonResult DelColumnProduct(int id)
        {
            var chitiethd = db.ChiTietHoaDons.FirstOrDefault(l => l.maCTHD == id);
            if (chitiethd == null)
            {
                return Json(new { isvalid = false, msg = "Không tìm thấy" });
            }
            var sanpham = db.SanPhams.SingleOrDefault(m => m.maSP == chitiethd.maSP);
            var tonkho = sanpham.tonKho + chitiethd.soLuong;
            sanpham.tonKho = tonkho;
            db.ChiTietHoaDons.Remove(chitiethd);
            db.SaveChanges();
            return Json(new { isvalid = true, msg = "Đã xoá" });
        }

        public JsonResult LoadTongTien(int maHD)
        {

            if (!db.ChiTietHoaDons.Where(m => m.maHD == maHD).Any())
            {
                return Json(db.HoaDons.ToList().Where(m => m.maHD == maHD).Select(m => new { tongtien = 0, maHD = m.maHD, giamgia = 0, tiencantra = 0 }), JsonRequestBehavior.AllowGet);
            }
            else
            {
                var tongtien = db.ChiTietHoaDons.Where(m => m.maHD == maHD).Sum(m => m.giaTien * m.soLuong);
                int giamgiaINT;
                var giamgia = int.TryParse(db.HoaDons.Where(m => m.maHD == maHD).Select(m => m.giamgia).ToString(), out giamgiaINT);
                int tiencantraINT;
                var tiencantra = int.TryParse(db.HoaDons.Where(m => m.maHD == maHD).Select(m => m.tiencantra).ToString(), out tiencantraINT);
                var hoadon = db.HoaDons.Find(maHD);
                hoadon.tongTien = tongtien;
                hoadon.tiencantra = tiencantraINT;
                hoadon.giamgia = giamgiaINT;
                //db.SaveChanges();
                return Json(db.HoaDons.ToList().Where(m => m.maHD == maHD).Select(m => new { tongtien = m.tongTien, maHD = m.maHD, giamgia = m.giamgia, tiencantra = m.tiencantra }), JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult UpdateTongTien(int maHD, int giamgia, int tiencantra, int tongtien)
        {
            var hd = db.HoaDons.SingleOrDefault(m => m.maHD == maHD);
            if (hd != null)
            {
                hd.giamgia = giamgia;
                hd.tiencantra = tiencantra;
                hd.tongTien = tongtien;
                db.SaveChanges();
            }
            return Json(new { giamgia = giamgia, tiencantra = tiencantra });
        }

        public JsonResult LoadTonKho()
        {
            return Json(db.SanPhams.ToList().Select(m => new {
                maSP = m.maSP,
                tenSP = m.tenSP,
                giaTien = m.giaTien,
                tonkho = m.tonKho
            }), JsonRequestBehavior.AllowGet);
        }

    }
}

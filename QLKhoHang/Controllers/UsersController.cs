using QLKhoHang.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace QLKhoHang.Controllers
{
    public class UsersController : Controller
    {
        private KhoHangEntities db = new KhoHangEntities();

        //Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Username, string Password)
        {
            if (ModelState.IsValid)
            {

                var data = db.users.Where(s => s.Username == Username && s.Password == Password).ToList();
                Console.WriteLine(data);
                if (data.Count() > 0)
                {
                    Session["Username"] = data.FirstOrDefault().Username;
                    Session["name"] = data.FirstOrDefault().tenNV;
                    return RedirectToAction("Index", "SanPham");
                }
                else if (data.Count() <= 0)
                {
                    ViewBag.fail = "Tên đăng nhập hoặc mật khẩu không chính xác";
                }

            }
            return View();

        }

        //Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }


        public ActionResult Index()
        {
            if (Session["Username"] != null)
            {
               
                return View(db.users.Where(m => m.Username != "admin").ToList());
            }
            else
            {
                return Redirect("login");
            }
                
        }


        // GET: users1/Create
        public ActionResult Create()
        {
            if (Session["Username"] == null)
            {

                return Redirect("login");
            }
            else
            {
                return View();
            }
            
        }

        // POST: users1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Username,Password,tenNV,diachiNV,dienthoaiNV")] user user)
        {
            if (ModelState.IsValid)
            {
                user.Password = "123456";
                db.users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // GET: users1/Edit/5
        public ActionResult Edit(string id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("login");
            }
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: users1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Username,Password,tenNV,diachiNV,dienthoaiNV")] user user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [HttpGet]
        public ActionResult ResetPassword(string id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("login");
            }
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword([Bind(Include = "Username,Password,tenNV,diachiNV,dienthoaiNV")] user user)
        {
            if (ModelState.IsValid)
            {
                user.Password = "123456";
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: users1/Delete/5
        public ActionResult Delete(string id)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("login");
            }
            if (id == null)
            {
                return RedirectToAction("Index");
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: users1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            user user = db.users.Find(id);
            var hoadons = db.HoaDons.ToList();
            foreach(HoaDon hoaDon in hoadons)
            {
                if (hoaDon.tenNV == user.Username)
                {
                    ViewBag.ErrorNV = "Đang tồn tại hoá đơn chứa nhân viên này";
                    return View(db.users.Find(id));
                }
            }
            db.users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ChangePassword(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: users1/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword([Bind(Include = "Username,Password,tenNV,diachiNV,dienthoaiNV")] user user, string newPass, string confirmPass, string oldPassword)
        {
            if (oldPassword != user.Password)
            {
                ViewBag.passOld = "Mật khẩu hiện tại không chính xác";
                return View(user);
            }
            if (newPass != confirmPass)
            {
                ViewBag.xacnhan = "Xác nhận mật khẩu không chính xác";
                return View(user);
            }
            if (ModelState.IsValid)
            {
                user.Password = newPass;
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index","SanPham");
            }
            return View(user);
        }
    }
}
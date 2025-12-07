using SofiaCosmetics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly QLMyPhamEntities db = new QLMyPhamEntities();

        [HttpGet]
        public ActionResult Login()
        {
            if (Session["ADMIN_LOGIN"] != null)
                return RedirectToAction("Index", "TrangChu", new { area = "Admin" });

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string tenDangNhap, string matKhau)
        {
            tenDangNhap = (tenDangNhap ?? "").Trim();

            var admin = db.ADMINs.FirstOrDefault(a =>
                        a.TenDangNhap == tenDangNhap || a.Email == tenDangNhap);

            if (admin == null)
            {
                ViewBag.Error = "Tài khoản không tồn tại!";
                return View();
            }

            if (admin.TrangThai != true)
            {
                ViewBag.Error = "Tài khoản đã bị khóa!";
                return View();
            }

            if (admin.MatKhau != matKhau)
            {
                ViewBag.Error = "Sai mật khẩu!";
                return View();
            }

            Session["ADMIN_LOGIN"] = admin.MaAdmin;
            Session["ADMIN_NAME"] = admin.HoTen;
            Session["ADMIN_ROLE"] = admin.VaiTro;

            return RedirectToAction("Index", "TrangChu", new { area = "Admin" });
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
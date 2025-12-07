using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Newtonsoft.Json;
using SofiaCosmetics.Helpers;
using SofiaCosmetics.Models;

namespace SofiaCosmetics.Controllers
{
    public class UserController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();

        //  LOGIN 
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string Email, string MatKhau)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu.";
                return View();
            }

            string mkMaHoa = PasswordHelper.HashSHA256(MatKhau);

            var kh = db.KHACHHANGs
                .FirstOrDefault(k => k.Email == Email && k.MatKhau == mkMaHoa && k.TrangThai == true);

            if (kh != null)
            {
                Session["KH"] = kh;
                Session["MaKH"] = kh.MaKH;
                Session["HoTen"] = kh.HoTen;
                Session["Email"] = kh.Email;
                Session["NgaySinh"] = kh.NgaySinh;


                FormsAuthentication.SetAuthCookie(kh.Email, false);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác.";
            return View();
        }

        //  REGISTER 
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(KHACHHANG model, string XacNhanMatKhau)
        {
            try
            {
                ModelState.Clear();

                if (model.MatKhau != XacNhanMatKhau)
                {
                    TempData["Error"] = "Mật khẩu xác nhận không khớp.";
                    return RedirectToAction("Register");
                }

                if (db.KHACHHANGs.Any(k => k.Email == model.Email))
                {
                    TempData["Error"] = "Email đã tồn tại.";
                    return RedirectToAction("Register");
                }

                model.MatKhau = PasswordHelper.HashSHA256(model.MatKhau);
                model.TrangThai = true;
                model.NgayTao = DateTime.Now;

                db.KHACHHANGs.Add(model);
                db.SaveChanges();

                SendEmail(model);

                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            catch
            {
                TempData["Error"] = "Có lỗi xảy ra khi đăng ký.";
                return RedirectToAction("Register");
            }
        }


        //  EMAIL HTML 
        public void SendEmail(KHACHHANG kh)
        {
            var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                    "2324801030035@student.tdmu.edu.vn",
                    "lvoy grxj rqik dfej")
            };

            string body = $@"
            <div style='font-family:Arial; padding:20px; line-height:1.6;'>
                <h2 style='color:#28a745;'>🎉 Chúc mừng! Bạn đã đăng ký thành công</h2>
                <p>Xin chào <strong>{kh.HoTen}</strong>,</p>
                <p>Cảm ơn bạn đã trở thành thành viên của 
                    <strong style='color:#e91e63;'>SofiaCosmetics</strong>.
                </p>

                <h3 style='margin-top:30px;'>📌 Thông tin tài khoản của bạn</h3>

                <table style='border-collapse:collapse; width:100%; max-width:500px;'>
                    <tr><td>Họ và tên:</td><td>{kh.HoTen}</td></tr>
                    <tr><td>Email:</td><td>{kh.Email}</td></tr>
                    <tr><td>Số điện thoại:</td><td>{kh.SDT}</td></tr>
                    <tr><td>Ngày sinh:</td><td>{kh.NgaySinh?.ToString("dd/MM/yyyy")}</td></tr>
                </table>

                <p style='margin-top:20px;'>Trân trọng,<br/>SofiaCosmetics</p>
            </div>";

            var mail = new System.Net.Mail.MailMessage();
            mail.From = new System.Net.Mail.MailAddress("2324801030035@student.tdmu.edu.vn", "SofiaCosmetics");
            mail.To.Add(kh.Email);
            mail.Subject = "Đăng ký tài khoản thành công - SofiaCosmetics";
            mail.Body = body;
            mail.IsBodyHtml = true;

            client.Send(mail);
        }

        //  LOGIN GOOGLE 
        public ActionResult LoginGoogle()
        {
            string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            string redirectUri = Url.Action("GoogleCallback", "User", null, Request.Url.Scheme);

            string url =
                "https://accounts.google.com/o/oauth2/v2/auth" +
                "?response_type=code" +
                "&client_id=" + clientId +
                "&redirect_uri=" + redirectUri +
                "&scope=openid%20email%20profile" +
                "&prompt=select_account";

            return Redirect(url);
        }

        public async Task<ActionResult> GoogleCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
                return RedirectToAction("Login");

            string clientId = ConfigurationManager.AppSettings["GoogleClientId"];
            string clientSecret = ConfigurationManager.AppSettings["GoogleClientSecret"];
            string redirectUri = Url.Action("GoogleCallback", "User", null, Request.Url.Scheme);

            using (var client = new HttpClient())
            {
                var tokenRequest = new Dictionary<string, string>
        {
            {"code", code},
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"redirect_uri", redirectUri},
            {"grant_type", "authorization_code"}
        };

                var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token",
                                 new FormUrlEncodedContent(tokenRequest));

                var json = await tokenResponse.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(json);
                string accessToken = data.access_token;

                var userInfo = await client.GetStringAsync(
                    "https://www.googleapis.com/oauth2/v2/userinfo?access_token=" + accessToken);

                dynamic user = JsonConvert.DeserializeObject(userInfo);
                string email = user.email;
                string name = user.name;

                var kh = db.KHACHHANGs.FirstOrDefault(k => k.Email == email);

                if (kh == null)
                {
                    kh = new KHACHHANG
                    {
                        Email = email,
                        HoTen = name,
                        MatKhau = "",
                        TrangThai = true,
                        NgayTao = DateTime.Now
                    };

                    db.KHACHHANGs.Add(kh);
                    db.SaveChanges();
                }
                Session["KH"] = kh;
                Session["MaKH"] = kh.MaKH;
                Session["HoTen"] = kh.HoTen;

                return RedirectToAction("Index", "Home");
            }
        }

        // Nhấn nút đăng nhập bằng Facebook
        public ActionResult LoginFacebook()
        {
            string appId = ConfigurationManager.AppSettings["FB_AppId"];
            string redirectUri = Url.Action("FacebookCallback", "User", null, Request.Url.Scheme);

            string url =
                "https://www.facebook.com/v18.0/dialog/oauth" +
                "?client_id=" + appId +
                "&redirect_uri=" + redirectUri +
                "&response_type=code" +
                "&scope=email,public_profile";

            return Redirect(url);
        }

        public async Task<ActionResult> FacebookCallback(string code)
        {
            if (string.IsNullOrEmpty(code))
                return RedirectToAction("Login");

            string appId = ConfigurationManager.AppSettings["FB_AppId"];
            string secret = ConfigurationManager.AppSettings["FB_AppSecret"];
            string redirectUri = Url.Action("FacebookCallback", "User", null, Request.Url.Scheme);

            using (var http = new HttpClient())
            {
                // Lấy token
                var tokenResponse = await http.GetStringAsync(
                    $"https://graph.facebook.com/v18.0/oauth/access_token?" +
                    $"client_id={appId}&redirect_uri={redirectUri}&client_secret={secret}&code={code}");

                dynamic token = JsonConvert.DeserializeObject(tokenResponse);
                string accessToken = token.access_token;

                // Lấy thông tin user
                var userInfo = await http.GetStringAsync(
                    $"https://graph.facebook.com/me?fields=id,name,email&access_token={accessToken}");

                dynamic user = JsonConvert.DeserializeObject(userInfo);

                string email = user.email;
                string name = user.name;

                var kh = db.KHACHHANGs.FirstOrDefault(k => k.Email == email);

                if (kh == null)
                {
                    kh = new KHACHHANG
                    {
                        Email = email,
                        HoTen = name,
                        MatKhau = "",
                        TrangThai = true,
                        NgayTao = DateTime.Now
                    };

                    db.KHACHHANGs.Add(kh);
                    db.SaveChanges();
                }
                Session["KH"] = kh;
                Session["MaKH"] = kh.MaKH;
                Session["HoTen"] = kh.HoTen;

                return RedirectToAction("Index", "Home");
            }
        }

        //  LOGOUT 
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        //  FORGOT PASSWORD 
        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string Email)
        {
            var kh = db.KHACHHANGs.FirstOrDefault(k => k.Email == Email);
            if (kh == null)
            {
                TempData["Error"] = "Email không tồn tại trong hệ thống!";
                return View();
            }

            // Tạo token reset
            string token = TokenHelper.CreateResetToken(Email);

            // Tạo link để gửi mail
            string link = Url.Action("ResetPassword", "User", new { token = token }, Request.Url.Scheme);

            // Gửi mail
            SendResetPasswordEmail(Email, kh.HoTen, link);

            TempData["Success"] = "Link đặt lại mật khẩu đã được gửi về email của bạn!";
            return RedirectToAction("ForgotPassword");
        }
        public void SendResetPasswordEmail(string email, string name, string link)
        {
            var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                    "2324801030035@student.tdmu.edu.vn",
                    "lvoy grxj rqik dfej")
            };

            string body = $@"
    <div style='font-family:Poppins;padding:20px;'>
        <h2 style='color:#e91e63;'>🔐 Đặt lại mật khẩu</h2>
        <p>Xin chào <strong>{name}</strong>,</p>
        <p>Bạn đã yêu cầu đặt lại mật khẩu. Nhấn vào nút dưới đây:</p>

        <p style='margin:25px 0;text-align:center;'>
            <a href='{link}' 
               style='padding:12px 25px;background:#e91e63;color:white;
                      border-radius:8px;text-decoration:none;font-weight:bold;'>
               ĐẶT LẠI MẬT KHẨU
            </a>
        </p>

        <p>Link sẽ hết hạn trong 30 phút.</p>
    </div>";

            var mail = new System.Net.Mail.MailMessage();
            mail.From = new System.Net.Mail.MailAddress("2324801030035@student.tdmu.edu.vn", "SofiaCosmetics");
            mail.To.Add(email);
            mail.Subject = "Đặt lại mật khẩu - SofiaCosmetics";
            mail.Body = body;
            mail.IsBodyHtml = true;

            client.Send(mail);
        }
        [HttpGet]
        public ActionResult ResetPassword(string token)
        {
            string email = TokenHelper.ValidateToken(token);

            if (email == null)
            {
                TempData["Error"] = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("ForgotPassword");
            }

            ViewBag.Token = token;
            ViewBag.Email = email;
            return View();
        }
        [HttpPost]
        public ActionResult ResetPassword(string token, string MatKhau, string XacNhanMatKhau)
        {
            string email = TokenHelper.ValidateToken(token);

            if (email == null)
            {
                TempData["Error"] = "Link không hợp lệ hoặc đã hết hạn!";
                return RedirectToAction("ForgotPassword");
            }

            if (MatKhau != XacNhanMatKhau)
            {
                TempData["Error"] = "Mật khẩu xác nhận không khớp!";
                ViewBag.Token = token;
                return View();
            }

            var kh = db.KHACHHANGs.FirstOrDefault(k => k.Email == email);
            kh.MatKhau = PasswordHelper.HashSHA256(MatKhau);
            db.SaveChanges();

            TempData["Success"] = "Đặt lại mật khẩu thành công!";
            return RedirectToAction("Login");
        }
        [HttpGet]
        public ActionResult EditInfo()
        {
            int? makh = Session["MaKH"] as int?;
            if (makh == null) return RedirectToAction("Login");

            var kh = db.KHACHHANGs.Find(makh);
            return View(kh);
        }

        [HttpPost]
        public ActionResult EditInfo(KHACHHANG model)
        {
            int? makh = Session["MaKH"] as int?;
            if (makh == null) return RedirectToAction("Login");

            var kh = db.KHACHHANGs.Find(makh);
            if (kh == null) return HttpNotFound();

            kh.HoTen = model.HoTen;
            kh.SDT = model.SDT;
            kh.NgaySinh = model.NgaySinh;
            kh.DiaChi = model.DiaChi;

            db.SaveChanges();

            // Cập nhật session
            Session["HoTen"] = kh.HoTen;
            Session["SDT"] = kh.SDT;
            Session["NgaySinh"] = kh.NgaySinh;
            Session["DiaChi"] = kh.DiaChi;

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("EditInfo");
        }

    }
}

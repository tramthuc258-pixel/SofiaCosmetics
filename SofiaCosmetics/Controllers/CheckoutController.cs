using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.ViewModels;
using System.Globalization;

namespace SofiaCosmetics.Controllers
{
    public class CheckoutController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();

        private List<CartItem> LayGioHang()
        {
            return Session["Cart"] as List<CartItem> ?? new List<CartItem>();
        }

        public ActionResult Index()
        {
            var cart = LayGioHang();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index", "Cart");
            }

            int? makh = Session["MaKH"] as int?;
            if (makh == null)
                return RedirectToAction("Login", "User");

            var kh = db.KHACHHANGs.Find(makh);

            ViewBag.HoTen = kh.HoTen;
            ViewBag.Email = kh.Email;
            ViewBag.SDT = kh.SDT;
            ViewBag.DiaChi = kh.DiaChi;

            return View(cart);
        }


        // ĐẶT HÀNG 
        [HttpPost]
        public ActionResult DatHang(string HoTen, string SDT, string Email, string DiaChi, string phuongthuc)
        {
            var cart = LayGioHang();

            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            int? makh = Session["MaKH"] as int?;
            if (makh == null)
                return RedirectToAction("Login", "User");

            // Cập nhật thông tin KH
            var kh = db.KHACHHANGs.Find(makh);
            kh.HoTen = HoTen;
            kh.SDT = SDT;
            kh.Email = Email;
            kh.DiaChi = DiaChi;
            db.SaveChanges();

            decimal tongTien = cart.Sum(x => x.ThanhTien);

            Session["Pending_MaKH"] = makh;
            Session["Pending_TongTien"] = tongTien;
            Session["Pending_Cart"] = cart;
            Session["Pending_PhuongThuc"] = phuongthuc;

            if (phuongthuc == "VNPAY")
                return RedirectToAction("PaymentVNPay");

            if (phuongthuc == "PAYOS")
                return RedirectToAction("PaymentPayOS");

            // COD
            TaoDonHang(false);

            Session["Cart"] = null;
            return RedirectToAction("ThanhCong");
        }


        //  TẠO ĐƠN HÀNG 
        private void TaoDonHang(bool daThanhToan)
        {
            var cart = Session["Pending_Cart"] as List<CartItem>;
            if (cart == null || !cart.Any()) return;

            int makh = (int)Session["Pending_MaKH"];
            var kh = db.KHACHHANGs.Find(makh);

            DONHANG dh = new DONHANG
            {
                MaKH = makh,
                NgayDat = DateTime.Now,
                TongTien = (decimal)Session["Pending_TongTien"],
                TrangThai = daThanhToan ? "Đã thanh toán" : "Chờ xác nhận"
            };

            db.DONHANGs.Add(dh);
            db.SaveChanges();

            // ⭐ LƯU ID ĐƠN HÀNG LẠI ĐỂ VIEW SỬ DỤNG
            Session["LastOrderId"] = dh.MaDH;


            foreach (var item in cart)
            {
                CHITIETDONHANG ct = new CHITIETDONHANG
                {
                    MaDH = dh.MaDH,
                    MaCTSP = item.MaCTSP,
                    SoLuong = item.SoLuong,
                    DonGia = item.Gia
                };

                db.CHITIETDONHANGs.Add(ct);

                var sp = db.CHITIET_SANPHAM.Find(item.MaCTSP);
                if (sp != null)
                    sp.SoLuongTon -= item.SoLuong;
            }

            THANHTOAN tt = new THANHTOAN
            {
                MaDH = dh.MaDH,
                PhuongThuc = Session["Pending_PhuongThuc"].ToString(),
                TrangThai = daThanhToan ? "Đã thanh toán" : "Chưa thanh toán",
                NgayThanhToan = daThanhToan ? DateTime.Now : (DateTime?)null
            };

            db.THANHTOANs.Add(tt);
            db.SaveChanges();

            SendOrderEmail(kh.Email, kh.HoTen, dh, cart);
        }


        //  TRANG THÀNH CÔNG 
        public ActionResult ThanhCong()
        {
            //  TRUYỀN ID ĐƠN HÀNG SANG VIEW
            ViewBag.OrderId = Session["LastOrderId"];

            return View();
        }


        //  VNPay 
        public ActionResult PaymentVNPay()
        {
            decimal tongTien = (decimal)Session["Pending_TongTien"];

            string url = ConfigurationManager.AppSettings["VNPAY_Url"];
            string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
            string tmnCode = ConfigurationManager.AppSettings["VNPAY_TmnCode"];
            string hashSecret = ConfigurationManager.AppSettings["VNPAY_HashSecret"];

            PayLib pay = new PayLib();
            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", ((long)tongTien * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", "VND");
            pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress());
            pay.AddRequestData("vnp_Locale", "vn");
            pay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng SofiaCosmetics");
            pay.AddRequestData("vnp_OrderType", "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

            return Redirect(paymentUrl);
        }


        //  GỬI MAIL
        public void SendOrderEmail(string email, string name, DONHANG dh, List<CartItem> cart)
        {
            var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                    "2324801030035@student.tdmu.edu.vn",
                    "lvoy grxj rqik dfej")
            };

            string itemsHtml = "";
            foreach (var i in cart)
            {
                itemsHtml += $@"
                    <tr>
                        <td>{i.TenSP} ({i.TenBienThe})</td>
                        <td>{i.SoLuong}</td>
                        <td>{i.Gia.ToString("N0")} đ</td>
                        <td>{i.ThanhTien.ToString("N0")} đ</td>
                    </tr>";
            }

            string body = $@"
                <div style='font-family:Poppins'>
                    <h2 style='color:#e91e63'>🎉 Đặt hàng thành công!</h2>
                    <p>Xin chào <strong>{name}</strong>,</p>
                    <p>Bạn vừa đặt đơn hàng #{dh.MaDH}. Chúng tôi sẽ sớm xử lý cho bạn.</p>

                    <h3>📦 Chi tiết đơn hàng</h3>
                    <table border='1' cellpadding='8' cellspacing='0' style='width:100%;border-collapse:collapse'>
                        <tr style='background:#ffe6ea'>
                            <th>Sản phẩm</th>
                            <th>Số lượng</th>
                            <th>Giá</th>
                            <th>Thành tiền</th>
                        </tr>
                        {itemsHtml}
                    </table>

                    <h3 style='margin-top:20px;'>💰 Tổng thanh toán:
                        <span style='color:red'>{dh.TongTien.Value.ToString("N0")} đ</span>
                    </h3>

                    <p>📍 Địa chỉ giao hàng:</p>
                    <p><strong>{dh.KHACHHANG.DiaChi}</strong></p>

                    <p style='margin-top:20px;'>Cảm ơn bạn đã mua sắm tại SofiaCosmetics!</p>
                </div>";

            var mail = new System.Net.Mail.MailMessage();
            mail.From = new System.Net.Mail.MailAddress("2324801030035@student.tdmu.edu.vn", "SofiaCosmetics");
            mail.To.Add(email);
            mail.Subject = "Xác nhận đơn hàng #" + dh.MaDH;
            mail.Body = body;
            mail.IsBodyHtml = true;

            client.Send(mail);
        }
        // XEM CHI TIẾT ĐƠN HÀNG 
        public ActionResult OrderDetails(int id)
        {
            int? makh = Session["MaKH"] as int?;
            if (makh == null)
                return RedirectToAction("Login", "User");

            var dh = db.DONHANGs.Find(id);
            if (dh == null || dh.MaKH != makh)
                return HttpNotFound();

            return View(dh);
        }

        // HỦY ĐƠN HÀNG 
        public ActionResult CancelOrder(int id)
        {
            int? makh = Session["MaKH"] as int?;
            if (makh == null)
                return RedirectToAction("Login", "User");

            var dh = db.DONHANGs.Find(id);
            if (dh == null || dh.MaKH != makh)
                return HttpNotFound();

            // Không cho hủy nếu đã thanh toán
            if (dh.TrangThai == "Đã thanh toán")
            {
                TempData["Error"] = "Đơn hàng đã thanh toán — không thể hủy!";
                return RedirectToAction("OrderDetails", new { id });
            }

            // Chỉ được hủy trong 12 giờ
            TimeSpan t = DateTime.Now - dh.NgayDat.Value;
            if (t.TotalHours > 12)
            {
                TempData["Error"] = "Đơn hàng đã quá 12 giờ — không thể hủy!";
                return RedirectToAction("OrderDetails", new { id });
            }

            // Hủy đơn
            dh.TrangThai = "Đã hủy";

            // Trả lại kho
            var list = db.CHITIETDONHANGs.Where(x => x.MaDH == id).ToList();
            foreach (var item in list)
            {
                var sp = db.CHITIET_SANPHAM.Find(item.MaCTSP);
                if (sp != null)
                    sp.SoLuongTon += item.SoLuong;
            }

            db.SaveChanges();

            TempData["Success"] = "Đơn hàng đã được hủy thành công!";
            return RedirectToAction("OrderDetails", new { id });
        }
        public ActionResult LichSuMuaHang()
        {
            int? makh = Session["MaKH"] as int?;
            if (makh == null)
                return RedirectToAction("Login", "User");

            var list = db.DONHANGs
                         .Where(d => d.MaKH == makh)
                         .OrderByDescending(d => d.NgayDat)
                         .ToList();

            return View(list);
        }


    }

}

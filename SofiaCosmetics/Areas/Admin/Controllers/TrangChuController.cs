using SofiaCosmetics.Models;
using System;
using System.Collections.Generic;
using System.Data.Objects;  // EntityFunctions
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class TrangChuController : BaseAdminController
    {
        //private QLMyPhamEntities db = new QLMyPhamEntities();

        // ===== DTO THÔNG BÁO (EF-safe) =====
        public class NotiDto
        {
            public string Title { get; set; }
            public string SubTitle { get; set; }
            public string Url { get; set; }
            public DateTime Time { get; set; }
        }

        // ================= DASHBOARD =================
        public ActionResult Index(string range = "today")
        {
            ViewBag.Range = range;

            try
            {
                // ========== NAVBAR DATA ==========
                int? adminId = Session["ADMIN_ID"] as int?;
                if (adminId == null) adminId = 1; // demo fallback, nhớ bỏ khi login set session

                var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == adminId);
                ViewBag.AdminInfo = admin;

                var notiList = BuildNotifications();
                ViewBag.NotiList = notiList;
                ViewBag.NotiCount = notiList.Count;

                // ========== DASHBOARD DATA ==========
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                // xác định thời gian theo range
                DateTime fromDate;
                DateTime toDate = tomorrow;

                switch (range)
                {
                    case "7days":
                        fromDate = today.AddDays(-6);
                        break;
                    case "month":
                        fromDate = new DateTime(today.Year, today.Month, 1);
                        break;
                    case "year":
                        fromDate = new DateTime(today.Year, 1, 1);
                        break;
                    default:
                        fromDate = today;
                        break;
                }

                // 1) DOANH THU THEO RANGE (chỉ đơn hoàn thành)
                decimal doanhThu = db.DONHANGs
                    .Where(d => d.NgayDat >= fromDate && d.NgayDat < toDate
                             && d.TrangThai == "Hoàn thành")
                    .Sum(d => (decimal?)d.TongTien) ?? 0;
                ViewBag.DoanhThuHomNay = doanhThu;

                // 2) KHÁCH MỚI THEO RANGE
                int khachMoiRange = db.KHACHHANGs
                    .Count(k => k.NgayTao >= fromDate && k.NgayTao < toDate);
                ViewBag.KhachMoiHomNay = khachMoiRange;

                // 3) THỐNG KÊ ĐƠN HÀNG TOÀN HỆ THỐNG
                int choXuLy = db.DONHANGs.Count(d => d.TrangThai == "Chờ xác nhận"
                                                 || d.TrangThai == "Chờ xử lý");
                int dangGiao = db.DONHANGs.Count(d => d.TrangThai == "Đang giao");
                int hoanThanh = db.DONHANGs.Count(d => d.TrangThai == "Hoàn thành");

                ViewBag.ChoXuLy = choXuLy;
                ViewBag.DangGiao = dangGiao;
                ViewBag.HoanThanh = hoanThanh;

                // 4) TOP SẢN PHẨM BÁN CHẠY (6 SP) -> DTO SanPhamBanChay
                var topSanPham =
                    (from sp in db.SANPHAMs
                     join ctsp in db.CHITIET_SANPHAM on sp.MaSP equals ctsp.MaSP
                     join ctdh in db.CHITIETDONHANGs on ctsp.MaCTSP equals ctdh.MaCTSP
                     join dh in db.DONHANGs on ctdh.MaDH equals dh.MaDH
                     where dh.TrangThai == "Hoàn thành"
                     group ctdh by new { sp.MaSP, sp.TenSP } into g
                     select new
                     {
                         MaSP = g.Key.MaSP,
                         TenSP = g.Key.TenSP,
                         SoLuongBan = g.Sum(x => (int?)x.SoLuong) ?? 0,
                         HinhAnh = (
                              from ctsp2 in db.CHITIET_SANPHAM
                              join ha in db.HINHANHs on ctsp2.MaCTSP equals ha.MaCTSP
                              where ctsp2.MaSP == g.Key.MaSP
                              select ha.DuongDan
                         ).FirstOrDefault()
                     })
                     .OrderByDescending(x => x.SoLuongBan)
                     .Take(6)
                     .ToList();

                var dtoList = topSanPham.Select(x => new SanPhamBanChay
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    SoLuongBan = x.SoLuongBan,
                    HinhAnh = x.HinhAnh
                }).ToList();

                ViewBag.TopSanPham = dtoList;

                // 5) BIỂU ĐỒ DOANH THU
                if (range == "today")
                {
                    ViewBag.DoanhThuLabels = new List<string> { today.ToString("dd/MM") };
                    ViewBag.DoanhThuValues = new List<decimal> { doanhThu };
                }
                else
                {
                    var doanhThuRange = db.DONHANGs
                        .Where(d => d.NgayDat >= fromDate && d.NgayDat < toDate
                                 && d.TrangThai == "Hoàn thành")
                        .GroupBy(d => EntityFunctions.TruncateTime(d.NgayDat))
                        .Select(g => new
                        {
                            Ngay = g.Key,
                            TongTien = g.Sum(x => (decimal?)x.TongTien) ?? 0
                        })
                        .OrderBy(x => x.Ngay)
                        .ToList();

                    var labels = new List<string>();
                    var values = new List<decimal>();

                    int totalDays = (toDate.Date - fromDate.Date).Days;
                    for (int i = 0; i < totalDays; i++)
                    {
                        var day = fromDate.AddDays(i);
                        labels.Add(day.ToString("dd/MM"));

                        var match = doanhThuRange.FirstOrDefault(x => x.Ngay == day);
                        values.Add(match?.TongTien ?? 0);
                    }

                    ViewBag.DoanhThuLabels = labels;
                    ViewBag.DoanhThuValues = values;
                }

                // 6) SẢN PHẨM SẮP HẾT
                var sanPhamSapHet = db.CHITIET_SANPHAM
                    .Where(ct => ct.SoLuongTon < 5)
                    .OrderBy(ct => ct.SoLuongTon)
                    .Take(5)
                    .Select(ct => new SanPhamSapHetDto
                    {
                        TenSP = ct.SANPHAM.TenSP,
                        Ton = ct.SoLuongTon ?? 0,
                        Gia = ct.GiaKhuyenMai ?? ct.Gia
                    })
                    .ToList();
                ViewBag.SanPhamSapHet = sanPhamSapHet;

                // 7) ĐƠN HÀNG GẦN ĐÂY
                var donGanDay = db.DONHANGs
                    .OrderByDescending(d => d.NgayDat)
                    .Take(5)
                    .Select(d => new DonGanDayDto
                    {
                        MaDH = d.MaDH,
                        Khach = d.KHACHHANG.HoTen,
                        TrangThai = d.TrangThai
                    })
                    .ToList();
                ViewBag.DonGanDay = donGanDay;
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;

                ViewBag.TopSanPham = new List<SanPhamBanChay>();
                ViewBag.NotiList = new List<NotiDto>();
                ViewBag.NotiCount = 0;
                ViewBag.AdminInfo = null;

                ViewBag.DoanhThuLabels = new List<string>();
                ViewBag.DoanhThuValues = new List<decimal>();
                ViewBag.DoanhThuHomNay = 0m;
                ViewBag.KhachMoiHomNay = 0;
                ViewBag.ChoXuLy = ViewBag.DangGiao = ViewBag.HoanThanh = 0;
                ViewBag.SanPhamSapHet = new List<object>();
                ViewBag.DonGanDay = new List<object>();
            }

            return View();
        }

        // ================== NOTI JSON auto refresh ==================
        public JsonResult GetNotifications()
        {
            var notiList = BuildNotifications();

            return Json(new
            {
                count = notiList.Count,
                items = notiList.Take(6).Select(x => new {
                    x.Title,
                    x.SubTitle,
                    x.Url,
                    Time = x.Time.ToString("HH:mm dd/MM")
                })
            }, JsonRequestBehavior.AllowGet);
        }

        // helper build notifications
        private List<NotiDto> BuildNotifications()
        {
            var list = new List<NotiDto>();

            // Đơn chờ xử lý
            var donMoi = db.DONHANGs
                .Where(d => d.TrangThai == "Chờ xác nhận" || d.TrangThai == "Chờ xử lý")
                .OrderByDescending(d => d.NgayDat)
                .Take(5)
                .ToList()
                .Select(d => new NotiDto
                {
                    Title = "Đơn hàng mới chờ xử lý",
                    SubTitle = "Mã đơn #" + d.MaDH + " • " + d.KHACHHANG.HoTen,
                    Url = "/Admin/DonHang/Index?status=pending",
                    Time = d.NgayDat ?? DateTime.Now
                })
                .ToList();

            list.AddRange(donMoi);

            // Khách mới hôm nay
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var khachMoi = db.KHACHHANGs
                .Where(k => k.NgayTao >= today && k.NgayTao < tomorrow)
                .OrderByDescending(k => k.NgayTao)
                .Take(3)
                .ToList()
                .Select(k => new NotiDto
                {
                    Title = "Khách hàng mới",
                    SubTitle = k.HoTen + " vừa đăng ký",
                    Url = "/Admin/KhachHang/Index",
                    Time = k.NgayTao ?? DateTime.Now
                })
                .ToList();

            list.AddRange(khachMoi);

            return list.OrderByDescending(x => x.Time).ToList();
        }

        // ================== ADMIN INFO JSON ==================
        public JsonResult GetAdminInfo()
        {
            int? adminId = Session["ADMIN_ID"] as int?;
            if (adminId == null) adminId = 1; // fallback demo

            var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == adminId);
            if (admin == null)
                return Json(new { ok = false }, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                ok = true,
                data = new
                {
                    admin.MaAdmin,
                    admin.TenDangNhap,
                    admin.HoTen,
                    admin.Email,
                    admin.SDT,
                    admin.VaiTro
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateAdminInfo(string hoTen, string email, string sdt)
        {
            int? adminId = Session["ADMIN_ID"] as int?;
            if (adminId == null) adminId = 1;

            var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == adminId);
            if (admin == null)
                return Json(new { ok = false, msg = "Không tìm thấy admin" });

            admin.HoTen = hoTen;
            admin.Email = email;
            admin.SDT = sdt;

            db.SaveChanges();
            return Json(new { ok = true, msg = "Cập nhật thành công!" });
        }

        // ================== CHANGE PASSWORD JSON ==================
        [HttpPost]
        public JsonResult ChangePassword(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            int? adminId = Session["ADMIN_ID"] as int?;
            if (adminId == null) adminId = 1;

            var admin = db.ADMINs.FirstOrDefault(a => a.MaAdmin == adminId);
            if (admin == null)
                return Json(new { ok = false, msg = "Không tìm thấy admin" });

            if (string.IsNullOrEmpty(matKhauCu) ||
                string.IsNullOrEmpty(matKhauMoi) ||
                string.IsNullOrEmpty(xacNhanMatKhau))
                return Json(new { ok = false, msg = "Vui lòng nhập đầy đủ thông tin" });

            if (admin.MatKhau != matKhauCu)
                return Json(new { ok = false, msg = "Mật khẩu cũ không đúng" });

            if (matKhauMoi.Length < 6)
                return Json(new { ok = false, msg = "Mật khẩu mới phải từ 6 ký tự" });

            if (matKhauMoi != xacNhanMatKhau)
                return Json(new { ok = false, msg = "Xác nhận mật khẩu không khớp" });

            admin.MatKhau = matKhauMoi;
            db.SaveChanges();

            return Json(new { ok = true, msg = "Đổi mật khẩu thành công!" });
        }
    }
}
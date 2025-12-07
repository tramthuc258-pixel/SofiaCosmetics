using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class PhanTichController : BaseAdminController
    {
        // QLMyPhamEntities db = new QLMyPhamEntities();

        // =========================
        // DASHBOARD MAIN
        // =========================
        public ActionResult Index()
        {
            var vm = BuildDashboardVM();
            return View(vm);
        }

        // =========================
        // API: SUMMARY CARDS
        // =========================
        public JsonResult GetSummary()
        {
            var vm = BuildDashboardVM();
            return Json(new
            {
                TongDoanhThu = vm.TongDoanhThu,
                TongDonHang = vm.TongDonHang,
                TongKhachHang = vm.TongKhachHang,
                TongSanPham = vm.TongSanPham
            }, JsonRequestBehavior.AllowGet);
        }

        // =========================
        // API: REVENUE BY MONTH
        // =========================
        public JsonResult GetRevenueByMonth(int months = 6)
        {
            var data = GetRevenueLastMonths(months);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // =========================
        // API: SALES BY CATEGORY
        // =========================
        public JsonResult GetSalesByCategory()
        {
            var data = GetSalesCategoryShare();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // =========================
        // BUILD VM (REAL DB)
        // =========================
        private AdminAnalyticsVM BuildDashboardVM()
        {
            var vm = new AdminAnalyticsVM();

            // 1) Lấy danh sách đơn hoàn thành (linh hoạt chữ hoa thường/dấu/khoảng trắng)
            var donHoanThanhQuery = db.DONHANGs
                .Where(d => d.TrangThai != null
                         && d.TrangThai.Trim().ToLower().Contains("hoàn thành"));

            var donHoanThanhIds = donHoanThanhQuery.Select(d => d.MaDH);

            // 2) Tổng doanh thu = sum(SoLuong * DonGia) của các đơn hoàn thành
            vm.TongDoanhThu = db.CHITIETDONHANGs
                .Where(ct => donHoanThanhIds.Contains(ct.MaDH))
                .Select(ct => (decimal?)((ct.SoLuong ?? 0) * (ct.DonGia ?? 0m)))
                .Sum() ?? 0m;

            // 3) Tổng đơn hàng
            vm.TongDonHang = db.DONHANGs.Count();

            // 4) Tổng khách hàng đang hoạt động
            vm.TongKhachHang = db.KHACHHANGs.Count(k => k.TrangThai == true);

            // 5) Tổng sản phẩm đang bán
            vm.TongSanPham = db.SANPHAMs.Count(s => s.TrangThai == true);

            // charts
            vm.DoanhThuTheoThang = GetRevenueLastMonths(6);
            vm.BanTheoDanhMuc = GetSalesCategoryShare();

            return vm;
        }

        // =========================
        // DOANH THU THEO THÁNG
        // =========================
        private List<RevenuePoint> GetRevenueLastMonths(int months)
        {
            var toDate = DateTime.Now;
            var fromDate = toDate.AddMonths(-months + 1);

            var donHoanThanh = db.DONHANGs
                .Where(d => d.TrangThai != null
                         && d.TrangThai.Trim().ToLower().Contains("hoàn thành")
                         && d.NgayDat.HasValue
                         && d.NgayDat.Value >= fromDate
                         && d.NgayDat.Value <= toDate);

            var raw = donHoanThanh
                .Join(db.CHITIETDONHANGs,
                      d => d.MaDH,
                      ct => ct.MaDH,
                      (d, ct) => new
                      {
                          NgayDat = d.NgayDat.Value,
                          SoLuong = ct.SoLuong ?? 0,
                          DonGia = ct.DonGia ?? 0m
                      })
                .ToList();

            var data = raw
                .GroupBy(x => new { x.NgayDat.Year, x.NgayDat.Month })
                .Select(g => new RevenuePoint
                {
                    Thang = $"{g.Key.Month:00}/{g.Key.Year}",
                    DoanhThu = g.Sum(x => x.SoLuong * x.DonGia)
                })
                .OrderBy(x => DateTime.Parse("01/" + x.Thang))
                .ToList();

            // đảm bảo đủ tháng (tháng nào không có vẫn hiện 0)
            var full = new List<RevenuePoint>();
            for (int i = 0; i < months; i++)
            {
                var t = fromDate.AddMonths(i);
                string key = $"{t.Month:00}/{t.Year}";
                var found = data.FirstOrDefault(x => x.Thang == key);
                full.Add(found ?? new RevenuePoint { Thang = key, DoanhThu = 0m });
            }

            return full;
        }

        // =========================
        // TỶ LỆ DOANH THU THEO DANH MỤC
        // =========================
        private List<CategorySalePoint> GetSalesCategoryShare()
        {
            var donHoanThanhIds = db.DONHANGs
                .Where(d => d.TrangThai != null
                         && d.TrangThai.Trim().ToLower().Contains("hoàn thành"))
                .Select(d => d.MaDH);

            var raw = db.CHITIETDONHANGs
                .Where(ct => donHoanThanhIds.Contains(ct.MaDH))
                .Join(db.CHITIET_SANPHAM,
                    ct => ct.MaCTSP,
                    ctsp => ctsp.MaCTSP,
                    (ct, ctsp) => new { ct, ctsp.MaSP })
                .Join(db.SANPHAMs,
                    x => x.MaSP,
                    sp => sp.MaSP,
                    (x, sp) => new { x.ct, sp.MaDM })
                .Join(db.DANHMUCs,
                    x => x.MaDM,
                    dm => dm.MaDM,
                    (x, dm) => new
                    {
                        dm.TenDM,
                        DoanhThu = (decimal?)((x.ct.SoLuong ?? 0) * (x.ct.DonGia ?? 0m))
                    })
                .ToList();

            decimal total = raw.Sum(x => x.DoanhThu) ?? 0m;

            var result = raw
                .GroupBy(x => x.TenDM)
                .Select(g =>
                {
                    decimal doanhThuDM = g.Sum(x => x.DoanhThu) ?? 0m;
                    decimal tyLe = total == 0m ? 0m : Math.Round(doanhThuDM * 100m / total, 1);

                    return new CategorySalePoint
                    {
                        TenDM = g.Key,
                        DoanhThu = doanhThuDM,
                        TyLe = tyLe
                    };
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            return result;
        }
    }
}

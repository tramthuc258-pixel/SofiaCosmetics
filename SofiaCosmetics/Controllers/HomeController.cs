using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.ViewModels;

namespace SofiaCosmetics.Controllers
{
    public class HomeController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();
        public ActionResult Index()
        {
            ViewBag.ShowSlider = true;
            // 1️⃣ Lấy 8 sản phẩm trending
            var sanphams = db.SANPHAMs
                .Include("CHITIET_SANPHAM.HINHANHs")
                .AsEnumerable()
                .Select(sp => new SanPhamViewModel
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.CHITIET_SANPHAM.FirstOrDefault()?.Gia,
                    MaCTSP = sp.CHITIET_SANPHAM.Select(c => c.MaCTSP).FirstOrDefault(),
                    HinhAnh = sp.CHITIET_SANPHAM
                                .SelectMany(ct => ct.HINHANHs)
                                .Select(h => h.DuongDan)
                                .FirstOrDefault(),
                    PhanTramGiam = (sp.KHUYENMAI != null &&
                                    sp.KHUYENMAI.NgayBatDau <= DateTime.Now &&
                                    sp.KHUYENMAI.NgayKetThuc >= DateTime.Now)
                                    ? sp.KHUYENMAI.PhanTramGiam
                                    : 0
                })
                .Take(8)
                .ToList();

            // Tính giá khuyến mãi 
            foreach (var sp in sanphams)
            {
                if (sp.PhanTramGiam > 0)
                    sp.GiaKhuyenMai = sp.Gia * (decimal)(1 - sp.PhanTramGiam / 100);
                else
                    sp.GiaKhuyenMai = sp.Gia;
            }

            // Lấy 4 sản phẩm bán chạy nhất
            var bestSellerData = (from sp in db.SANPHAMs
                                  join ctsp in db.CHITIET_SANPHAM on sp.MaSP equals ctsp.MaSP
                                  join ctdh in db.CHITIETDONHANGs on ctsp.MaCTSP equals ctdh.MaCTSP
                                  group new { sp, ctsp, ctdh } by new
                                  {
                                      sp.MaSP,
                                      sp.TenSP,
                                      sp.KHUYENMAI
                                  } into g
                                  orderby g.Sum(x => x.ctdh.SoLuong) descending
                                  select new
                                  {
                                      MaSP = g.Key.MaSP,
                                      TenSP = g.Key.TenSP,
                                      Gia = g.Min(x => x.ctsp.Gia),
                                      HinhAnh = g.SelectMany(x => x.ctsp.HINHANHs)
                                                 .Select(h => h.DuongDan)
                                                 .FirstOrDefault(),
                                      PhanTramGiam = (g.Key.KHUYENMAI != null &&
                                                      g.Key.KHUYENMAI.NgayBatDau <= DateTime.Now &&
                                                      g.Key.KHUYENMAI.NgayKetThuc >= DateTime.Now)
                                                      ? g.Key.KHUYENMAI.PhanTramGiam
                                                      : 0
                                  })
                                  .Take(4)
                                  .ToList();

            //  Tính giá khuyến mãi 
            var bestSeller = bestSellerData.Select(sp => new SanPhamViewModel
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                Gia = sp.Gia,
                HinhAnh = sp.HinhAnh,
                PhanTramGiam = sp.PhanTramGiam,
                GiaKhuyenMai = (sp.PhanTramGiam > 0)
                                ? sp.Gia * (decimal)(1 - sp.PhanTramGiam / 100)
                                : sp.Gia
            }).ToList();

            //  Gộp dữ liệu vào ViewModel
            var model = new TrangChuViewModel
            {
                TrendingProducts = sanphams,
                BestSellerProducts = bestSeller
            };

            return View(model);
        }

        public ActionResult SanPham(int page = 1, int pageSize = 8)
        {
            var sanPhams = db.SANPHAMs
                .Select(sp => new SanPhamViewModel
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.CHITIET_SANPHAM.FirstOrDefault().Gia,
                    GiaKhuyenMai = sp.CHITIET_SANPHAM.FirstOrDefault().GiaKhuyenMai,
                    HinhAnh = sp.CHITIET_SANPHAM.SelectMany(c => c.HINHANHs)
                                .Select(h => h.DuongDan).FirstOrDefault(),
                    MaCTSP = sp.CHITIET_SANPHAM.Select(c => c.MaCTSP).FirstOrDefault(),
                    PhanTramGiam = sp.KHUYENMAI != null ? sp.KHUYENMAI.PhanTramGiam : 0
                })
                .OrderBy(x => x.MaSP)
                .ToPagedList(page, pageSize);

            return View(sanPhams);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult SliderPartial()
        {
            var sliders = db.SLIDERs
                .Where(x => x.TrangThai == true)
                .OrderBy(x => x.ThuTu)
                .ToList();

            return PartialView("SliderPartial", sliders);
        }
        public ActionResult DanhMuc(string link, int page = 1, int pageSize = 8)
        {
            if (string.IsNullOrEmpty(link))
                return RedirectToAction("Index");

            var menu = db.MENUs.FirstOrDefault(x => x.MenuLink == link);
            if (menu == null) return HttpNotFound();

            var dm = db.DANHMUCs.FirstOrDefault(x => x.TenDM == menu.MenuName);
            if (dm == null) return HttpNotFound();

            var listDM = db.DANHMUCs
                            .Where(x => x.MaCha == dm.MaDM)
                            .Select(x => x.MaDM).ToList();

            listDM.Add(dm.MaDM);

            var sanphamVM = db.SANPHAMs
                .Where(x => listDM.Contains(x.MaDM))
                .Select(x => new SanPhamViewModel
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    Gia = x.CHITIET_SANPHAM.FirstOrDefault().Gia,
                    GiaKhuyenMai = x.CHITIET_SANPHAM.FirstOrDefault().GiaKhuyenMai,
                    HinhAnh = x.CHITIET_SANPHAM.SelectMany(ct => ct.HINHANHs)
                                .Select(h => h.DuongDan).FirstOrDefault(),
                    PhanTramGiam = x.KHUYENMAI != null ? x.KHUYENMAI.PhanTramGiam : 0,
                    MaCTSP = x.CHITIET_SANPHAM.Select(c => c.MaCTSP).FirstOrDefault()
                })
                .OrderBy(x => x.MaSP)
                .ToPagedList(page, pageSize);

            ViewBag.TenDanhMuc = dm.TenDM;
            ViewBag.Link = link;

            return View(sanphamVM);
        }
        public ActionResult TimKiem(string keyword, int page = 1, int pageSize = 8)
        {
            ViewBag.TuKhoa = keyword;

            if (string.IsNullOrWhiteSpace(keyword))
                return View(new PagedList<SanPhamViewModel>(new List<SanPhamViewModel>(), page, pageSize));

            var data = db.SANPHAMs
                .Where(sp => sp.TenSP.Contains(keyword))
                .Select(sp => new SanPhamViewModel
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    MaCTSP = sp.CHITIET_SANPHAM.Select(c => c.MaCTSP).FirstOrDefault(),
                    Gia = sp.CHITIET_SANPHAM.Select(c => c.Gia).FirstOrDefault(),
                    GiaKhuyenMai = sp.CHITIET_SANPHAM.Select(c => c.GiaKhuyenMai).FirstOrDefault(),
                    PhanTramGiam = sp.KHUYENMAI != null ? sp.KHUYENMAI.PhanTramGiam : 0,
                    HinhAnh = sp.CHITIET_SANPHAM.SelectMany(c => c.HINHANHs)
                                .Select(h => h.DuongDan).FirstOrDefault()
                })
                .OrderBy(x => x.MaSP)
                .ToPagedList(page, pageSize);

            return View(data);
        }
        public ActionResult ChiTietSP(int id)
        {
            db.Configuration.LazyLoadingEnabled = false;

            var sp = db.SANPHAMs
                .Include("THUONGHIEU")
                .Include("DANHMUC")
                .Include("CHITIET_SANPHAM.HINHANHs")
                .FirstOrDefault(x => x.MaSP == id);

            if (sp == null)
                return HttpNotFound();

            // Lấy tất cả ảnh từ các biến thể
            var allImages = sp.CHITIET_SANPHAM
                .SelectMany(ct => ct.HINHANHs)
                .Select(h => h.DuongDan)
                .Distinct()
                .ToList();

            // Ảnh dùng để xem (không phải biến thể)
            var mainImage = allImages.FirstOrDefault()
                            ?? "/images/products/no-image.png";

            // KHÔNG chọn biến thể mặc định
            var vm = new SanPhamViewModel
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                MaCTSP = 0,          
                HinhAnh = mainImage, 
                Gia = null,
                GiaKhuyenMai = null
            };

            // List biến thể
            ViewBag.BienThe = sp.CHITIET_SANPHAM.Select(bt => new SanPhamViewModel
            {
                MaCTSP = bt.MaCTSP,
                TenBienThe = bt.TenBienThe,
                Gia = bt.Gia,
                GiaKhuyenMai = bt.GiaKhuyenMai,
                SoLuongTon = bt.SoLuongTon ?? 0,
                HinhAnh = bt.HINHANHs.Select(h => h.DuongDan).FirstOrDefault()
                          ?? "/images/products/no-image.png"
            }).ToList();

            ViewBag.HinhAnhs = allImages;
            ViewBag.MoTa = sp.MoTa;
            ViewBag.TenDanhMuc = sp.DANHMUC?.TenDM ?? "";
            ViewBag.TenThuongHieu = sp.THUONGHIEU?.TenThuongHieu ?? "";

            return View(vm);
        }

        // Load menu cha (ParentId = NULL)
        [ChildActionOnly]
        public ActionResult Header()
        {
            // Chỉ lấy menu cha (ParentId == null) để hiển thị trên thanh menu
            var menus = db.MENUs
                          .Where(m => m.ParentId == null)
                          .OrderBy(m => m.OrderNumber)
                          .ToList();

            // Lấy Id của các menu cha
            var parentIds = menus.Select(m => m.Id).ToList();

            // Đếm số menu con của từng menu cha để biết menu nào có dropdown
            var childCounts = db.MENUs
                .Where(m => m.ParentId != null && parentIds.Contains((int)m.ParentId))
                .GroupBy(m => m.ParentId)
                .ToDictionary(g => (int)g.Key, g => g.Count());

            ViewBag.ChildCounts = childCounts;

            // Trả về cho Header.cshtml **chỉ danh sách menu cha**
            return PartialView(menus);
        }


        [ChildActionOnly]
        public ActionResult LoadChildMenu(int parentId)
        {
            List<MENU> lst = new List<MENU>();
            lst = db.MENUs.Where(m => m.ParentId == parentId).OrderBy(m => m.OrderNumber).ToList();
            ViewBag.Count = lst.Count();
            int[] a = new int[lst.Count()];
            for (int i = 0; i < lst.Count; i++)
            {
                int id = lst[i].Id;
                List<MENU> l = db.MENUs.Where(m => m.ParentId == id).ToList();
                int k = l.Count();
                a[i] = k;
            }
            ViewBag.Lst = a;
            return PartialView("LoadChildMenu", lst);
        }
        public ActionResult QuickView(int id)
        {
            var sp = db.SANPHAMs
                .Include("CHITIET_SANPHAM.HINHANHs")
                .Include("THUONGHIEU")
                .Include("DANHMUC")
                .FirstOrDefault(x => x.MaSP == id);

            if (sp == null)
                return HttpNotFound();

            //  Không gán biến thể đầu tiên
            var first = sp.CHITIET_SANPHAM.FirstOrDefault();

            var vm = new SanPhamViewModel
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,

                // KHÔNG được gán MaCTSP
                MaCTSP = null,

                Gia = first?.Gia,
                GiaKhuyenMai = first?.GiaKhuyenMai,
                SoLuongTon = 0,

                HinhAnh = first?.HINHANHs
                    .Select(h => h.DuongDan)
                    .FirstOrDefault()
                    ?? "/images/products/no-image.png",
            };

            // Biến thể
            ViewBag.BienThe = sp.CHITIET_SANPHAM.Select(bt => new SanPhamViewModel
            {
                MaCTSP = bt.MaCTSP,
                TenBienThe = bt.TenBienThe,
                Gia = bt.Gia,
                GiaKhuyenMai = bt.GiaKhuyenMai,
                SoLuongTon = bt.SoLuongTon ?? 0,
                HinhAnh = bt.HINHANHs
                    .Select(h => h.DuongDan)
                    .FirstOrDefault()
                    ?? "/images/products/no-image.png"
            }).ToList();

            // Mô tả ngắn
            ViewBag.MoTaNgan = string.IsNullOrEmpty(sp.MoTa)
                ? ""
                : (sp.MoTa.Length > 200 ? sp.MoTa.Substring(0, 200) + "..." : sp.MoTa);

            return PartialView("QuickView", vm);
        }

        //DANHGIA san pham
        [HttpPost]
        public ActionResult GuiDanhGia(int maSP, int soSao, string noiDung)
        {
            // Lấy MaKH từ session
            int? maKH = Session["MaKH"] as int?;

            if (maKH == null)
            {
                return Json(new { success = false, message = "Bạn phải đăng nhập để đánh giá." });
            }

            var kh = db.KHACHHANGs.Find(maKH.Value);
            if (kh == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });
            }

            var dg = new DANHGIA
            {
                MaSP = maSP,
                MaKH = kh.MaKH,
                SoSao = soSao,
                NoiDung = noiDung,
                NgayDanhGia = DateTime.Now
            };

            db.DANHGIAs.Add(dg);
            db.SaveChanges();

            return Json(new { success = true, message = "Đánh giá thành công!" });
        }


        public ActionResult LayDanhGia(int maSP)
        {
            var list = db.DANHGIAs
                .Where(x => x.MaSP == maSP)
                .OrderByDescending(x => x.NgayDanhGia)
                .Select(x => new
                {
                    x.NoiDung,
                    x.SoSao,
                    x.NgayDanhGia,
                    TenKH = x.KHACHHANG.HoTen
                })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


    }
}
using System;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.ViewModels;

namespace SofiaCosmetics.Controllers
{
    public class KhuyenMaiController : Controller
    {
        QLMyPhamEntities db = new QLMyPhamEntities();

        public ActionResult Index(int? page)
        {
            int pageSize = 12;
            int pageNumber = page ?? 1;

            var list = (from ct in db.CHITIET_SANPHAM
                        join sp in db.SANPHAMs on ct.MaSP equals sp.MaSP
                        where ct.GiaKhuyenMai != null && ct.GiaKhuyenMai < ct.Gia
                        select new SanPhamViewModel
                        {
                            MaSP = sp.MaSP,
                            MaCTSP = ct.MaCTSP,
                            TenSP = sp.TenSP,
                            TenBienThe = ct.TenBienThe,

                            Gia = ct.Gia,
                            GiaKhuyenMai = ct.GiaKhuyenMai,

                            PhanTramGiam = (int)Math.Round(
                                ((ct.Gia - ct.GiaKhuyenMai.Value) / ct.Gia) * 100
                            ),

                            HinhAnh = db.HINHANHs
                                        .Where(h => h.MaCTSP == ct.MaCTSP)
                                        .Select(h => h.DuongDan)
                                        .FirstOrDefault()
                        })
                        .OrderByDescending(x => x.PhanTramGiam)
                        .ToList();

            ViewBag.ShowSlider = false;
            return View(list.ToPagedList(pageNumber, pageSize));
        }
    }
}
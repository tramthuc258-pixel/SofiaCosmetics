using OfficeOpenXml;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class KhachHangController : BaseAdminController
    {
        //QLMyPhamEntities db = new QLMyPhamEntities();

        // ===========================
        // DANH SÁCH + TÌM KIẾM
        // ===========================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 2)
        {
            var list = db.KHACHHANGs
                .OrderByDescending(x => x.MaKH)
                .ToList()
                .Select(x => new AdminKhachHang
                {
                    MaKH = x.MaKH,
                    HoTen = x.HoTen,
                    Email = x.Email,
                    SDT = x.SDT,
                    DiaChi = x.DiaChi,
                    NgayTao = x.NgayTao,
                    TrangThai = x.TrangThai == true
                }).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                string kwNoMark = RemoveUnicode(kw);

                list = list.Where(x =>
                    /* --- TÌM THEO MÃ KHÁCH HÀNG --- */
                    ("kh" + x.MaKH.ToString("000")).ToLower().Contains(kw) ||
                    x.MaKH.ToString().Contains(kw) ||

                    /* --- TÌM THEO TÊN, KHÔNG DẤU --- */
                    RemoveUnicode(x.HoTen.ToLower()).Contains(kwNoMark) ||

                    /* --- TÌM THEO EMAIL --- */
                    (x.Email != null && x.Email.ToLower().Contains(kw)) ||

                    /* --- TÌM THEO SỐ ĐIỆN THOẠI --- */
                    (x.SDT != null && x.SDT.Contains(kw)) ||

                    /* --- TÌM THEO TRẠNG THÁI --- */
                    (kwNoMark.Contains("hoat") && x.TrangThai == true) ||
                    (kwNoMark.Contains("ngung") && x.TrangThai == false)
                ).ToList();
            }

            ViewBag.Search = search;

            // phân trang
            int totalPage = (int)Math.Ceiling((double)list.Count / pageSize);
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;
            list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();


            return View(list);
        }
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            string[] arr1 = { "á","à","ả","ạ","ã","â","ấ","ầ","ẩ","ẫ","ậ","ă","ắ","ằ","ẳ","ẵ","ặ",
                      "đ",
                      "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                      "í","ì","ỉ","ĩ","ị",
                      "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                      "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                      "ý","ỳ","ỷ","ỹ","ỵ" };

            string[] arr2 = { "a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a",
                      "d",
                      "e","e","e","e","e","e","e","e","e","e","e",
                      "i","i","i","i","i",
                      "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                      "u","u","u","u","u","u","u","u","u","u","u",
                      "y","y","y","y","y" };

            text = text.ToLower();
            for (int i = 0; i < arr1.Length; i++)
                text = text.Replace(arr1[i], arr2[i]);

            return text;
        }

        // ===========================
        // THÊM KHÁCH HÀNG
        // ===========================
        [HttpPost]
        public JsonResult Add(AddKhachHangModel model)
        {
            try
            {
                KHACHHANG kh = new KHACHHANG
                {
                    HoTen = model.HoTen,
                    Email = model.Email,
                    SDT = model.SDT,
                    DiaChi = model.DiaChi,
                    NgayTao = DateTime.Now,
                    TrangThai = true
                };

                db.KHACHHANGs.Add(kh);
                db.SaveChanges();

                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }


        // ===========================
        // LẤY THÔNG TIN ĐỂ SỬA
        // ===========================
        public JsonResult Get(int id)
        {
            var kh = db.KHACHHANGs.Find(id);
            if (kh == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                kh.MaKH,
                kh.HoTen,
                kh.Email,
                kh.SDT,
                kh.DiaChi,
                kh.TrangThai
            }, JsonRequestBehavior.AllowGet);
        }


        // ===========================
        // SỬA KHÁCH HÀNG
        // ===========================
        [HttpPost]
        public JsonResult Edit(EditKhachHangModel model)
        {
            try
            {
                var kh = db.KHACHHANGs.Find(model.MaKH);
                if (kh == null) return Json(false);

                kh.HoTen = model.HoTen;
                kh.Email = model.Email;
                kh.SDT = model.SDT;
                kh.DiaChi = model.DiaChi;
                kh.TrangThai = model.TrangThai;

                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }


        // ===========================
        // XÓA KHÁCH HÀNG
        // ===========================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            var kh = db.KHACHHANGs.Find(id);
            if (kh == null) return Json(false);

            db.KHACHHANGs.Remove(kh);
            db.SaveChanges();

            return Json(true);
        }


        // ===========================
        // XEM CHI TIẾT
        // ===========================
        public JsonResult Details(int id)
        {
            var kh = db.KHACHHANGs.Find(id);
            if (kh == null) return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                kh.HoTen,
                kh.Email,
                kh.SDT,
                kh.DiaChi,
                NgayTao = kh.NgayTao?.ToString("dd/MM/yyyy"),
                TrangThai = kh.TrangThai.GetValueOrDefault() ? "Hoạt động" : "Ngừng hoạt động"
            }, JsonRequestBehavior.AllowGet);

        }

        public FileResult ExportExcel()
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("KhachHang");

                // ======== HEADER =========
                ws.Cells["A1"].Value = "Danh sách khách hàng";
                ws.Cells["A1:G1"].Merge = true;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 18;
                ws.Cells["A1"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // ======== TIÊU ĐỀ CỘT =========
                string[] headers = { "Mã KH", "Họ tên", "Email", "SĐT", "Địa chỉ", "Ngày tạo", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                {
                    ws.Cells[3, i + 1].Value = headers[i];
                }

                // Style Header
                using (var range = ws.Cells["A3:G3"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(52, 152, 219)); // Xanh nhẹ
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // ======== DATA =========
                var list = db.KHACHHANGs.OrderByDescending(x => x.MaKH).ToList();
                int row = 4;

                foreach (var kh in list)
                {
                    ws.Cells[row, 1].Value = "KH" + kh.MaKH.ToString("000");
                    ws.Cells[row, 2].Value = kh.HoTen;
                    ws.Cells[row, 3].Value = kh.Email;
                    ws.Cells[row, 4].Value = kh.SDT;
                    ws.Cells[row, 5].Value = kh.DiaChi;

                    ws.Cells[row, 6].Value = kh.NgayTao?.ToString("dd/MM/yyyy");

                    ws.Cells[row, 7].Value = (kh.TrangThai == true)
                                            ? "Hoạt động"
                                            : "Ngừng hoạt động";

                    row++;
                }

                // ======== BORDER =========
                using (var range = ws.Cells[3, 1, row - 1, 7])
                {
                    range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // ======== AUTO FIT CỘT =========
                ws.Cells.AutoFitColumns();

                // File name
                string fileName = "KhachHang_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                return File(pkg.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }
    }
}
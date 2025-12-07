using OfficeOpenXml;
using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class DonHangController : BaseAdminController
    {
        //QLMyPhamEntities db = new QLMyPhamEntities();

        // ============================
        // HÀM BỎ DẤU (dùng cho search)
        // ============================
        private string RemoveUnicode(string text)
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

        // ============================
        // DANH SÁCH + TÌM KIẾM
        // ============================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 4)
        {
            search = (search ?? "").Trim();
            string kw = search.ToLower();
            string kwNoMark = RemoveUnicode(kw);

            // ===== 1) Query gốc trên DB =====
            var q = from dh in db.DONHANGs
                    join kh in db.KHACHHANGs on dh.MaKH equals kh.MaKH
                    select new
                    {
                        dh.MaDH,
                        KhachHang = kh.HoTen,
                        dh.NgayDat,
                        TongTien = dh.TongTien ?? 0,
                        dh.TrangThai
                    };

            // ===== 2) Search trên DB (được) =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                // tìm theo mã DH nếu nhập kiểu DH001 hoặc số
                int num;
                bool isCodeSearch = false;
                string onlyNum = kw.Replace("dh", ""); // "dh001" -> "001"
                if (int.TryParse(onlyNum, out num))
                    isCodeSearch = true;

                q = q.Where(x =>
                    (isCodeSearch && x.MaDH == num)
                    || x.KhachHang.Contains(search)
                    || x.TrangThai.Contains(search)
                );
            }

            // ===== 3) Đếm tổng & clamp page =====
            int totalItems = q.Count();
            int totalPage = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (totalPage == 0) totalPage = 1;

            page = Math.Max(1, Math.Min(page, totalPage));

            // ===== 4) Lấy đúng trang =====
            var list = q
                .OrderByDescending(x => x.MaDH)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList() // mới ToList ở đây
                .Select(x => new AdminOrderListVM
                {
                    MaDH = x.MaDH,
                    KhachHang = x.KhachHang,
                    NgayDat = x.NgayDat,
                    TongTien = x.TongTien,
                    TrangThai = x.TrangThai
                })
                .ToList();

            // ===== 5) Search KHÔNG DẤU + mã format (lọc memory bổ sung) =====
            if (!string.IsNullOrWhiteSpace(search))
            {
                list = list.Where(x =>
                    ("dh" + x.MaDH.ToString("000")).ToLower().Contains(kw)
                    || x.MaDH.ToString().Contains(kw)
                    || (!string.IsNullOrEmpty(x.KhachHang) &&
                        (x.KhachHang.ToLower().Contains(kw) ||
                         RemoveUnicode(x.KhachHang.ToLower()).Contains(kwNoMark)))
                    || (!string.IsNullOrEmpty(x.TrangThai) &&
                        (x.TrangThai.ToLower().Contains(kw) ||
                         RemoveUnicode(x.TrangThai.ToLower()).Contains(kwNoMark)))
                ).ToList();

                // sau lọc memory => tính lại page (để khớp)
                totalItems = list.Count();
                totalPage = (int)Math.Ceiling(totalItems / (double)pageSize);
                if (totalPage == 0) totalPage = 1;

                page = Math.Max(1, Math.Min(page, totalPage));

                list = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            ViewBag.Search = search;
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.PageSize = pageSize;

            return View(list);
        }

        // ============================
        // CHI TIẾT ĐƠN (JSON cho modal XEM + SỬA)
        // ============================
        public JsonResult Details(int id)
        {
            var dh = db.DONHANGs.Find(id);
            if (dh == null) return Json(new { success = false }, JsonRequestBehavior.AllowGet);

            var kh = db.KHACHHANGs.Find(dh.MaKH);

            var items = (from ctdh in db.CHITIETDONHANGs
                         join ctsp in db.CHITIET_SANPHAM on ctdh.MaCTSP equals ctsp.MaCTSP
                         join sp in db.SANPHAMs on ctsp.MaSP equals sp.MaSP
                         where ctdh.MaDH == id
                         select new
                         {
                             sp.TenSP,
                             ctsp.TenBienThe,
                             SoLuong = ctdh.SoLuong ?? 0,
                             DonGia = (decimal)(ctdh.DonGia ?? ctsp.Gia),
                             HinhAnh = db.HINHANHs
                                         .Where(h => h.MaCTSP == ctsp.MaCTSP)
                                         .Select(h => h.DuongDan)
                                         .FirstOrDefault()
                         }).ToList()
                         .Select(x => new
                         {
                             x.TenSP,
                             x.TenBienThe,
                             x.SoLuong,
                             x.DonGia,
                             ThanhTien = x.SoLuong * x.DonGia,
                             x.HinhAnh
                         }).ToList();

            var data = new
            {
                MaDH = dh.MaDH,
                MaDHText = "DH" + dh.MaDH.ToString("000"),
                KhachHang = kh?.HoTen ?? "",
                Email = kh?.Email ?? "",
                SDT = kh?.SDT ?? "",
                DiaChi = kh?.DiaChi ?? "",
                NgayDat = dh.NgayDat?.ToString("dd/MM/yyyy HH:mm"),
                TongTien = dh.TongTien ?? 0,
                TrangThai = dh.TrangThai,
                Items = items
            };

            return Json(new { success = true, data }, JsonRequestBehavior.AllowGet);
        }

        // ============================
        // SỬA TRẠNG THÁI (AJAX)
        // ============================
        [HttpPost]
        public JsonResult Edit(AdminOrderEditVM model)
        {
            try
            {
                var dh = db.DONHANGs.Find(model.MaDH);
                if (dh == null) return Json(false);

                dh.TrangThai = model.TrangThai;
                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ============================
        // XÓA ĐƠN HÀNG (AJAX)
        // ============================
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var dh = db.DONHANGs.Find(id);
                if (dh == null) return Json(false);

                var ctdhList = db.CHITIETDONHANGs.Where(x => x.MaDH == id).ToList();
                foreach (var item in ctdhList)
                {
                    db.CHITIETDONHANGs.Remove(item);
                }

                db.DONHANGs.Remove(dh);
                db.SaveChanges();
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        // ============================
        // XUẤT EXCEL
        // ============================
        public FileResult ExportExcel()
        {
            using (var pkg = new ExcelPackage())
            {
                var ws = pkg.Workbook.Worksheets.Add("DonHang");

                // ====== 1) TIÊU ĐỀ ======
                ws.Cells["A1"].Value = "Danh sách đơn hàng";
                ws.Cells["A1:E1"].Merge = true;
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 18;
                ws.Cells["A1"].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                ws.Cells["A1"].Style.VerticalAlignment =
                    OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                ws.Row(1).Height = 28;

                // Viền + khung tiêu đề
                using (var rangeTitle = ws.Cells["A1:E1"])
                {
                    rangeTitle.Style.Border.Top.Style =
                    rangeTitle.Style.Border.Bottom.Style =
                    rangeTitle.Style.Border.Left.Style =
                    rangeTitle.Style.Border.Right.Style =
                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                // ====== 2) HEADER GIỐNG KHÁCH HÀNG (NỀN XANH - CHỮ TRẮNG) ======
                string[] headers = { "Mã ĐH", "Khách hàng", "Ngày đặt", "Tổng tiền", "Trạng thái" };
                for (int i = 0; i < headers.Length; i++)
                    ws.Cells[3, i + 1].Value = headers[i];

                using (var headerRange = ws.Cells["A3:E3"])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Font.Size = 12;
                    headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);

                    // nền xanh kiểu bảng KH (bạn có thể đổi mã màu nếu muốn)
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(
                        System.Drawing.Color.FromArgb(0, 112, 192) // xanh dương đậm
                    );

                    headerRange.Style.HorizontalAlignment =
                        OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    headerRange.Style.VerticalAlignment =
                        OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    // viền header
                    headerRange.Style.Border.Top.Style =
                    headerRange.Style.Border.Bottom.Style =
                    headerRange.Style.Border.Left.Style =
                    headerRange.Style.Border.Right.Style =
                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                }

                ws.Row(3).Height = 22;

                // ====== 3) DATA ======
                var list = (from dh in db.DONHANGs
                            join kh in db.KHACHHANGs on dh.MaKH equals kh.MaKH
                            orderby dh.MaDH descending
                            select new
                            {
                                dh.MaDH,
                                kh.HoTen,
                                dh.NgayDat,
                                dh.TongTien,
                                dh.TrangThai
                            }).ToList();

                int row = 4;
                foreach (var item in list)
                {
                    ws.Cells[row, 1].Value = "DH" + item.MaDH.ToString("000");
                    ws.Cells[row, 2].Value = item.HoTen;
                    ws.Cells[row, 3].Value = item.NgayDat?.ToString("dd/MM/yyyy");
                    ws.Cells[row, 4].Value = item.TongTien ?? 0;
                    ws.Cells[row, 5].Value = item.TrangThai;
                    row++;
                }

                int lastRow = row - 1;

                // ====== 4) FORMAT BẢNG + VIỀN KHUNG ======
                using (var dataRange = ws.Cells[$"A3:E{lastRow}"])
                {
                    // viền toàn bảng
                    dataRange.Style.Border.Top.Style =
                    dataRange.Style.Border.Bottom.Style =
                    dataRange.Style.Border.Left.Style =
                    dataRange.Style.Border.Right.Style =
                        OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    dataRange.Style.VerticalAlignment =
                        OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                }

                // Căn lề từng cột giống bảng
                ws.Column(1).Width = 12;  // Mã ĐH
                ws.Column(2).Width = 25;  // Khách hàng
                ws.Column(3).Width = 15;  // Ngày đặt
                ws.Column(4).Width = 16;  // Tổng tiền
                ws.Column(5).Width = 18;  // Trạng thái

                ws.Cells[$"A4:A{lastRow}"].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                ws.Cells[$"C4:C{lastRow}"].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                ws.Cells[$"D4:D{lastRow}"].Style.Numberformat.Format = "#,##0";
                ws.Cells[$"D4:D{lastRow}"].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;

                ws.Cells[$"E4:E{lastRow}"].Style.HorizontalAlignment =
                    OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                // Tô nền zebra nhẹ cho dễ nhìn (giống excel hay dùng)
                for (int r = 4; r <= lastRow; r++)
                {
                    if (r % 2 == 0)
                    {
                        using (var rr = ws.Cells[$"A{r}:E{r}"])
                        {
                            rr.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rr.Style.Fill.BackgroundColor.SetColor(
                                System.Drawing.Color.FromArgb(242, 242, 242) // xám nhạt
                            );
                        }
                    }
                }

                string fileName = "DonHang_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
                return File(pkg.GetAsByteArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName);
            }
        }
    }
}
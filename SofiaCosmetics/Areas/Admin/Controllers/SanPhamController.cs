using SofiaCosmetics.Models;
using SofiaCosmetics.Models.AdminModels;
using SofiaCosmetics.Areas.Admin.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SofiaCosmetics.Areas.Admin.Controllers
{
    public class SanPhamController : BaseAdminController
    {
        // ======================== INDEX: LIST + SEARCH + PAGING ========================
        public ActionResult Index(string search = "", int page = 1, int pageSize = 25)
        {
            search = (search ?? "").Trim();
            string kw = search.ToLower();
            string kwNoMark = RemoveUnicode(kw);

            var q = db.CHITIET_SANPHAM.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                int num;
                bool isCodeSearch = false;
                string onlyNum = kw.Replace("sp", "");
                if (int.TryParse(onlyNum, out num)) isCodeSearch = true;

                q = q.Where(ct =>
                    (isCodeSearch && (ct.MaSP == num || ct.MaCTSP == num))
                    || ct.SANPHAM.TenSP.Contains(search)
                    || ct.SANPHAM.THUONGHIEU.TenThuongHieu.Contains(search)
                    || (ct.TenBienThe ?? "").Contains(search)
                    || (ct.SANPHAM.TrangThai == true && "con ban active conban".Contains(kwNoMark))
                    || (ct.SANPHAM.TrangThai == false && "ngung ban inactive ngungban het".Contains(kwNoMark))
                );
            }

            int totalItems = q.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));

            var raw = q
                .OrderByDescending(ct => ct.MaSP)
                .ThenBy(ct => ct.MaCTSP)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ct => new
                {
                    ct.MaCTSP,
                    ct.MaSP,
                    ct.TenBienThe,
                    ct.Gia,
                    ct.GiaKhuyenMai,
                    ct.SoLuongTon,

                    TenSP = ct.SANPHAM.TenSP,
                    ThuongHieu = ct.SANPHAM.THUONGHIEU.TenThuongHieu,
                    TrangThai = ct.SANPHAM.TrangThai,

                    PhanTram = ct.MaKM != null
                                ? (double?)ct.KHUYENMAI.PhanTramGiam
                                : null,

                    ct.MaKM
                })
                .ToList();

            // lọc in-memory bổ sung (theo mã dạng SP001, bỏ dấu, ...)
            if (!string.IsNullOrWhiteSpace(search))
            {
                raw = raw.Where(x =>
                    ("sp" + x.MaSP.ToString("000")).ToLower().Contains(kw) ||
                    x.MaSP.ToString().Contains(kw) ||
                    RemoveUnicode((x.TenSP ?? "").ToLower()).Contains(kwNoMark) ||
                    RemoveUnicode((x.ThuongHieu ?? "").ToLower()).Contains(kwNoMark) ||
                    RemoveUnicode((x.TenBienThe ?? "").ToLower()).Contains(kwNoMark)
                ).ToList();

                totalItems = raw.Count();
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
                page = Math.Max(1, Math.Min(page, totalPages == 0 ? 1 : totalPages));
                raw = raw.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            var data = raw.Select(x =>
            {
                decimal giaGoc = x.Gia;
                decimal giaSauGiam = x.GiaKhuyenMai ?? giaGoc;

                string hinhAnh = db.HINHANHs
                    .Where(h => h.MaCTSP == x.MaCTSP)
                    .Select(h => h.DuongDan)
                    .FirstOrDefault();

                return new SanPhamAdmin
                {
                    MaSP = x.MaSP,
                    MaCTSP = x.MaCTSP,
                    TenSP = x.TenSP,
                    ThuongHieu = x.ThuongHieu,

                    TenBienThe = x.TenBienThe ?? "Default",
                    GiaGoc = giaGoc,

                    GiaSauGiam = giaSauGiam,
                    PhanTramGiam = x.PhanTram,
                    MaKM = x.MaKM,

                    TonKho = x.SoLuongTon,
                    HinhAnh = hinhAnh,
                    TrangThai = x.TrangThai
                };
            }).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;
            ViewBag.PageSize = pageSize;

            return View(data);
        }

        // ======================== SAVE IMAGE ========================
        private string SaveProductImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".webp")
                throw new Exception("Chỉ hỗ trợ JPG/PNG/WEBP!");

            if (!file.ContentType.StartsWith("image/"))
                throw new Exception("File không phải hình ảnh!");

            if (file.ContentLength > 2 * 1024 * 1024)
                throw new Exception("Ảnh tối đa 2MB!");

            var fileName = "product_" + DateTime.Now.Ticks + ext;
            var folder = Server.MapPath("~/Upload/images/product/");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var savePath = Path.Combine(folder, fileName);
            file.SaveAs(savePath);

            return "/Upload/images/product/" + fileName;
        }

        // ======================== REMOVE UNICODE ========================
        public static string RemoveUnicode(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            string[] arr1 = {
                "á","à","ả","ạ","ã","â","ấ","ầ","ẩ","ẫ","ậ","ă","ắ","ằ","ẳ","ẵ","ặ",
                "đ",
                "é","è","ẻ","ẽ","ẹ","ê","ế","ề","ể","ễ","ệ",
                "í","ì","ỉ","ĩ","ị",
                "ó","ò","ỏ","õ","ọ","ô","ố","ồ","ổ","ỗ","ộ","ơ","ớ","ờ","ở","ỡ","ợ",
                "ú","ù","ủ","ũ","ụ","ư","ứ","ừ","ử","ữ","ự",
                "ý","ỳ","ỷ","ỹ","ỵ"
            };
            string[] arr2 = {
                "a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a","a",
                "d",
                "e","e","e","e","e","e","e","e","e","e","e",
                "i","i","i","i","i",
                "o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o","o",
                "u","u","u","u","u","u","u","u","u","u","u",
                "y","y","y","y","y"
            };
            text = text.ToLower();
            for (int i = 0; i < arr1.Length; i++)
                text = text.Replace(arr1[i], arr2[i]);
            return text;
        }

        // ======================== DROPDOWNS ========================
        public JsonResult GetDanhMuc()
        {
            var list = db.DANHMUCs.Select(x => new { x.MaDM, x.TenDM }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetThuongHieu()
        {
            var list = db.THUONGHIEUx.Select(x => new { x.MaTH, x.TenThuongHieu }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetKhuyenMai()
        {
            var list = db.KHUYENMAIs.Select(x => new
            {
                x.MaKM,
                x.TenKhuyenMai,
                x.PhanTramGiam
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLoaiBienThe(int maDM)
        {
            var variants = new Dictionary<int, List<string>>
            {
                { 1, new List<string>{ "Da dầu", "Da khô", "Da hỗn hợp", "Da nhạy cảm", "Da thường", "Mọi loại da" } },
                { 2, new List<string>{ "Tone sáng", "Tone tự nhiên", "Tone lạnh", "Tone ấm" } },
                { 3, new List<string>{ "Tóc dầu", "Tóc khô", "Tóc hư tổn", "Tóc thường" } },
                { 4, new List<string>{ "Tẩy tế bào chết", "Sữa tắm dưỡng ẩm", "Sữa tắm làm sạch", "Dưỡng thể" } },
                { 5, new List<string>{ "Nam", "Nữ", "Unisex" } },
                { 6, new List<string>{ "Da dầu", "Da khô", "Da nhạy cảm" } },
                { 8, new List<string>{ "Gel", "Cream", "Lotion" } }
            };

            var result = variants.ContainsKey(maDM) ? variants[maDM] : new List<string>();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // ======================== GET PRODUCT (view chi tiết SP) ========================
        public JsonResult GetProduct(int id)
        {
            var sp = db.SANPHAMs
                .Include("DANHMUC")
                .Include("THUONGHIEU")
                .FirstOrDefault(x => x.MaSP == id);

            if (sp == null) return Json(null, JsonRequestBehavior.AllowGet);

            var ctList = db.CHITIET_SANPHAM
                .Include("KHUYENMAI")
                .Where(x => x.MaSP == id).ToList();

            var imgs = ctList.SelectMany(ct =>
                db.HINHANHs
                    .Where(h => h.MaCTSP == ct.MaCTSP)
                    .Select(h => new VariantImageVM
                    {
                        Id = h.MaHinh,
                        Url = h.DuongDan,
                        MaCTSP = ct.MaCTSP,
                        TenBienThe = ct.TenBienThe ?? "Default",
                        Gia = ct.Gia,
                        TonKho = ct.SoLuongTon ?? 0
                    })
            ).ToList();

            var ctDefault = ctList.FirstOrDefault();

            return Json(new
            {
                sp.MaSP,
                sp.TenSP,
                sp.MoTa,
                sp.MaDM,
                sp.MaTH,
                sp.TrangThai,

                DanhMuc = sp.DANHMUC?.TenDM ?? "",
                ThuongHieu = sp.THUONGHIEU?.TenThuongHieu ?? "",

                TenBienThe = ctDefault?.TenBienThe ?? "Default",
                Gia = ctDefault?.Gia ?? 0,
                TonKho = ctDefault?.SoLuongTon ?? 0,

                Images = imgs
            }, JsonRequestBehavior.AllowGet);
        }

        // ======================== GET VARIANT (edit đúng biến thể) ========================
        public JsonResult GetVariant(int id)
        {
            var ct = db.CHITIET_SANPHAM
                .Include("SANPHAM.DANHMUC")
                .Include("SANPHAM.THUONGHIEU")
                .Include("KHUYENMAI")
                .FirstOrDefault(x => x.MaCTSP == id);

            if (ct == null) return Json(null, JsonRequestBehavior.AllowGet);

            var sp = ct.SANPHAM;

            var imgs = db.HINHANHs
                .Where(h => h.MaCTSP == ct.MaCTSP)
                .Select(h => new VariantImageVM
                {
                    Id = h.MaHinh,
                    Url = h.DuongDan,
                    MaCTSP = ct.MaCTSP,
                    TenBienThe = ct.TenBienThe ?? "Default",
                    Gia = ct.Gia,
                    TonKho = ct.SoLuongTon ?? 0
                })
                .ToList();

            return Json(new
            {
                sp.MaSP,
                ct.MaCTSP,

                sp.TenSP,
                sp.MoTa,
                sp.MaDM,
                sp.MaTH,
                sp.TrangThai,

                TenBienThe = ct.TenBienThe ?? "Default",
                Gia = ct.Gia,
                TonKho = ct.SoLuongTon ?? 0,

                ct.MaKM,
                PhanTramGiam = ct.MaKM != null ? ct.KHUYENMAI.PhanTramGiam : 0,

                Images = imgs
            }, JsonRequestBehavior.AllowGet);
        }

        // ======================== ADD PRODUCT ========================
        [HttpPost]
        public JsonResult AddProduct(AddProductModel model, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            try
            {
                var files = ImageFiles?.Where(f => f != null && f.ContentLength > 0).ToList();
                if (files == null || files.Count == 0)
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất 1 ảnh!" });

                var sp = new SANPHAM
                {
                    TenSP = model.TenSP,
                    MoTa = model.MoTa,
                    MaTH = model.MaTH,
                    MaDM = model.MaDM,
                    TrangThai = model.TrangThai,
                    NgayTao = DateTime.Now
                };
                db.SANPHAMs.Add(sp);
                db.SaveChanges();

                var ct = new CHITIET_SANPHAM
                {
                    MaSP = sp.MaSP,
                    Gia = model.Gia,
                    SoLuongTon = model.TonKho,
                    TenBienThe = string.IsNullOrWhiteSpace(model.TenBienThe) ? "Default" : model.TenBienThe.Trim(),
                    MaKM = null,
                    GiaKhuyenMai = null
                };

                if (model.MaKM.HasValue && model.MaKM.Value > 0)
                {
                    ct.MaKM = model.MaKM;
                    var km = db.KHUYENMAIs.Find(model.MaKM.Value);
                    double pt = km?.PhanTramGiam ?? 0;
                    if (pt > 0) ct.GiaKhuyenMai = ct.Gia * (decimal)(100 - pt) / 100;
                }

                db.CHITIET_SANPHAM.Add(ct);
                db.SaveChanges();

                foreach (var f in files)
                {
                    string url = SaveProductImage(f);
                    db.HINHANHs.Add(new HINHANH
                    {
                        MaCTSP = ct.MaCTSP,
                        DuongDan = url
                    });
                }
                db.SaveChanges();

                // LOG
                AuditLogger.Log(
                    module: "SanPham",
                    action: "CREATE",
                    target: $"SP#{sp.MaSP}",
                    note: $"Tạo sản phẩm {sp.TenSP}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi thêm SP: " + ex.Message });
            }
        }

        // ======================== UPDATE VARIANT ========================
        [HttpPost]
        public JsonResult UpdateProduct(EditProductModel model, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            try
            {
                var sp = db.SANPHAMs.Find(model.MaSP);
                if (sp == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });

                var ct = db.CHITIET_SANPHAM.FirstOrDefault(x => x.MaCTSP == model.MaCTSP);
                if (ct == null)
                    return Json(new { success = false, message = "Không tìm thấy biến thể!" });

                // backup thông tin cũ nếu muốn log chi tiết hơn
                var oldGia = ct.Gia;
                var oldTon = ct.SoLuongTon;
                var oldKM = ct.MaKM;

                // update SP
                sp.TenSP = model.TenSP;
                sp.MoTa = model.MoTa;
                sp.MaDM = model.MaDM;
                sp.MaTH = model.MaTH;
                sp.TrangThai = model.TrangThai;

                // update CTSP
                ct.TenBienThe = string.IsNullOrWhiteSpace(model.TenBienThe) ? "Default" : model.TenBienThe.Trim();
                ct.Gia = model.Gia;
                ct.SoLuongTon = model.TonKho;

                if (model.MaKM.HasValue && model.MaKM.Value > 0)
                {
                    ct.MaKM = model.MaKM;
                    var km = db.KHUYENMAIs.Find(model.MaKM.Value);
                    double pt = km?.PhanTramGiam ?? 0;

                    if (pt > 0)
                        ct.GiaKhuyenMai = ct.Gia * (decimal)(100 - pt) / 100;
                    else
                        ct.GiaKhuyenMai = null;
                }
                else
                {
                    ct.MaKM = null;
                    ct.GiaKhuyenMai = null;
                }

                var files = ImageFiles?.Where(f => f != null && f.ContentLength > 0).ToList();
                if (files != null && files.Count > 0)
                {
                    foreach (var f in files)
                    {
                        string url = SaveProductImage(f);
                        db.HINHANHs.Add(new HINHANH
                        {
                            MaCTSP = ct.MaCTSP,
                            DuongDan = url
                        });
                    }
                }

                db.SaveChanges();

                // LOG
                AuditLogger.Log(
                    module: "SanPham",
                    action: "EDIT",
                    target: $"CTSP#{ct.MaCTSP}",
                    note: $"Sửa {sp.TenSP} | Biến thể {ct.TenBienThe} | Giá={ct.Gia} | Tồn={ct.SoLuongTon}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi cập nhật: " + ex.Message });
            }
        }

        // ======================== DELETE SINGLE IMAGE ========================
        [HttpPost]
        public JsonResult DeleteImage(int id)
        {
            try
            {
                var img = db.HINHANHs.Find(id);
                if (img == null) return Json(new { success = false });

                int maCTSP = img.MaCTSP;
                var ct = db.CHITIET_SANPHAM.Find(maCTSP);
                int maSP = ct != null ? ct.MaSP : 0;

                try
                {
                    if (!string.IsNullOrEmpty(img.DuongDan) &&
                        img.DuongDan.StartsWith("/Upload/images/product/"))
                    {
                        var path = Server.MapPath(img.DuongDan);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                }
                catch { }

                db.HINHANHs.Remove(img);
                db.SaveChanges();

                // LOG
                AuditLogger.Log(
                    module: "SanPham",
                    action: "DELETE_IMAGE",
                    target: $"IMG#{id}",
                    note: $"Xóa ảnh của CTSP#{maCTSP} SP#{maSP}"
                );

                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        // ======================== DELETE VARIANT ========================
        [HttpPost]
        public JsonResult DeleteVariant(int id)
        {
            try
            {
                var ct = db.CHITIET_SANPHAM.Find(id);
                if (ct == null) return Json(new { success = false });

                var ten = (ct.TenBienThe ?? "Default").Trim().ToLower();
                bool isDefault = ten == "default";

                int maSP = ct.MaSP;

                var imgs = db.HINHANHs.Where(h => h.MaCTSP == ct.MaCTSP).ToList();
                foreach (var i in imgs)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(i.DuongDan) &&
                            i.DuongDan.StartsWith("/Upload/images/product/"))
                        {
                            var oldPath = Server.MapPath(i.DuongDan);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                    }
                    catch { }

                    db.HINHANHs.Remove(i);
                }

                db.CHITIET_SANPHAM.Remove(ct);
                db.SaveChanges();

                if (isDefault)
                {
                    var leftVariants = db.CHITIET_SANPHAM.Where(x => x.MaSP == maSP).ToList();
                    foreach (var v in leftVariants)
                    {
                        var vImgs = db.HINHANHs.Where(h => h.MaCTSP == v.MaCTSP).ToList();
                        foreach (var img in vImgs)
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(img.DuongDan) &&
                                    img.DuongDan.StartsWith("/Upload/images/product/"))
                                {
                                    var oldPath = Server.MapPath(img.DuongDan);
                                    if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                                }
                            }
                            catch { }

                            db.HINHANHs.Remove(img);
                        }
                        db.CHITIET_SANPHAM.Remove(v);
                    }

                    var sp = db.SANPHAMs.Find(maSP);
                    if (sp != null) db.SANPHAMs.Remove(sp);

                    db.SaveChanges();

                    // LOG xóa luôn SP
                    AuditLogger.Log(
                        module: "SanPham",
                        action: "DELETE_PRODUCT",
                        target: $"SP#{maSP}",
                        note: "Xóa Default variant => xóa luôn sản phẩm"
                    );
                }
                else
                {
                    // LOG xóa variant thường
                    AuditLogger.Log(
                        module: "SanPham",
                        action: "DELETE_VARIANT",
                        target: $"CTSP#{ct.MaCTSP}",
                        note: $"Xóa biến thể {ct.TenBienThe} của SP#{maSP}"
                    );
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ======================== ADD VARIANT ========================
        [HttpPost]
        public JsonResult AddVariant(AddVariantModel model, IEnumerable<HttpPostedFileBase> ImageFiles)
        {
            try
            {
                var sp = db.SANPHAMs.Find(model.MaSP);
                if (sp == null)
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm gốc!" });

                string tenBienThe = (model.TenBienThe ?? "").Trim();
                if (string.IsNullOrWhiteSpace(tenBienThe))
                    tenBienThe = "Default";

                bool existed = db.CHITIET_SANPHAM.Any(x =>
                    x.MaSP == model.MaSP &&
                    (x.TenBienThe ?? "Default") == tenBienThe);

                if (existed)
                    return Json(new { success = false, message = "Biến thể này đã tồn tại!" });

                var files = ImageFiles?.Where(f => f != null && f.ContentLength > 0).ToList();
                if (files == null || files.Count == 0)
                    return Json(new { success = false, message = "Vui lòng chọn ít nhất 1 ảnh biến thể!" });

                double pt = 0;
                decimal? giaSau = null;

                if (model.MaKM.HasValue && model.MaKM.Value > 0)
                {
                    var km = db.KHUYENMAIs.Find(model.MaKM.Value);
                    pt = km?.PhanTramGiam ?? 0;
                    if (pt > 0) giaSau = model.Gia * (decimal)(100 - pt) / 100;
                }

                var ct = new CHITIET_SANPHAM
                {
                    MaSP = model.MaSP,
                    TenBienThe = tenBienThe,
                    Gia = model.Gia,
                    SoLuongTon = model.TonKho,

                    MaKM = (model.MaKM.HasValue && model.MaKM.Value > 0) ? model.MaKM : null,
                    GiaKhuyenMai = giaSau
                };

                db.CHITIET_SANPHAM.Add(ct);
                db.SaveChanges();

                foreach (var f in files)
                {
                    string url = SaveProductImage(f);
                    db.HINHANHs.Add(new HINHANH
                    {
                        MaCTSP = ct.MaCTSP,
                        DuongDan = url
                    });
                }

                db.SaveChanges();

                // LOG
                AuditLogger.Log(
                    module: "SanPham",
                    action: "ADD_VARIANT",
                    target: $"SP#{model.MaSP} / CTSP#{ct.MaCTSP}",
                    note: $"BienThe={ct.TenBienThe} | Gia={ct.Gia} | Ton={ct.SoLuongTon} | KM={ct.MaKM}"
                );

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi thêm biến thể: " + ex.Message });
            }
        }
    }
}

using SofiaCosmetics.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace SofiaCosmetics.Areas.Admin.Helpers
{
    public static class AuditLogger
    {
        // Đặt cố định tên file log
        private const string LogVirtualPath = "~/App_Data/audit.log";

        /// <summary>
        /// Ghi log thao tác admin
        /// </summary>
        /// <param name="module">Tên module: NhanSu, SanPham, DonHang...</param>
        /// <param name="action">Hành động: CREATE / EDIT / DELETE / UPDATE_STATUS...</param>
        /// <param name="target">Đối tượng tác động: Admin#5, Product#10-Variant#22...</param>
        /// <param name="note">Ghi chú chi tiết (optional)</param>
        public static void Log(string module, string action, string target, string note = "")
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return;

                // Lấy admin đang đăng nhập
                int? maAdminLogin = ctx.Session["ADMIN_LOGIN"] as int?;
                string actorName = "Unknown";

                using (var db = new QLMyPhamEntities())
                {
                    if (maAdminLogin.HasValue)
                    {
                        var actor = db.ADMINs.FirstOrDefault(x => x.MaAdmin == maAdminLogin.Value);
                        if (actor != null)
                        {
                            actorName = !string.IsNullOrWhiteSpace(actor.HoTen)
                                ? actor.HoTen
                                : actor.TenDangNhap;
                        }
                    }
                }

                // Dòng log
                string line =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {actorName} ({maAdminLogin}) | {module} | {action} | {target} | {note}";

                var path = ctx.Server.MapPath(LogVirtualPath);
                var dir = Path.GetDirectoryName(path);

                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                File.AppendAllText(path, line + Environment.NewLine, Encoding.UTF8);
            }
            catch
            {
                // Nuốt lỗi, không cho log làm crash hệ thống
            }
        }

        /// <summary>
        /// Đọc log (dùng cho controller nếu muốn)
        /// </summary>
        public static string[] ReadLines(int take = 200)
        {
            try
            {
                var ctx = HttpContext.Current;
                if (ctx == null) return new string[0];

                var path = ctx.Server.MapPath(LogVirtualPath);
                if (!File.Exists(path)) return new string[0];

                var lines = File.ReadAllLines(path, Encoding.UTF8);
                return lines.Reverse().Take(take).ToArray();
            }
            catch
            {
                return new string[0];
            }
        }
    }
}

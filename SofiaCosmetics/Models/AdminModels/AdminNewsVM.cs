using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    // Dùng cho danh sách tin tức (Index)
    // ViewModel cho list (Index)
    public class AdminNewsListVM
    {
        public int MaTT { get; set; }
        public string TenTrang { get; set; }
        public string MetaTitle { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    // ViewModel cho Details (modal xem)
    public class AdminNewsDetailVM
    {
        public int MaTT { get; set; }
        public string TenTrang { get; set; }
        public string MetaTitle { get; set; }
        public string NoiDung { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    // ViewModel nhận từ modal thêm/sửa
    public class AdminNewsFormVM
    {
        public int MaTT { get; set; }

        [Required(ErrorMessage = "Tên trang không được trống")]
        public string TenTrang { get; set; }

        [Required(ErrorMessage = "Meta title không được trống")]
        public string MetaTitle { get; set; }

        // HTML từ CKEditor
        public string NoiDung { get; set; }
    }
}
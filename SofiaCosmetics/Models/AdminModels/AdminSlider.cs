using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    public class AdminSlider
    {
        public int MaSlider { get; set; }
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public string Link { get; set; }
        public int ThuTu { get; set; }
        public bool TrangThai { get; set; }
        public DateTime? NgayTao { get; set; }
    }

    public class AddSliderModel
    {
        public string TieuDe { get; set; }
        public string MoTa { get; set; }
        public string HinhAnh { get; set; }
        public string Link { get; set; }
        public int ThuTu { get; set; }
        public bool TrangThai { get; set; }
    }

    public class EditSliderModel : AddSliderModel
    {
        public int MaSlider { get; set; }
    }
}
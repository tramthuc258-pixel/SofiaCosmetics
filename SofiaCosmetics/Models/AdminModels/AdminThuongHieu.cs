using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    public class AdminThuongHieu
    {
        public int MaTH { get; set; }
        public string TenThuongHieu { get; set; }
        public string MoTa { get; set; }
    }

    public class AddThuongHieuModel
    {
        public string TenThuongHieu { get; set; }
        public string MoTa { get; set; }
    }

    public class EditThuongHieuModel
    {
        public int MaTH { get; set; }
        public string TenThuongHieu { get; set; }
        public string MoTa { get; set; }
    }
}
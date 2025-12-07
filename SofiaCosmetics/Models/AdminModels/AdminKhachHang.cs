using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
	public class AdminKhachHang
	{
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public DateTime? NgayTao { get; set; }
        public bool TrangThai { get; set; }
    }

    public class AddKhachHangModel
    {
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
    }

    public class EditKhachHangModel
    {
        public int MaKH { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public string SDT { get; set; }
        public string DiaChi { get; set; }
        public bool TrangThai { get; set; }
    }
}
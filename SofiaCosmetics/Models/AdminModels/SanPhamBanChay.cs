using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models
{
	public class SanPhamBanChay
	{
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public int SoLuongBan { get; set; }
        public string HinhAnh { get; set; }
    }

    public class SanPhamSapHetDto
    {
        public string TenSP { get; set; }
        public int Ton { get; set; }
        public decimal Gia { get; set; }
    }

    public class DonGanDayDto
    {
        public int MaDH { get; set; }
        public string Khach { get; set; }
        public string TrangThai { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SofiaCosmetics.Models;

namespace SofiaCosmetics.Models.ViewModels
{
    public class CartItem
    {
        public int MaCTSP { get; set; }
        public string TenSP { get; set; }
        public string TenBienThe { get; set; }
        public string HinhAnh { get; set; }
        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        public decimal ThanhTien => Gia * SoLuong;
    }
}
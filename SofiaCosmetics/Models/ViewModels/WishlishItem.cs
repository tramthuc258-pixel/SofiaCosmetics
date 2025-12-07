using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.ViewModels
{
    public class WishlistItem
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal Gia { get; set; }
        public string HinhAnh { get; set; }
    }
}
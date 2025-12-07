using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.ViewModels
{
    public class TrangChuViewModel
    {
        public List<SanPhamViewModel> TrendingProducts { get; set; }
        public List<SanPhamViewModel> BestSellerProducts { get; set; }
    }
}
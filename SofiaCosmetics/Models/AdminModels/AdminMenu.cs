using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SofiaCosmetics.Models.AdminModels
{
    public class AdminMenu
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public string MenuLink { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
    }

    public class AddMenuModel
    {
        public string MenuName { get; set; }
        public string MenuLink { get; set; }
        public int? ParentId { get; set; }
    }

    public class EditMenuModel
    {
        public int Id { get; set; }
        public string MenuName { get; set; }
        public string MenuLink { get; set; }
        public int? ParentId { get; set; }
    }
}
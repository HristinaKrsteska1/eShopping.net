using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eShopping.NET.Areas.Admin.Models.ViewModels.Shop
{
    public class OrdersForAdminVM
    {
        public int OrderNumber { get; set; }
        public string Username { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductsAndQuantity { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
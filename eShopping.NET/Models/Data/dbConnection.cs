using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace eShopping.NET.Models.Data
{
    public class dbConnection : DbContext
    {
        public DbSet<pageDTO> Pages { get; set; }
        public DbSet<SidebarDTO> Sidebar{ get; set; }
        public DbSet<CategoryDTO> Categories { get; set; }
        public DbSet<ProductDTO> Products { get; set; }



    }
} 
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace inventory___sales_management_system.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public DateTime DateEdited { get; set; } = DateTime.Now;

        public string Name { get; set; }

        public decimal Price { get; set; } = 0m;

        public decimal Cost { get; set; } = 0m;

        public int QuantityAvailable { get; set; } = 0;

        public int LowStockThreshold { get; set; } = 0;

        public bool IsActive { get; set; } = false;

        public int? CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }

}
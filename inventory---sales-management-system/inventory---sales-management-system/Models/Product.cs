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

        [Display(Name = "Last Edited")]
        public DateTime DateEdited { get; set; } = DateTime.Now;

        [Display(Name = "Product Name")]
        public string Name { get; set; }

        public decimal Price { get; set; } = 0m;

        public decimal Cost { get; set; } = 0m;

        [Display(Name = "Quantity Available")]
        public int QuantityAvailable { get; set; } = 0;

        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 0;

        [Display(Name = "Activity Status")]
        public bool IsActive { get; set; } = false;

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }

}
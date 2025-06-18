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

        [Display(Name = "Product")]
        public string Name { get; set; }

        public decimal Price { get; set; } = 0m;

        public decimal Cost { get; set; } = 0m;

        [Display(Name = "Qty Available")]
        public int QuantityAvailable { get; set; } = 0;

        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 0;

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = false;

        public bool IsOnSale { get; set; } = false;

        [Display(Name = "Discount (%)")]
        [Range(0, 100)]
        public decimal? DiscountPercent { get; set; } 

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }

}
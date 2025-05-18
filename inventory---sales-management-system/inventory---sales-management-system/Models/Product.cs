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

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public decimal Cost { get; set; }

        [Required]
        public int QuantityAvailable { get; set; }

        public int LowStockThreshold { get; set; }

        public bool IsActive { get; set; } = true;

        public int CategoryId { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.Models
{
    public class StockEntry
    {
        public int StockEntryId { get; set; }

        [Display(Name = "Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Display(Name = "Supplier")]
        public string Supplier { get; set; }

        [Display(Name = "Cost per Qty")]
        public decimal CostPerQty { get; set; }

        [Display(Name = "Qty Added")]
        public int QuantityAdded { get; set; }

        [Display(Name = "Total Cost")]
        public decimal TotalCost => CostPerQty * QuantityAdded;

        [Display(Name = "Added By")]
        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
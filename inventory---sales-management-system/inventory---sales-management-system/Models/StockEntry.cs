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

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Supplier Name")]
        public string Supplier { get; set; }

        [Required]
        [Display(Name = "Cost per Quantity")]
        public decimal CostPerQty { get; set; }

        [Required]
        [Display(Name = "Quantity Added")]
        public int QuantityAdded { get; set; }

        [Display(Name = "Total Cost")]
        public decimal TotalCost => CostPerQty * QuantityAdded;

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
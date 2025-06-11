using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Stock
{
    public class StockHistoryViewModel
    {
        [Display(Name = "Date Added")]
        public DateTime DateAdded { get; set; }

        [Display(Name = "Supplier Name")]
        public string Supplier { get; set; }

        [Display(Name = "Cost per Quantity")]
        public decimal CostPerQty { get; set; }

        [Display(Name = "Quantity Added")]
        public int QuantityAdded { get; set; }

        [Display(Name = "Total Cost")]
        public decimal TotalCost => CostPerQty * QuantityAdded;

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Added By")]
        public string AddedByUsername { get; set; }
    }
}
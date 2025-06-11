using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Stock
{
    public class AddStockViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [Display(Name = "Supplier Name")]
        public string Supplier { get; set; }

        [Required]
        [Display(Name = "Cost per Quantity")]
        public decimal CostPerQty { get; set; }

        [Required]
        [Display(Name = "Quantity Added")]
        public int QuantityAdded { get; set; }
    }

}
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
        public string Supplier { get; set; }

        [Required]
        public decimal CostPerQty { get; set; }

        [Required]
        public int QuantityAdded { get; set; }
    }

}
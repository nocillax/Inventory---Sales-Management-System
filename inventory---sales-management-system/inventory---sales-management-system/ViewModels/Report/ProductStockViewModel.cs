using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Report
{
    public class ProductStockViewModel
    {
        [Display(Name = "Product")]
        public string ProductName { get; set; }

        [Display(Name = "Qty Available")]
        public int QuantityAvailable { get; set; }

        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; }
    }

}
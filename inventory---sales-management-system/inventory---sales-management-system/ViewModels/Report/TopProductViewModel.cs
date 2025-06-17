using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Report
{
    public class TopProductViewModel
    {
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Display(Name = "Qty Sold")]
        public int QuantitySold { get; set; }

        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }
    }

}
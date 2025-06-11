using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Sale
{
    public class SaleHistoryViewModel
    {
        public int SaleId { get; set; }

        [Display(Name = "Sale Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Sales Person")]
        public string SalesPersonName { get; set; }

        [Display(Name = "Buyer Name")]
        public string BuyerName { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }
}
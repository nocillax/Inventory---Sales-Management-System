using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Sale
{
    public class SaleDetailsViewModel
    {
        public int SaleId { get; set; }

        [Display(Name = "Salesperson")]
        public string SalesPersonName { get; set; }

        [Display(Name = "Sale Date")]
        public DateTime Date { get; set; }

        [Display(Name = "Buyer Name")]
        public string BuyerName { get; set; }

        public List<SaleItemViewModel> SaleItems { get; set; }

        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }
    }

    public class SaleItemViewModel
    {
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        [Display(Name = "Price at Sale")]
        public decimal PriceAtSale { get; set; }

        public decimal Subtotal => Quantity * PriceAtSale;
    }
}
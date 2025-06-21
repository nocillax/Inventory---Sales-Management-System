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

        [Required(ErrorMessage = "Supplier Name is Required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        public string Supplier { get; set; }

        [Required(ErrorMessage = "Cost per Unit is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal CostPerQty { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int QuantityAdded { get; set; }
    }

}
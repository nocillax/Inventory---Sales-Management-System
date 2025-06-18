using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace inventory___sales_management_system.ViewModels.Product
{
    public class EditProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be non-negative")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Cost must be non-negative")]
        public decimal Cost { get; set; }

        [Required(ErrorMessage = "Quantity Available is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
        public int QuantityAvailable { get; set; }

        [Required(ErrorMessage = "Low Stock Threshold is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Low Stock Threshold must be non-negative")]
        public int LowStockThreshold { get; set; }

        [Display(Name = "Activity Status")]
        public bool IsActive { get; set; } = false;

        [Display(Name = "On Sale")]
        public bool IsOnSale { get; set; } = false;

        [Display(Name = "Discount (%)")]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100")]
        public decimal? DiscountPercent { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace inventory___sales_management_system.ViewModels.Sale
{
    public class CreateSaleViewModel
    {
        [Required(ErrorMessage ="Date & Time is required")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Buyer Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string BuyerName { get; set; }

        // List of active products for dropdown/search
        public List<inventory___sales_management_system.Models.Product> ProductsList { get; set; }

        // Arrays to hold selected product IDs and quantities on POST
        public int[] ProductIds { get; set; }
        public int[] Quantities { get; set; }
    }
}
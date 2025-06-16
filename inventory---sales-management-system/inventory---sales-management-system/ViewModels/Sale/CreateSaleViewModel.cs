using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace inventory___sales_management_system.ViewModels.Sale
{
    public class CreateSaleViewModel
    {
        public DateTime Date { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Buyer Name is required")]
        public string BuyerName { get; set; }

        // List of active products for dropdown/search
        public List<inventory___sales_management_system.Models.Product> ProductsList { get; set; }

        // Arrays to hold selected product IDs and quantities on POST
        public int[] ProductIds { get; set; }
        public int[] Quantities { get; set; }
    }
}
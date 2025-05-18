using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.Models
{
    public class SaleItem
    {
        public int SaleItemId { get; set; }

        public int Quantity { get; set; }

        [Required]
        public decimal PriceAtSale { get; set; }

        public int SaleId { get; set; }

        public virtual Sale Sale { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }

}
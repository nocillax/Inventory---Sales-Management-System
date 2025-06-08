using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.Models
{
    public class StockEntry
    {
        public int StockEntryId { get; set; }

        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public DateTime DateAdded { get; set; } = DateTime.Now;

        public string Supplier { get; set; }

        public decimal CostPerQty { get; set; } 

        public int QuantityAdded { get; set; }

        public decimal TotalCost => CostPerQty * QuantityAdded;

        public int UserId { get; set; }

        public virtual User User { get; set; }
    }
}
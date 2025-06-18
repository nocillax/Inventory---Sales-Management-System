using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Report
{
    public class DeadStockViewModel
    {
        public string ProductName { get; set; }
        public DateTime? LastSoldDate { get; set; }
        public int QuantityAvailable { get; set; }
    }
}
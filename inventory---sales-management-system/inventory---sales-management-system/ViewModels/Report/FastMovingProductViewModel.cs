using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Report
{
    public class FastMovingProductViewModel
    {
        public string ProductName { get; set; }
        public int QuantitySold { get; set; }
        public int DaysActive { get; set; }
        public double UnitsPerDay { get; set; }
    }
}
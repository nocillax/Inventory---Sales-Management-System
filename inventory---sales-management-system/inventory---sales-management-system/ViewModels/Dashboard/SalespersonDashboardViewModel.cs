using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Dashboard
{
    public class SalespersonDashboardViewModel
    {
        public int TodaysSaleCount { get; set; }
        public decimal TodaysSaleTotal { get; set; }

        public decimal ThisMonthSaleTotal { get; set; }
        public decimal LastMonthSaleTotal { get; set; }
        public decimal SaleGrowthPercent { get; set; }

        public List<string> Last7Days { get; set; }      
        public List<decimal> Last7DaysSales { get; set; }  
    }
}
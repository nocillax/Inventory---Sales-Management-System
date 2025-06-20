using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Dashboard
{
    public class ManagerDashboardViewModel
    {
        // KPI Cards
        public decimal TodaysSales { get; set; }
        public int LowStockCount { get; set; }
        public int DeadStockCount { get; set; }
        public decimal TotalSalesThisMonth { get; set; }

        // Profit
        public decimal ThisMonthProfit { get; set; }
        public decimal LastMonthProfit { get; set; }
        public decimal MonthlyProfitChangePercent { get; set; }

        // Forecast
        public decimal ForecastedSales { get; set; }

        // Charts
        public List<string> Last6Months { get; set; }
        public List<decimal> MonthlySales { get; set; }
        public List<decimal> MonthlyProfits { get; set; }

        public List<string> SaleHours { get; set; }
        public List<int> HourlySalesCount { get; set; }

        // Mini Cards
        public int AvailableProducts { get; set; }
        public int ProductsSoldThisMonth { get; set; }

        public string TopProductName { get; set; }
        public int TopProductQty { get; set; }

        public string TopSalespersonName { get; set; }
        public decimal TopSalespersonTotal { get; set; }

        public string MostProfitableProductName { get; set; }
        public decimal MostProfitableProductAmount { get; set; }

        public string MostLossProductName { get; set; }
        public decimal MostLossProductAmount { get; set; }

        public int TimeSinceLastSale { get; set; }

        public string FastestMovingProductName { get; set; }
        public double FastestMovingRate { get; set; }

    }


}
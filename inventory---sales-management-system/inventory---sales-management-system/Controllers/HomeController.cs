using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.ViewModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager", "Salesperson")]
    public class HomeController : Controller
    {
        private ISMSDBContext db;

        public HomeController()
        {
            db = new ISMSDBContext();
        }


        public ActionResult Index()
        {
            var role = Session["UserRole"] as string;
            var username = Session["Username"] as string;
            var message = "";

            if (role == "Manager")
            {
                var viewModel = GetManagerDashboardViewModel();
                return View("ManagerDashboard", viewModel);
            }
            else if (role == "Salesperson")
            {
                var viewModel = GetSalespersonDashboardViewModel(username);
                return View("SalespersonDashboard", viewModel);
            }
            else
            {
                message = "Role not recognized";
            }

            ViewBag.RoleMessage = message;
            return View();
        }

        
        // Manager Dashboard Stuffs ------------------------------------------------------

        private class KpiStats
        {
            public decimal TodaysSales { get; set; }
            public int LowStockCount { get; set; }
            public int DeadStockCount { get; set; }
            public decimal TotalSalesThisMonth { get; set; }
            public int ProductsSoldThisMonth { get; set; }
        }

        private class ProfitStats
        {
            public decimal ThisMonthProfit { get; set; }
            public decimal LastMonthProfit { get; set; }
            public decimal ChangePercent { get; set; }
        }

        private class MiniCardStats
        {
            public string TopProduct { get; set; }
            public int TopProductQty { get; set; }
            public string TopSalesperson { get; set; }
            public decimal TopSalespersonTotal { get; set; }
            public string MostProfitableProduct { get; set; }
            public decimal MostProfitAmount { get; set; }
            public string MostLossProduct { get; set; }
            public decimal MostLossAmount { get; set; }
            public int TimeSinceLastSale { get; set; }
            public string FastestMovingProduct { get; set; }
            public double UnitsPerDay { get; set; }
        }



        private KpiStats GetKpiStats()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var cutoffDate = today.AddDays(-60);

            var todaysSales = db.Sales
                .Where(s => DbFunctions.TruncateTime(s.Date) == today)
                .Select(s => s.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var lowStock = db.Products
                .Count(p => p.IsActive && p.QuantityAvailable < p.LowStockThreshold);

            var deadStock = db.Products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.ProductId,
                    LastSold = db.SaleItems
                        .Where(si => si.ProductId == p.ProductId)
                        .OrderByDescending(si => si.Sale.Date)
                        .Select(si => (DateTime?)si.Sale.Date)
                        .FirstOrDefault()
                })
                .AsEnumerable()
                .Count(x => x.LastSold == null || x.LastSold < cutoffDate);

            var totalSalesThisMonth = db.Sales
                .Where(s => s.Date >= startOfMonth)
                .Select(s => s.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var productsSoldThisMonth = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth)
                .Select(si => (int?)si.Quantity)
                .Sum() ?? 0;

            return new KpiStats
            {
                TodaysSales = todaysSales,
                LowStockCount = lowStock,
                DeadStockCount = deadStock,
                TotalSalesThisMonth = totalSalesThisMonth,
                ProductsSoldThisMonth = productsSoldThisMonth
            };
        }


        private ProfitStats GetMonthlyProfitStats()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            var thisMonthProfit = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth)
                .Select(si => (si.PriceAtSale - si.Product.Cost) * si.Quantity)
                .DefaultIfEmpty(0)
                .Sum();

            var lastMonthProfit = db.SaleItems
                .Where(si => si.Sale.Date >= startOfLastMonth && si.Sale.Date <= endOfLastMonth)
                .Select(si => (si.PriceAtSale - si.Product.Cost) * si.Quantity)
                .DefaultIfEmpty(0)
                .Sum();

            decimal change = lastMonthProfit != 0
                ? ((thisMonthProfit - lastMonthProfit) / lastMonthProfit) * 100
                : 0;

            return new ProfitStats
            {
                ThisMonthProfit = thisMonthProfit,
                LastMonthProfit = lastMonthProfit,
                ChangePercent = change
            };
        }


        private decimal GetForecastedSales()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var last3MonthsStart = startOfMonth.AddMonths(-3);

            return db.Sales
                .Where(s => s.Date >= last3MonthsStart)
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g => g.Sum(s => s.TotalAmount))
                .DefaultIfEmpty(0)
                .Average();
        }


        private (List<string>, List<decimal>, List<decimal>) GetMonthlySalesChartData()
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-5);
            var months = new List<string>();
            var sales = new List<decimal>();
            var profits = new List<decimal>();

            for (int i = 0; i < 6; i++)
            {
                var m = start.AddMonths(i);
                months.Add(m.ToString("MMM"));

                var monthlySales = db.Sales
                    .Where(s => s.Date.Year == m.Year && s.Date.Month == m.Month)
                    .Select(s => s.TotalAmount)
                    .DefaultIfEmpty(0)
                    .Sum();
                sales.Add(monthlySales);

                var monthlyProfit = db.SaleItems
                    .Where(si => si.Sale.Date.Year == m.Year && si.Sale.Date.Month == m.Month)
                    .Select(si => (si.PriceAtSale - si.Product.Cost) * si.Quantity)
                    .DefaultIfEmpty(0)
                    .Sum();
                profits.Add(monthlyProfit);
            }

            return (months, sales, profits);
        }

        private (List<string>, List<int>) GetPeakSalesHourData()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var saleHours = db.Sales
                .Where(s => s.Date >= startOfMonth)
                .GroupBy(s => s.Date.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToDictionary(g => g.Hour, g => g.Count);

            var labels = Enumerable.Range(0, 24)
                .Select(h => h.ToString("D2") + ":00")
                .ToList();

            var counts = Enumerable.Range(0, 24)
                .Select(h => saleHours.ContainsKey(h) ? saleHours[h] : 0)
                .ToList();

            return (labels, counts);
        }



        private MiniCardStats GetMiniCardStats()
        {
            var startOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            var topProduct = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth)
                .GroupBy(si => si.Product.Name)
                .Select(g => new { Name = g.Key, Qty = g.Sum(si => si.Quantity) })
                .OrderByDescending(x => x.Qty)
                .FirstOrDefault();

            var topSalesperson = db.Sales
                .Where(s => s.Date >= startOfMonth)
                .GroupBy(s => s.User.Username)
                .Select(g => new { Name = g.Key, Total = g.Sum(s => s.TotalAmount) })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            var profitByProduct = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth)
                .GroupBy(si => si.Product.Name)
                .Select(g => new { Name = g.Key, Profit = g.Sum(si => (si.PriceAtSale - si.Product.Cost) * si.Quantity) })
                .ToList();

            var profit = profitByProduct.OrderByDescending(x => x.Profit).FirstOrDefault();
            var loss = profitByProduct.OrderBy(x => x.Profit).FirstOrDefault();

            var lastSaleDate = db.Sales
                .OrderByDescending(s => s.Date)
                .Select(s => (DateTime?)s.Date)
                .FirstOrDefault();

            int hoursSince = lastSaleDate.HasValue
                ? (int)Math.Round((DateTime.Now - lastSaleDate.Value).TotalHours)
                : -1;




            DateTime today = DateTime.Today;

            var fastestMoving = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth && si.Sale.Date <= today)
                .GroupBy(si => new { si.Product.ProductId, si.Product.Name, si.Product.DateEdited })
                .ToList()
                .Select(g =>
                {
                    var productDate = g.Key.DateEdited < today ? g.Key.DateEdited : today;
                    var activeDays = Math.Max(1, (today - productDate).Days + 1);
                    var qtySold = g.Sum(x => x.Quantity);
                    return new
                    {
                        Name = g.Key.Name,
                        UnitsPerDay = qtySold / (double)activeDays
                    };
                })
                .OrderByDescending(x => x.UnitsPerDay)
                .FirstOrDefault();

            int totalDays = (DateTime.Today - startOfMonth).Days + 1;

            return new MiniCardStats
            {
                TopProduct = topProduct?.Name ?? "N/A",
                TopProductQty = topProduct?.Qty ?? 0,
                TopSalesperson = topSalesperson?.Name ?? "N/A",
                TopSalespersonTotal = topSalesperson?.Total ?? 0,
                MostProfitableProduct = profit?.Name ?? "N/A",
                MostProfitAmount = profit?.Profit ?? 0,
                MostLossProduct = loss?.Name ?? "N/A",
                MostLossAmount = loss?.Profit ?? 0,
                TimeSinceLastSale = hoursSince,
                FastestMovingProduct = fastestMoving?.Name ?? "N/A",
                UnitsPerDay = fastestMoving != null ? Math.Round(fastestMoving.UnitsPerDay, 2) : 0
            };
        }


        private ManagerDashboardViewModel GetManagerDashboardViewModel()
        {
            var kpi = GetKpiStats();
            var profit = GetMonthlyProfitStats();
            var forecast = GetForecastedSales();
            var (months, sales, profits) = GetMonthlySalesChartData();
            var (hourLabels, hourCounts) = GetPeakSalesHourData();
            var mini = GetMiniCardStats();

            return new ManagerDashboardViewModel
            {
                TodaysSales = kpi.TodaysSales,
                LowStockCount = kpi.LowStockCount,
                DeadStockCount = kpi.DeadStockCount,
                TotalSalesThisMonth = kpi.TotalSalesThisMonth,
                ProductsSoldThisMonth = kpi.ProductsSoldThisMonth,
                ThisMonthProfit = profit.ThisMonthProfit,
                LastMonthProfit = profit.LastMonthProfit,
                MonthlyProfitChangePercent = profit.ChangePercent,
                ForecastedSales = forecast,
                Last6Months = months,
                MonthlySales = sales,
                MonthlyProfits = profits,
                SaleHours = hourLabels,
                HourlySalesCount = hourCounts,
                TopProductName = mini.TopProduct,
                TopProductQty = mini.TopProductQty,
                TopSalespersonName = mini.TopSalesperson,
                TopSalespersonTotal = mini.TopSalespersonTotal,
                MostProfitableProductName = mini.MostProfitableProduct,
                MostProfitableProductAmount = mini.MostProfitAmount,
                MostLossProductName = mini.MostLossProduct,
                MostLossProductAmount = mini.MostLossAmount,
                TimeSinceLastSale = mini.TimeSinceLastSale,
                FastestMovingProductName = mini.FastestMovingProduct,
                FastestMovingRate = mini.UnitsPerDay,
                AvailableProducts = db.Products.Count(p => p.IsActive)
            };
        }



        // Salesperson Dashboard Stuffs ------------------------------------------------------

        private class TodayStats
        {
            public int Count { get; set; }
            public decimal Total { get; set; }
        }

        private class MonthlyStats
        {
            public decimal ThisMonthTotal { get; set; }
            public decimal LastMonthTotal { get; set; }
            public decimal GrowthPercent { get; set; }
        }

        private class WeeklySalesStats
        {
            public List<string> Labels { get; set; }          
            public List<decimal> DailyTotals { get; set; }     
        }


        private TodayStats GetSalesTodayStats(string username)
        {
            var today = DateTime.Today;

            var sales = db.Sales
                .Where(s => DbFunctions.TruncateTime(s.Date) == today && s.User.Username == username)
                .ToList();

            return new TodayStats
            {
                Count = sales.Count,
                Total = sales.Sum(s => s.TotalAmount)
            };
        }

        private MonthlyStats GetMonthlySalesStats(string username)
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            var thisMonthTotal = db.Sales
                .Where(s => s.Date >= startOfMonth && s.User.Username == username)
                .Select(s => s.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var lastMonthTotal = db.Sales
                .Where(s => s.Date >= startOfLastMonth && s.Date <= endOfLastMonth && s.User.Username == username)
                .Select(s => s.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

            decimal growth = lastMonthTotal != 0
                ? ((thisMonthTotal - lastMonthTotal) / lastMonthTotal) * 100
                : 0;

            return new MonthlyStats
            {
                ThisMonthTotal = thisMonthTotal,
                LastMonthTotal = lastMonthTotal,
                GrowthPercent = growth
            };
        }

        private WeeklySalesStats GetLast7DaysSalesStats(string username)
        {
            var today = DateTime.Today;
            var startDate = today.AddDays(-6);

            var sales = db.Sales
                .Where(s => s.Date >= startDate && s.User.Username == username)
                .ToList();

            var days = Enumerable.Range(0, 7).Select(i => startDate.AddDays(i)).ToList();
            var labels = days.Select(d => d.ToString("ddd")).ToList(); // Mon, Tue, ...
            var totals = days.Select(d =>
                sales
                    .Where(s => s.Date.Date == d.Date)
                    .Sum(s => s.TotalAmount)
            ).ToList();

            return new WeeklySalesStats
            {
                Labels = labels,
                DailyTotals = totals
            };
        }


        private SalespersonDashboardViewModel GetSalespersonDashboardViewModel(string username)
        {
            var todayStats = GetSalesTodayStats(username);
            var monthStats = GetMonthlySalesStats(username);
            var weeklyStats = GetLast7DaysSalesStats(username);

            return new SalespersonDashboardViewModel
            {
                TodaysSaleCount = todayStats.Count,
                TodaysSaleTotal = todayStats.Total,
                ThisMonthSaleTotal = monthStats.ThisMonthTotal,
                LastMonthSaleTotal = monthStats.LastMonthTotal,
                SaleGrowthPercent = monthStats.GrowthPercent,
                Last7Days = weeklyStats.Labels,
                Last7DaysSales = weeklyStats.DailyTotals
            };
        }



    }



}
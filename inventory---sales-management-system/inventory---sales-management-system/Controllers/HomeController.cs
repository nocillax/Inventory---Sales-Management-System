using inventory___sales_management_system.Context;
using inventory___sales_management_system.Helpers;
using inventory___sales_management_system.ViewModels.Dashboard;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
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
            var message = "";

            if (role == "Manager")
            {
                var viewModel = GetManagerDashboardViewModel();
                return View("ManagerDashboard", viewModel);
            }
            //else if (role == "Salesperson")
            //{
            //    var viewModel = GetSalespersonDashboardViewModel();
            //    return View("SalespersonDashboard", viewModel);
            //}
            else
            {
                message = "Role not recognized";
            }

            ViewBag.RoleMessage = message;
            return View();
        }

        private DashboardViewModel GetManagerDashboardViewModel()
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;
            DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime startOfLastMonth = startOfMonth.AddMonths(-1);
            DateTime endOfLastMonth = startOfMonth.AddDays(-1);
            DateTime last3MonthsStart = startOfMonth.AddMonths(-3);
            DateTime last6MonthsStart = startOfMonth.AddMonths(-5);

            // KPI Cards
            var todaysSales = db.Sales
                .Where(s => DbFunctions.TruncateTime(s.Date) == today)
                .Select(s => s.TotalAmount)
                .DefaultIfEmpty(0)
                .Sum();

            var lowStockCount = db.Products
                .Count(p => p.IsActive && p.QuantityAvailable < p.LowStockThreshold);

            DateTime cutoffDate = DateTime.Today.AddDays(-60);
            var deadStockCount = db.Products
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

            // Profit
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

            decimal profitChangePercent = 0;
            if (lastMonthProfit != 0)
            {
                profitChangePercent = ((thisMonthProfit - lastMonthProfit) / lastMonthProfit) * 100;
            }

            // Forecast
            var forecastedSales = db.Sales
                .Where(s => s.Date >= last3MonthsStart)
                .GroupBy(s => new { s.Date.Year, s.Date.Month })
                .Select(g => g.Sum(s => s.TotalAmount))
                .DefaultIfEmpty(0)
                .Average();

            // Charts: Monthly
            var months = new List<string>();
            var sales = new List<decimal>();
            var profits = new List<decimal>();

            for (int i = 0; i < 6; i++)
            {
                var m = last6MonthsStart.AddMonths(i);
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

            // Chart: Peak Sales Hours (Monthly)
            var saleHours = db.Sales
                .Where(s => s.Date >= startOfMonth)
                .GroupBy(s => s.Date.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Hour)
                .ToList();

            var hourLabels = saleHours.Select(h => h.Hour.ToString("D2") + ":00").ToList();
            var hourCounts = saleHours.Select(h => h.Count).ToList();

            // Best Product This Month
            var topProduct = db.SaleItems
                .Where(si => si.Sale.Date >= startOfMonth)
                .GroupBy(si => si.Product.Name)
                .Select(g => new
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Quantity)
                .FirstOrDefault();

            // Best Salesperson This Month
            var topSalesperson = db.Sales
                .Where(s => s.Date >= startOfMonth)
                .GroupBy(s => s.User.Username)
                .Select(g => new
                {
                    Username = g.Key,
                    Total = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.Total)
                .FirstOrDefault();

            return new DashboardViewModel
            {
                TodaysSales = todaysSales,
                LowStockCount = lowStockCount,
                DeadStockCount = deadStockCount,
                TotalSalesThisMonth = totalSalesThisMonth,
                ThisMonthProfit = thisMonthProfit,
                LastMonthProfit = lastMonthProfit,
                MonthlyProfitChangePercent = profitChangePercent,
                ForecastedSales = Math.Round(forecastedSales, 2),
                Last6Months = months,
                MonthlySales = sales,
                MonthlyProfits = profits,
                SaleHours = hourLabels,
                HourlySalesCount = hourCounts,
                AvailableProducts = db.Products.Count(p => p.IsActive),
                ProductsSoldThisMonth = db.SaleItems
                    .Where(si => si.Sale.Date >= startOfMonth)
                    .Select(si => si.Quantity)
                    .DefaultIfEmpty(0)
                    .Sum(),
                TopProductName = topProduct?.ProductName ?? "N/A",
                TopProductQty = topProduct?.Quantity ?? 0,
                TopSalespersonName = topSalesperson?.Username ?? "N/A",
                TopSalespersonTotal = topSalesperson?.Total ?? 0
            };
        }


    }


}
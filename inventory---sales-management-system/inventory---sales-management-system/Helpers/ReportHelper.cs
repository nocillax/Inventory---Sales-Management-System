using inventory___sales_management_system.Context;
using inventory___sales_management_system.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.Helpers
{
    public class ReportHelper
    {
        public static List<SalesSummaryViewModel> GetMonthlySalesByDate(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var allDays = Enumerable.Range(0, (end - start).Days)
                .Select(offset => start.AddDays(offset))
                .ToList();

            var salesData = db.Sales
                .Where(s => s.Date >= start && s.Date < end)
                .GroupBy(s => DbFunctions.TruncateTime(s.Date))
                .ToList()
                .ToDictionary(
                    g => g.Key.Value,
                    g => g.Sum(s => s.TotalAmount)
                );

            var summary = allDays
                .Select(d => new SalesSummaryViewModel
                {
                    GroupLabel = d.ToString("yyyy-MM-dd"),
                    TotalSales = salesData.ContainsKey(d) ? salesData[d] : 0
                })
                .ToList();

            return summary;
        }

        public static List<SalesSummaryViewModel> GetMonthlySalesByUser(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.Sales
                .Where(s => s.Date >= start && s.Date < end)
                .GroupBy(s => s.User.Username)
                .Select(g => new SalesSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalSales = g.Sum(s => s.TotalAmount)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return summary;
        }

        public static List<SalesSummaryViewModel> GetMonthlySalesByCategory(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Product.Category.Name)
                .Select(g => new SalesSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalSales = g.Sum(x => x.Quantity * x.PriceAtSale)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return summary;
        }


        public static List<SalesSummaryViewModel> GetMonthlySalesByProduct(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Product.Name)
                .Select(g => new SalesSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalSales = g.Sum(x => x.Quantity * x.PriceAtSale)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            return summary;
        }

        public static List<ProductStockViewModel> GetProductStockList(ISMSDBContext db, bool lowStockOnly)
        {
            var query = db.Products
                .Where(p => p.IsActive);

            if (lowStockOnly)
            {
                query = query.Where(p => p.QuantityAvailable < p.LowStockThreshold);
            }

            var sorted = lowStockOnly
                ? query.OrderBy(p => p.QuantityAvailable)
                : query.OrderByDescending(p => p.QuantityAvailable);

            return sorted
                .Select(p => new ProductStockViewModel
                {
                    ProductName = p.Name,
                    QuantityAvailable = p.QuantityAvailable,
                    LowStockThreshold = p.LowStockThreshold
                })
                .ToList();
        }

        public static List<TopProductViewModel> GetTopProductsByQuantity(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Product.Name)
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.Key,
                    QuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Quantity * x.PriceAtSale)
                })
                .OrderByDescending(x => x.QuantitySold)
                .ToList();

            return summary;
        }

        public static List<SalesSummaryViewModel> GetYearlySalesSummary(ISMSDBContext db, int year)
        {
            var start = new DateTime(year, 1, 1);
            var end = start.AddYears(1);

            var salesData = db.Sales
                .Where(s => s.Date >= start && s.Date < end)
                .ToList();

            var grouped = salesData
                .GroupBy(s => s.Date.Month)
                .Select(g => new SalesSummaryViewModel
                {
                    GroupLabel = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    TotalSales = g.Sum(x => x.TotalAmount)
                })
                .ToList();

            // Ensure all 12 months are present
            var fullYear = Enumerable.Range(1, 12)
                .Select(m =>
                {
                    var found = grouped.FirstOrDefault(g => DateTime.ParseExact(g.GroupLabel, "MMMM", null).Month == m);
                    return found ?? new SalesSummaryViewModel
                    {
                        GroupLabel = new DateTime(year, m, 1).ToString("MMMM"),
                        TotalSales = 0
                    };
                }).ToList();

            return fullYear;
        }

        public static List<DeadStockViewModel> GetDeadStockReport(ISMSDBContext db, int days)
        {
            DateTime cutoff = DateTime.Today.AddDays(-days);

            var deadStock = db.Products
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Name,
                    p.QuantityAvailable,
                    LastSold = db.SaleItems
                        .Where(si => si.ProductId == p.ProductId)
                        .OrderByDescending(si => si.Sale.Date)
                        .Select(si => (DateTime?)si.Sale.Date)
                        .FirstOrDefault()
                })
                .Where(x => x.LastSold == null || x.LastSold < cutoff)
                .Select(x => new DeadStockViewModel
                {
                    ProductName = x.Name,
                    QuantityAvailable = x.QuantityAvailable,
                    LastSoldDate = x.LastSold
                })
                .OrderBy(x => x.LastSoldDate ?? DateTime.MinValue)
                .ToList();

            return deadStock;
        }

        public static List<ProfitSummaryViewModel> GetMonthlyProfitByDate(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var allDays = Enumerable.Range(0, (end - start).Days)
                .Select(offset => start.AddDays(offset))
                .ToList();

            var profitData = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => DbFunctions.TruncateTime(si.Sale.Date))
                .ToList()
                .ToDictionary(
                    g => g.Key.Value,
                    g => g.Sum(x => (x.PriceAtSale - x.Product.Cost) * x.Quantity)
                );

            var summary = allDays
                .Select(d => new ProfitSummaryViewModel
                {
                    GroupLabel = d.ToString("yyyy-MM-dd"),
                    TotalProfit = profitData.ContainsKey(d) ? profitData[d] : 0
                })
                .ToList();

            return summary;
        }


        public static List<ProfitSummaryViewModel> GetMonthlyProfitByUser(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Sale.User.Username)
                .Select(g => new ProfitSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalProfit = g.Sum(si => (si.PriceAtSale - si.Product.Cost) * si.Quantity)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToList();

            return summary;
        }

        public static List<ProfitSummaryViewModel> GetMonthlyProfitByCategory(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Product.Category.Name)
                .Select(g => new ProfitSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalProfit = g.Sum(x => (x.PriceAtSale - x.Product.Cost) * x.Quantity)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToList();

            return summary;
        }

        public static List<ProfitSummaryViewModel> GetMonthlyProfitByProduct(ISMSDBContext db, int year, int month)
        {
            DateTime start = new DateTime(year, month, 1);
            DateTime end = start.AddMonths(1);

            var summary = db.SaleItems
                .Where(si => si.Sale.Date >= start && si.Sale.Date < end)
                .GroupBy(si => si.Product.Name)
                .Select(g => new ProfitSummaryViewModel
                {
                    GroupLabel = g.Key,
                    TotalProfit = g.Sum(x => (x.PriceAtSale - x.Product.Cost) * x.Quantity)
                })
                .OrderByDescending(x => x.TotalProfit)
                .ToList();

            return summary;
        }
    }
}
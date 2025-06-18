using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.ViewModels.Report;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    
    public class ReportController : Controller
    {
        private ISMSDBContext db;

        public ReportController()
        {
            db = new ISMSDBContext();
        }

        public ActionResult PdfHeader()
        {
            return View("_PdfHeader");
        }

        public ActionResult PdfFooter()
        {
            return View("_PdfFooter");
        }


        private List<SalesSummaryViewModel> GetMonthlySalesByDate(int year, int month)
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

        private List<SalesSummaryViewModel> GetMonthlySalesByUser(int year, int month)
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

        private List<SalesSummaryViewModel> GetMonthlySalesByCategory(int year, int month)
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


        private List<SalesSummaryViewModel> GetMonthlySalesByProduct(int year, int month)
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

        private List<ProductStockViewModel> GetProductStockList(bool lowStockOnly)
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

        private List<TopProductViewModel> GetTopProductsByQuantity(int year, int month)
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

        private List<SalesSummaryViewModel> GetYearlySalesSummary(int year)
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

        private List<DeadStockViewModel> GetDeadStockReport(int days)
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

        private List<ProfitSummaryViewModel> GetMonthlyProfitByDate(int year, int month)
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


        private List<ProfitSummaryViewModel> GetMonthlyProfitByUser(int year, int month)
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

        private List<ProfitSummaryViewModel> GetMonthlyProfitByCategory(int year, int month)
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

        private List<ProfitSummaryViewModel> GetMonthlyProfitByProduct(int year, int month)
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




        // Action -------------------------------------------------------------------------------------



        [RoleAuthorize("Manager")]
        public ActionResult MonthlySalesSummary(int? year, int? month, string groupBy = "Date", int page = 1 )
        {
            int pageSize = 10;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            List<SalesSummaryViewModel> summary;

            switch (groupBy)
            {
                case "User":
                    summary = GetMonthlySalesByUser(selectedYear, selectedMonth);
                    break;
                case "Category":
                    summary = GetMonthlySalesByCategory(selectedYear, selectedMonth);
                    break;
                case "Product":
                    summary = GetMonthlySalesByProduct(selectedYear, selectedMonth);
                    break;
                default:
                    summary = GetMonthlySalesByDate(selectedYear, selectedMonth);
                    break;
            }

            // Chart Data (Top 10 only for non-date)
            if (groupBy != "Date")
            {
                var top10 = summary
                    .OrderByDescending(x => x.TotalSales)
                    .Take(10)
                    .ToList();

                ViewBag.ChartLabels = top10.Select(x => x.GroupLabel).ToList();
                ViewBag.ChartData = top10.Select(x => x.TotalSales).ToList();
            }
            else
            {
                ViewBag.ChartLabels = summary.Select(x => x.GroupLabel).ToList();
                ViewBag.ChartData = summary.Select(x => x.TotalSales).ToList();
            }

            // Pagination
            List<SalesSummaryViewModel> pagedItems;
            if (groupBy == "Date")
            {
                pagedItems = summary;
                ViewBag.TotalPages = 1;
            }
            else
            {
                pagedItems = summary.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.TotalPages = (int)Math.Ceiling((double)summary.Count / pageSize);
            }

            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;
            ViewBag.Page = page;
            ViewBag.GroupBy = groupBy;

            return View("MonthlySalesSummary", pagedItems);
        }

        [RoleAuthorize("Manager")]
        public ActionResult ProductStockReport(int page = 1, bool lowStockOnly = false)
        {
            int pageSize = 20;
            var allItems = GetProductStockList(lowStockOnly);

            var pagedItems = allItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)allItems.Count / pageSize);
            ViewBag.LowStockOnly = lowStockOnly;

            return View("ProductStockReport", pagedItems);
        }

        [RoleAuthorize("Manager")]
        public ActionResult TopProducts(int? year, int? month, int page = 1)
        {
            int pageSize = 10;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            
            var summary = GetTopProductsByQuantity(selectedYear, selectedMonth);

            
            var top10 = summary
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToList();

            ViewBag.ChartLabels = top10.Select(x => x.ProductName).ToList();
            ViewBag.ChartData = top10.Select(x => x.QuantitySold).ToList();

            
            var pagedItems = summary
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)summary.Count / pageSize);
            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;

            return View("TopProducts", pagedItems);
        }

        [RoleAuthorize("Manager")]
        public ActionResult YearlySalesSummary(int? year)
        {
            int selectedYear = year ?? DateTime.Now.Year;

            var summary = GetYearlySalesSummary(selectedYear);

            ViewBag.Year = selectedYear;
            ViewBag.ChartLabels = summary.Select(s => s.GroupLabel).ToList();
            ViewBag.ChartData = summary.Select(s => s.TotalSales).ToList();

            return View("YearlySalesSummary", summary);
        }

        [RoleAuthorize("Manager")]
        public ActionResult DeadStock(int days = 60, int page = 1)
        {
            int pageSize = 25;

            var fullList = GetDeadStockReport(days);
            var pagedList = fullList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Days = days;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)fullList.Count / pageSize);
            ViewBag.DayOptions = new List<int> { 30, 60, 90, 180, 365 };

            return View("DeadStock", pagedList);
        }

        [RoleAuthorize("Manager")]
        public ActionResult MonthlyProfitSummary(int? year, int? month, string groupBy = "Date", int page = 1)
        {
            int pageSize = 10;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            List<ProfitSummaryViewModel> summary;

            switch (groupBy)
            {
                case "User":
                    summary = GetMonthlyProfitByUser(selectedYear, selectedMonth);
                    break;
                case "Category":
                    summary = GetMonthlyProfitByCategory(selectedYear, selectedMonth);
                    break;
                case "Product":
                    summary = GetMonthlyProfitByProduct(selectedYear, selectedMonth);
                    break;
                default:
                    summary = GetMonthlyProfitByDate(selectedYear, selectedMonth);
                    break;
            }

            // Chart Data (Top 10 only for non-date)
            if (groupBy != "Date")
            {
                var top10 = summary
                    .OrderByDescending(x => x.TotalProfit)
                    .Take(10)
                    .ToList();

                ViewBag.ChartLabels = top10.Select(x => x.GroupLabel).ToList();
                ViewBag.ChartData = top10.Select(x => x.TotalProfit).ToList();
            }
            else
            {
                ViewBag.ChartLabels = summary.Select(x => x.GroupLabel).ToList();
                ViewBag.ChartData = summary.Select(x => x.TotalProfit).ToList();
            }

            // Pagination
            List<ProfitSummaryViewModel> pagedItems;
            if (groupBy == "Date")
            {
                pagedItems = summary;
                ViewBag.TotalPages = 1;
            }
            else
            {
                pagedItems = summary.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                ViewBag.TotalPages = (int)Math.Ceiling((double)summary.Count / pageSize);
            }

            ViewBag.Year = selectedYear;
            ViewBag.Month = selectedMonth;
            ViewBag.Page = page;
            ViewBag.GroupBy = groupBy;

            return View("MonthlyProfitSummary", pagedItems);
        }



        // PDF ----------------------------------------------------------------------------------------------



        [RoleAuthorize("Manager")]
        public ActionResult GenerateMonthlySalesSummaryPdf(int year, int month, string groupBy = "Date")
        {
            List<SalesSummaryViewModel> summary;

            switch (groupBy)
            {
                case "User":
                    summary = GetMonthlySalesByUser(year, month);
                    break;
                case "Category":
                    summary = GetMonthlySalesByCategory(year, month);
                    break;
                case "Product":
                    summary = GetMonthlySalesByProduct(year, month);
                    break;
                default:
                    summary = GetMonthlySalesByDate(year, month);
                    break;
            }

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.GroupBy = groupBy;

            return new Rotativa.ViewAsPdf("MonthlySalesSummaryPdf", summary)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
                    $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
                    "--header-spacing 5",
                    $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
                    "--footer-spacing 10",
                    "--margin-bottom 20mm"
                })
            };
        }

        [RoleAuthorize("Manager")]
        public ActionResult GenerateProductStockPdf(bool lowStockOnly = false)
        {
            var summary = GetProductStockList(lowStockOnly);

            ViewBag.LowStockOnly = lowStockOnly;

            return new Rotativa.ViewAsPdf("ProductStockReportPdf", summary)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
                    $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
                    "--header-spacing 5",
                    $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
                    "--footer-spacing 10",
                    "--margin-bottom 20mm"
                })
            };
        }

        [RoleAuthorize("Manager")]
        public ActionResult GenerateTopProductsPdf(int year, int month)
        {
            var summary = GetTopProductsByQuantity(year, month);

            ViewBag.Year = year;
            ViewBag.Month = month;

            return new Rotativa.ViewAsPdf("TopProductsPdf", summary)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
                    $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
                    "--header-spacing 5",
                    $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
                    "--footer-spacing 10",
                    "--margin-bottom 20mm"
                })
            };
        }

        [RoleAuthorize("Manager")]
        public ActionResult GenerateYearlySummaryPdf(int year)
        {
            var summary = GetYearlySalesSummary(year);
            ViewBag.Year = year;

            return new Rotativa.ViewAsPdf("YearlySalesSummaryPdf", summary)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
                    $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
                    "--header-spacing 5",
                    $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
                    "--footer-spacing 10"
                })
            };
        }

        [RoleAuthorize("Manager")]
        public ActionResult GenerateDeadStockPdf(int days = 60)
        {
            var report = GetDeadStockReport(days);
            ViewBag.Days = days;

            return new Rotativa.ViewAsPdf("DeadStockPdf", report)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
            $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
            "--header-spacing 5",
            $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
            "--footer-spacing 10"
        })
            };
        }


        [RoleAuthorize("Manager")]
        public ActionResult GenerateMonthlyProfitSummaryPdf(int year, int month, string groupBy = "Date")
        {
            List<ProfitSummaryViewModel> summary;

            switch (groupBy)
            {
                case "User":
                    summary = GetMonthlyProfitByUser(year, month);
                    break;
                case "Category":
                    summary = GetMonthlyProfitByCategory(year, month);
                    break;
                case "Product":
                    summary = GetMonthlyProfitByProduct(year, month);
                    break;
                default:
                    summary = GetMonthlyProfitByDate(year, month);
                    break;
            }

            ViewBag.Year = year;
            ViewBag.Month = month;
            ViewBag.GroupBy = groupBy;

            return new Rotativa.ViewAsPdf("MonthlyProfitSummaryPdf", summary)
            {
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = string.Join(" ", new[]
                {
                    $"--header-html \"{Url.Action("PdfHeader", "Report", null, Request.Url.Scheme)}\"",
                    "--header-spacing 5",
                    $"--footer-html \"{Url.Action("PdfFooter", "Report", null, Request.Url.Scheme)}\"",
                    "--footer-spacing 10",
                    "--margin-bottom 20mm"
                })
            };
        }

    }
}
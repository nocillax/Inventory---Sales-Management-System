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
    [RoleAuthorize("Manager")]
    public class ReportController : Controller
    {
        private ISMSDBContext db;

        public ReportController()
        {
            db = new ISMSDBContext();
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

        public ActionResult TopProducts(int? year, int? month, int page = 1)
        {
            int pageSize = 10;
            int selectedYear = year ?? DateTime.Now.Year;
            int selectedMonth = month ?? DateTime.Now.Month;

            // Full list of products sold that month
            var summary = GetTopProductsByQuantity(selectedYear, selectedMonth);

            // Prepare chart data (top 10)
            var top10 = summary
                .OrderByDescending(x => x.QuantitySold)
                .Take(10)
                .ToList();

            ViewBag.ChartLabels = top10.Select(x => x.ProductName).ToList();
            ViewBag.ChartData = top10.Select(x => x.QuantitySold).ToList();

            // Pagination for table
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





        public ActionResult PdfHeader()
        {
            return View("_PdfHeader");
        }

        public ActionResult PdfFooter()
        {
            return View("_PdfFooter");
        }


        public ActionResult GenerateMonthlySummaryPdf(int year, int month, string groupBy = "Date")
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


    }
}
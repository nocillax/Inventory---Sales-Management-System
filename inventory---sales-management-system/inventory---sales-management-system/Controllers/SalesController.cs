using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Tree;
using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;
using inventory___sales_management_system.ViewModels.Sale;
using Rotativa;
using static inventory___sales_management_system.Models.User;

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager", "Salesperson")]
    public class SalesController : Controller
    {
        private ISMSDBContext db = new ISMSDBContext();

        // GET: Sales
        public ActionResult Index(DateTime? startDate, DateTime? endDate, string sortBy = "Date", string sortOrder = "desc", int page = 1)
        {
            var role = Session["UserRole"]?.ToString();
            var userId = (int)Session["UserId"];
            int pageSize = 25;

            var salesQuery = db.Sales.Include(s => s.User).AsQueryable();

            if (role == "Salesperson")
            {
                salesQuery = salesQuery.Where(s => s.UserId == userId);
            }

            if (startDate.HasValue)
            {
                salesQuery = salesQuery.Where(s => DbFunctions.TruncateTime(s.Date) >= startDate);
            }

            if (endDate.HasValue)
            {
                salesQuery = salesQuery.Where(s => DbFunctions.TruncateTime(s.Date) <= endDate);
            }

            // Date range validation
            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                ModelState.AddModelError("", "Start Date cannot be after End Date.");
            }

            // Sorting
            switch (sortBy)
            {
                case "TotalAmount":
                    salesQuery = (sortOrder == "asc")
                        ? salesQuery.OrderBy(s => s.TotalAmount)
                        : salesQuery.OrderByDescending(s => s.TotalAmount);
                    break;
                default:
                    salesQuery = (sortOrder == "asc")
                        ? salesQuery.OrderBy(s => s.Date)
                        : salesQuery.OrderByDescending(s => s.Date);
                    break;
            }

            var totalCount = salesQuery.Count();

            var sales = salesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SaleHistoryViewModel
                {
                    SaleId = s.SaleId,
                    Date = s.Date,
                    SalesPersonName = s.User.Username,
                    BuyerName = s.BuyerName,
                    TotalAmount = s.TotalAmount
                }).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(sales);
        }




        // GET: Sales/Details/5
        public ActionResult Details(int id)
        {
            var sale = db.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleItems.Select(si => si.Product))
                         .FirstOrDefault(s => s.SaleId == id);

            if (sale == null)
                return HttpNotFound();

            var vm = new SaleDetailsViewModel
            {
                SaleId = sale.SaleId,
                SalesPersonName = sale.User?.Username,
                Date = sale.Date,
                BuyerName = sale.BuyerName,
                TotalAmount = sale.TotalAmount,
                SaleItems = sale.SaleItems.Select(si => new SaleItemViewModel
                {
                    ProductName = si.Product?.Name ?? "N/A",
                    Quantity = si.Quantity,
                    RegularPrice = si.Product?.Price ?? 0,
                    DiscountPercent = (si.Product != null && si.Product.IsOnSale) ? si.Product.DiscountPercent : null,
                    PriceAtSale = si.PriceAtSale
                }).ToList()

            };

            return View(vm);
        }


        // GET: Sales/Create
        public ActionResult Create()
        {
            var vm = new CreateSaleViewModel
            {
                ProductsList = db.Products.Where(p => p.IsActive).ToList()
            };
            return View(vm);
        }


        // POST: Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateSaleViewModel vm)
        {
            if (!ModelState.IsValid || vm.ProductIds == null || vm.ProductIds.Length == 0)
            {
                ModelState.AddModelError("", "Please fill in all required fields and select at least one product.");
                vm.ProductsList = db.Products.Where(p => p.IsActive).ToList();
                return View(vm);
            }

            int userId = (int)Session["UserId"];
            var sale = new Sale
            {
                Date = vm.Date,
                BuyerName = vm.BuyerName,
                UserId = userId,
                SaleItems = new List<SaleItem>()
            };

            decimal totalAmount = 0;

            for (int i = 0; i < vm.ProductIds.Length; i++)
            {
                int productId = vm.ProductIds[i];
                int qty = vm.Quantities[i];

                var product = db.Products.Find(productId);
                if (product == null || !product.IsActive || product.QuantityAvailable < qty)
                {
                    ModelState.AddModelError("", $"Invalid product or insufficient stock for product ID {productId}.");
                    vm.ProductsList = db.Products.Where(p => p.IsActive).ToList();
                    return View(vm);
                }

                decimal discountPercent = (product.IsOnSale && product.DiscountPercent.HasValue) ? product.DiscountPercent.Value : 0;

                decimal priceAtSale = product.Price * (1 - discountPercent / 100);
                decimal totalPrice = priceAtSale * qty;
                totalAmount += totalPrice;

                product.QuantityAvailable -= qty;

                sale.SaleItems.Add(new SaleItem
                {
                    ProductId = productId,
                    Quantity = qty,
                    PriceAtSale = priceAtSale
                });
            }

            sale.TotalAmount = totalAmount;

            db.Sales.Add(sale);
            db.SaveChanges();

            TempData["SaleCreated"] = true;
            TempData["SaleId"] = sale.SaleId;

            return RedirectToAction("Details", new { id = sale.SaleId });
        }

        public ActionResult Invoice(int id)
        {
            var sale = db.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleItems.Select(si => si.Product))
                         .FirstOrDefault(s => s.SaleId == id);

            return new Rotativa.ViewAsPdf("Invoice", sale)
            {
                //FileName = $"Invoice_{sale.SaleId}.pdf",
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

        public ActionResult PdfHeader()
        {
            return View("_PdfHeader");
        }

        public ActionResult PdfFooter()
        {
            return View("_PdfFooter");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

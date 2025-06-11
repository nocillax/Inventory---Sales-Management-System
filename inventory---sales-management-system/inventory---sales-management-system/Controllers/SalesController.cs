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
        public ActionResult Index()
        {
            var role = Session["UserRole"]?.ToString();
            var userId = (int)Session["UserId"]; // Assuming UserId is stored as int in session

            var salesQuery = db.Sales.Include(s => s.User);

            if (role == "Salesperson")
            {
                salesQuery = salesQuery.Where(s => s.UserId == userId);
            }

            var sales = salesQuery.Select(s => new SaleHistoryViewModel
            {
                SaleId = s.SaleId,
                Date = s.Date,
                SalesPersonName = s.User.Username,
                BuyerName = s.BuyerName,
                TotalAmount = s.TotalAmount
            }).ToList();

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

                decimal priceAtSale = product.Price;
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

            return RedirectToAction("Index");
        }

        public ActionResult Invoice(int id)
        {
            var sale = db.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleItems.Select(si => si.Product))
                         .FirstOrDefault(s => s.SaleId == id);

            return new ViewAsPdf("Invoice", sale)
            {
                //FileName = $"Invoice_{sale.SaleId}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait
            };
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

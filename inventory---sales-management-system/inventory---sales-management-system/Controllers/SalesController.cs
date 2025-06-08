using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime.Tree;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;
using Rotativa;

namespace inventory___sales_management_system.Controllers
{
    public class SalesController : Controller
    {
        private ISMSDBContext db = new ISMSDBContext();

        // GET: Sales
        public ActionResult Index()
        {
            var sales = db.Sales.Include(s => s.User);
            return View(sales.ToList());
        }

        // GET: Sales/Details/5
        public ActionResult Details(int id)
        {
            var sale = db.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleItems.Select(si => si.Product))
                         .FirstOrDefault(s => s.SaleId == id);

            return View(sale);
        }

        // GET: Sales/Create
        public ActionResult Create()
        {
            var productsListItems = db.Products
                .Where(p => p.IsActive)
                .ToList();

            return View(productsListItems); 
        }


        // POST: Sales/Create
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Create(DateTime date, string buyerName, int[] productIds, int[] quantities) 
        {
            System.Diagnostics.Debug.WriteLine($"Date: {date}, Buyer: {buyerName}");
            System.Diagnostics.Debug.WriteLine($"Products count: {productIds?.Length ?? 0}");
            System.Diagnostics.Debug.WriteLine($"Quantities count: {quantities?.Length ?? 0}");


            if (!ModelState.IsValid)
            {
                ViewBag.ProductsList = new SelectList(db.Products.Where(p => p.IsActive), "ProductId", "Name");
                return View();
            }

            if (productIds == null || productIds.Length == 0)
            {
                ModelState.AddModelError("", "Please select at least one product.");
                ViewBag.ProductsList = new SelectList(db.Products.Where(p => p.IsActive), "ProductId", "Name");
                return View();
            }

            int userId = 1;

            var sale = new Sale
            {
                Date = date,
                BuyerName = buyerName,
                UserId = userId,
                SaleItems = new List<SaleItem>()
            };

            decimal totalAmount = 0;

            for (int i = 0; i < productIds.Length; i++)
            {
                int productId = productIds[i];
                int qty = quantities[i];

                var product = db.Products.Find(productId);

                if (product == null || !product.IsActive)
                {
                    ModelState.AddModelError("", $"Product with ID {productId} not found or inactive.");
                    ViewBag.ProductsList = new SelectList(db.Products.Where(p => p.IsActive), "ProductId", "Name");
                    return View();
                }

                if (product.QuantityAvailable < qty)
                {
                    ModelState.AddModelError("", $"Insufficient stock for product {product.Name}. Available: {product.QuantityAvailable}");
                    ViewBag.ProductsList = new SelectList(db.Products.Where(p => p.IsActive), "ProductId", "Name");
                    return View();
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

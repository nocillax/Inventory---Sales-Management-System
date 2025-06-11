using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;
using inventory___sales_management_system.ViewModels;
using inventory___sales_management_system.ViewModels.Product;
using inventory___sales_management_system.ViewModels.Stock;

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager", "Salesperson")]
    public class ProductsController : Controller
    {
        private ISMSDBContext db = new ISMSDBContext();

        // GET: Products
        public ActionResult Index()
        {
            var role = Session["UserRole"]?.ToString();
            var products = db.Products.Include(p => p.Category);

            if (role == "Salesperson")
            {
                products = products.Where(p => p.IsActive);
            }

            return View(products.ToList());
        }


        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [RoleAuthorize("Manager")]
        public ActionResult Create()
        {
            var vm = new CreateProductViewModel
            {
                Categories = new SelectList(db.Categories, "CategoryId", "Name")
            };
            return View(vm);
        }

        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateProductViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = vm.Name,
                    CategoryId = vm.CategoryId,
                    Price = 0m,
                    Cost = 0m,
                    QuantityAvailable = 0,
                    LowStockThreshold = 0,
                    IsActive = false,
                    DateEdited = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            vm.Categories = new SelectList(db.Categories, "CategoryId", "Name", vm.CategoryId);

            return View(vm);
        }


        [RoleAuthorize("Manager")]
        public ActionResult Edit(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var vm = new EditProductViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                Cost = product.Cost,
                QuantityAvailable = product.QuantityAvailable,
                LowStockThreshold = product.LowStockThreshold,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                Categories = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId)
            };

            return View(vm);
        }

        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditProductViewModel vm)
        {
            if (ModelState.IsValid)
            {
                var product = db.Products.Find(vm.ProductId);
                if (product == null) return HttpNotFound();

                product.Name = vm.Name;
                product.Price = vm.Price;
                product.Cost = vm.Cost;
                product.QuantityAvailable = vm.QuantityAvailable;
                product.LowStockThreshold = vm.LowStockThreshold;
                product.IsActive = vm.IsActive;
                product.CategoryId = vm.CategoryId;
                product.DateEdited = DateTime.Now;

                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            vm.Categories = new SelectList(db.Categories, "CategoryId", "Name", vm.CategoryId);
            return View(vm);
        }


        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var product = db.Products.Find(id);
            if (product == null)
                return HttpNotFound();

            db.Products.Remove(product);
            db.SaveChanges();

            TempData["DeleteMessage"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }




        // GET: AddStock
        [RoleAuthorize("Manager")]
        public ActionResult AddStock(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
                return HttpNotFound();

            ViewBag.ProductName = product.Name;

            var vm = new AddStockViewModel
            {
                ProductId = productId
            };

            return View(vm);
        }

        // POST: AddStock
        [RoleAuthorize("Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(AddStockViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var product = db.Products.Find(vm.ProductId);
                ViewBag.ProductName = product?.Name;
                return View(vm);
            }

            var productToUpdate = db.Products.Find(vm.ProductId);
            if (productToUpdate == null)
                return HttpNotFound();

            // Calculate weighted average cost
            decimal totalCurrentCost = productToUpdate.Cost * productToUpdate.QuantityAvailable;
            decimal totalNewCost = vm.CostPerQty * vm.QuantityAdded;
            int newQuantity = productToUpdate.QuantityAvailable + vm.QuantityAdded;

            productToUpdate.Cost = (totalCurrentCost + totalNewCost) / newQuantity;
            productToUpdate.QuantityAvailable = newQuantity;

            // Set audit info
            var stockEntry = new StockEntry
            {
                ProductId = vm.ProductId,
                Supplier = vm.Supplier,
                CostPerQty = vm.CostPerQty,
                QuantityAdded = vm.QuantityAdded,
                DateAdded = DateTime.Now,
                UserId = (int)Session["UserId"]
            };

            db.StockEntries.Add(stockEntry);
            db.SaveChanges();

            return RedirectToAction("Details", "Products", new { id = vm.ProductId });
        }


        [RoleAuthorize("Manager")]
        public ActionResult StockHistory(int? productId)
        {
            var query = db.StockEntries.Include(se => se.Product).Include(se => se.User).AsQueryable();

            if (productId.HasValue)
            {
                query = query.Where(se => se.ProductId == productId.Value);
            }

            var stockHistoryList = query.Select(se => new StockHistoryViewModel
            {
                DateAdded = se.DateAdded,
                Supplier = se.Supplier,
                CostPerQty = se.CostPerQty,
                QuantityAdded = se.QuantityAdded,
                ProductName = se.Product.Name,
                AddedByUsername = se.User.Username
            }).OrderByDescending(se => se.DateAdded).ToList();

            if (productId.HasValue)
            {
                ViewBag.ProductId = productId.Value;
                ViewBag.ProductName = db.Products
                    .Where(p => p.ProductId == productId.Value)
                    .Select(p => p.Name)
                    .FirstOrDefault();
            }

            return View(stockHistoryList);
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

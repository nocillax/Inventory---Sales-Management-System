using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;

namespace inventory___sales_management_system.Controllers
{
    public class ProductsController : Controller
    {
        private ISMSDBContext db = new ISMSDBContext();

        // GET: Products
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category);
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

        // GET: Products/Create
        public ActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductId,Name,Price,Cost,QuantityAvailable,LowStockThreshold,IsActive,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
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
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ProductId,Name,Price,Cost,QuantityAvailable,LowStockThreshold,IsActive,CategoryId")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult AddStock(int productId)
        {
            var product = db.Products.Find(productId);

            ViewBag.ProductName = product.Name;
            return View(new StockEntry { ProductId = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddStock(StockEntry stockEntry)
        {
            if (!ModelState.IsValid)
            {
                var product = db.Products.Find(stockEntry.ProductId);
                ViewBag.ProductName = product?.Name;
                return View(stockEntry);
            }

            var productToUpdate = db.Products.Find(stockEntry.ProductId);
            if (productToUpdate == null)
                return HttpNotFound();

            // Calculate weighted average cost
            decimal totalCurrentCost = productToUpdate.Cost * productToUpdate.QuantityAvailable;
            decimal totalNewCost = stockEntry.CostPerQty * stockEntry.QuantityAdded;
            int newQuantity = productToUpdate.QuantityAvailable + stockEntry.QuantityAdded;

            productToUpdate.Cost = (totalCurrentCost + totalNewCost) / newQuantity;
            productToUpdate.QuantityAvailable = newQuantity;

            // Set audit info (replace with logged in user ID when auth done)
            stockEntry.DateAdded = DateTime.Now;
            stockEntry.UserId = 2; // TODO: replace with actual user ID

            db.StockEntries.Add(stockEntry);
            db.SaveChanges();

            return RedirectToAction("Details", "Products", new { id = stockEntry.ProductId });
        }

        public ActionResult StockHistory(int productId)
        {
            var product = db.Products.Find(productId);
            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.ProductName = product.Name;
            ViewBag.ProductId = product.ProductId;

            var stockEntries = db.StockEntries
                                 .Include(se => se.User) // include user info
                                 .Where(se => se.ProductId == productId)
                                 .OrderByDescending(se => se.DateAdded)
                                 .ToList();

            return View(stockEntries);
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

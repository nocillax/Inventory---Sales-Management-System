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


        public ActionResult Index(int page = 1, string sortBy = "Name", string sortOrder = "asc", string isActiveFilter = null, string onSaleFilter = null, decimal? minPrice = null, decimal? maxPrice = null, int? categoryFilter = null)
        {
            int pageSize = 25;
            var role = Session["UserRole"]?.ToString();

            var query = db.Products.Include(p => p.Category).AsQueryable();

            // Salesperson sees only active products
            if (role == "Salesperson")
            {
                query = query.Where(p => p.IsActive);
            }

            // Filtering
            if (!string.IsNullOrEmpty(isActiveFilter))
            {
                bool isActive = bool.Parse(isActiveFilter);
                query = query.Where(p => p.IsActive == isActive);
            }

            if (!string.IsNullOrEmpty(onSaleFilter))
            {
                bool isOnSale = bool.Parse(onSaleFilter);
                query = query.Where(p => p.IsOnSale == isOnSale);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            {
                ModelState.AddModelError("", "Min Price cannot be higher than Max Price.");
            }

            if (categoryFilter.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryFilter.Value);
            }

            // Sorting
            switch (sortBy)
            {
                case "Name":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
                case "Category":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Category.Name) : query.OrderByDescending(p => p.Category.Name);
                    break;
                case "Price":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "Discount":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.DiscountPercent) : query.OrderByDescending(p => p.DiscountPercent);
                    break;
                case "Quantity":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.QuantityAvailable) : query.OrderByDescending(p => p.QuantityAvailable);
                    break;
                case "Status":
                    query = sortOrder == "asc" ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive);
                    break;
                default:
                    query = sortOrder == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
            }

            // Pagination
            int totalItems = query.Count();
            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // ViewBag
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.IsActiveFilter = isActiveFilter;
            ViewBag.OnSaleFilter = onSaleFilter;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.CategoryFilter = categoryFilter;

            // Category dropdown
            var categories = db.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "Name", categoryFilter);

            return View(products);
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
                    IsOnSale = false,
                    DiscountPercent = null,
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
                IsOnSale = product.IsOnSale,
                DiscountPercent = product.DiscountPercent,
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
                product.IsOnSale = vm.IsOnSale;
                product.DiscountPercent = vm.DiscountPercent;
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

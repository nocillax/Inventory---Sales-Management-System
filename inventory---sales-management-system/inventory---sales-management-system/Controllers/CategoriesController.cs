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

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager")]
    public class CategoriesController : Controller
    {
        private ISMSDBContext db = new ISMSDBContext();

        // GET: Categories
        public ActionResult Index(int page = 1, string sortBy = "Name", string sortOrder = "asc")
        {
            int pageSize = 25;
            var query = db.Categories.AsQueryable();

            // Sorting logic
            switch (sortBy)
            {
                case "Name":
                default:
                    query = sortOrder == "asc"
                        ? query.OrderBy(c => c.Name)
                        : query.OrderByDescending(c => c.Name);
                    break;
            }

            int totalItems = query.Count();
            var categories = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;

            return View(categories);
        }



        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryId,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryId,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            var products = db.Products.Where(p => p.CategoryId == id).ToList();
            foreach (var product in products)
            {
                product.CategoryId = null;
            }

            db.Categories.Remove(category);
            db.SaveChanges();

            TempData["DeleteMessage"] = "Category deleted successfully. Products updated.";
            return RedirectToAction("Index");
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

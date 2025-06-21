using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;
using inventory___sales_management_system.ViewModels.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    public class SearchController : Controller
    {
        private ISMSDBContext db;

        public SearchController()
        {
            db = new ISMSDBContext();
        }

        [HttpGet]
        public ActionResult AjaxSearch(string query)
        {
            string role = Session["UserRole"]?.ToString();
            var model = new GlobalSearchViewModel
            {
                Query = query,
                Products = db.Products
                             .Where(p => p.IsActive && p.Name.Contains(query))
                             .Take(5)
                             .ToList(),
                Categories = new List<Category>(),
                Users = new List<User>()
            };

            if (role == "Manager")
            {
                model.Categories = db.Categories
                                     .Where(c => c.Name.Contains(query))
                                     .Take(3)
                                     .ToList();

                model.Users = db.Users
                                .Where(u => u.Username.Contains(query) || u.Email.Contains(query))
                                .Take(3)
                                .ToList();
            }

            return PartialView("_AjaxSearchResults", model);
        }
    }
}
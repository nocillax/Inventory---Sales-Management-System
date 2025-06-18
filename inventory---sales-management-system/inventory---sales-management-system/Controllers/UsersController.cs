using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using static inventory___sales_management_system.Models.User;

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager")]
    public class UsersController : Controller
    {
        private ISMSDBContext db;

        public UsersController()
        {
            db = new ISMSDBContext();
        }

        // GET: Users
        public ActionResult Index(int page = 1, string sortBy = "Username", string sortOrder = "asc", int? roleFilter = null)
        {
            int pageSize = 25;
            var query = db.Users.AsQueryable();

            // Apply role filter if provided
            if (roleFilter.HasValue)
            {
                var selectedRole = (UserRole)roleFilter.Value;
                query = query.Where(u => u.Role == selectedRole);
            }

            // Sorting
            switch (sortBy)
            {
                case "Username":
                default:
                    query = sortOrder == "asc"
                        ? query.OrderBy(u => u.Username)
                        : query.OrderByDescending(u => u.Username);
                    break;
            }

            int totalItems = query.Count();
            var users = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Populate dropdown with enum values
            var roles = Enum.GetValues(typeof(UserRole))
                            .Cast<UserRole>()
                            .Select(r => new SelectListItem
                            {
                                Value = ((int)r).ToString(),
                                Text = r.ToString()
                            }).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.RoleList = new SelectList(roles, "Value", "Text", roleFilter?.ToString());

            return View(users);
        }



        public ActionResult Details(int id)
        {

            var user = db.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        public ActionResult Create()
        {
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(User.UserRole)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(User user, string ConfirmPassword)
        {
            // Check if passwords match
            if (user.PasswordHash == null || ConfirmPassword == null || !user.PasswordHash.Equals(ConfirmPassword))
            {
                ModelState.AddModelError("PasswordHash", "Passwords do not match.");
            }

            // Check email uniqueness
            if (db.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(Enum.GetValues(typeof(User.UserRole)), user.Role);
                return View(user);
            }

            // Hash the password before saving
            user.PasswordHash = HashPassword(user.PasswordHash);

            db.Users.Add(user);
            db.SaveChanges();

            TempData["Message"] = "User created successfully!";
            return RedirectToAction("Index");
        }


        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(User.UserRole)), user.Role);


            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string NewPassword, string ConfirmPassword, string Username, string Email, User.UserRole Role)
        {
            var userInDb = db.Users.Find(id);
            if (userInDb == null)
                return HttpNotFound();

            // Manually assign properties from parameters
            userInDb.Username = Username;
            userInDb.Email = Email;
            userInDb.Role = Role;

            // Password update logic
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (NewPassword != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    ViewBag.Roles = new SelectList(Enum.GetValues(typeof(User.UserRole)), Role);
                    return View(userInDb);
                }
                userInDb.PasswordHash = HashPassword(NewPassword);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(Enum.GetValues(typeof(User.UserRole)), Role);
                return View(userInDb);
            }

            db.SaveChanges();

            TempData["Message"] = "User updated successfully!";
            return RedirectToAction("Index");
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            TempData["DeleteMessage"] = "User deleted successfully!";
            return RedirectToAction("Index");
        }




    }
}
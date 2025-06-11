using inventory___sales_management_system.Context;
using inventory___sales_management_system.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    public class AccountController : Controller
    {
        private ISMSDBContext db;

        public AccountController()
        {
            db = new ISMSDBContext();
        }

        // GET: Account
        public ActionResult Login()
        {
            if (Session["Username"] != null)
            {
                // User already logged in, redirect to homepage/dashboard
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string hashedPassword = HashPassword(model.Password);

            var user = db.Users
                .FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            // Set session or auth cookie here
            Session["UserId"] = user.UserId;
            Session["Username"] = user.Username;
            Session["UserRole"] = user.Role.ToString();

            // Redirect to dashboard or homepage
            return RedirectToAction("Index", "Home");
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

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

    }
}
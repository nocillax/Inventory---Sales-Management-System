using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Enums;
using inventory___sales_management_system.Models;
using inventory___sales_management_system.ViewModels.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    [RoleAuthorize("Manager")]
    public class UsersController : Controller
    {
        public async Task<ActionResult> Index(int page = 1, string sortBy = "Username", string sortOrder = "asc", int? roleFilter = null)
        {
            var apiUrl = $"api/users?page={page}&sortBy={sortBy}&sortOrder={sortOrder}";
            if (roleFilter.HasValue)
                apiUrl += $"&roleFilter={roleFilter.Value}";

            var model = new List<UserViewModel>();
            int totalPages = 1;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    dynamic parsed = JsonConvert.DeserializeObject(result);
                    model = JsonConvert.DeserializeObject<List<UserViewModel>>(parsed.Users.ToString());
                    totalPages = (int)parsed.TotalPages;
                }
                else
                {
                    TempData["Error"] = "Failed to fetch user list.";
                }
            }

            var roles = Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .Select(r => new SelectListItem
                {
                    Value = ((int)r).ToString(),
                    Text = r.ToString()
                }).ToList();

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SortBy = sortBy;
            ViewBag.SortOrder = sortOrder;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.RoleList = new SelectList(roles, "Value", "Text", roleFilter?.ToString());

            return View(model);
        }

        public ActionResult Create()
        {
            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)));
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserViewModel userVM, string ConfirmPassword)
        {
            if (HashPassword(userVM.Password) == null || ConfirmPassword == null || HashPassword(userVM.Password) != ConfirmPassword)
            {
                ModelState.AddModelError("PasswordHash", "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)), userVM.Role);
                return View(userVM);
            }

            var user = new User
            {
                Username = userVM.Username,
                Email = userVM.Email,
                PasswordHash = HashPassword(userVM.Password),
                Role = userVM.Role
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var url = $"api/users?confirmPassword={ConfirmPassword}";
                var response = await client.PostAsJsonAsync(url, user);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Message"] = "User created via API successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "API Error: " + error);
                }
            }

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)), userVM.Role);
            return View(userVM);
        }

        public async Task<ActionResult> Details(int id)
        {
            UserViewModel user = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var response = await client.GetAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadAsAsync<UserViewModel>();
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpNotFound();
                }
                else
                {
                    TempData["Error"] = "Failed to fetch user from API.";
                    return RedirectToAction("Index");
                }
            }

            return View(user);
        }

        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            UserViewModel user = null;

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var response = await client.GetAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    user = await response.Content.ReadAsAsync<UserViewModel>();
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpNotFound();
                }
                else
                {
                    TempData["Error"] = "Failed to fetch user for edit.";
                    return RedirectToAction("Index");
                }
            }

            ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)), user.Role);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, string NewPassword, string ConfirmPassword, string Username, string Email, UserRole Role)
        {
            if (!string.IsNullOrEmpty(NewPassword) && NewPassword != ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)), Role);
                return View(new UserViewModel
                {
                    UserId = id,
                    Username = Username,
                    Email = Email,
                    Role = Role
                });
            }

            var updatedUser = new User
            {
                UserId = id,
                Username = Username,
                Email = Email,
                Role = Role
            };

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var response = await client.PutAsJsonAsync($"api/users/{id}", updatedUser);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError("", "API Error: " + error);
                    ViewBag.Roles = new SelectList(Enum.GetValues(typeof(UserRole)), Role);
                    return View(new UserViewModel
                    {
                        UserId = id,
                        Username = Username,
                        Email = Email,
                        Role = Role
                    });
                }

                if (!string.IsNullOrEmpty(NewPassword))
                {
                    var pwResponse = await client.PutAsync(
                        $"api/users/{id}/password?newPassword={NewPassword}&confirmPassword={ConfirmPassword}",
                        null);

                    if (!pwResponse.IsSuccessStatusCode)
                    {
                        TempData["Warning"] = "User updated but password was not.";
                    }
                }
            }

            TempData["Message"] = "User updated successfully!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:58370/");
                var response = await client.DeleteAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["DeleteMessage"] = "User deleted successfully!";
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return HttpNotFound();
                }
                else
                {
                    TempData["Error"] = "Failed to delete user via API.";
                }
            }

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
    }
}

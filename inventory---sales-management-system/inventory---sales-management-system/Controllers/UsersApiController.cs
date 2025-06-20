using inventory___sales_management_system.Attributes;
using inventory___sales_management_system.Context;
using inventory___sales_management_system.Enums;
using inventory___sales_management_system.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using static inventory___sales_management_system.Models.User;

namespace inventory___sales_management_system.Controllers
{

    //[ApiRoleAuthorize("Manager")]
    public class UsersApiController : ApiController
    {

        private ISMSDBContext db = new ISMSDBContext();

        [HttpGet]
        [Route("api/users")]
        public IHttpActionResult GetAllUsers(int page = 1, string sortBy = "Username", string sortOrder = "asc", int? roleFilter = null)
        {
            int pageSize = 25;
            var query = db.Users.AsQueryable();

            if (roleFilter.HasValue)
            {
                var selectedRole = (UserRole)roleFilter.Value;
                query = query.Where(u => u.Role == selectedRole);
            }

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
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    Role = u.Role.ToString()
                })
                .ToList();

            return Ok(new
            {
                Users = users,
                Page = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                SortBy = sortBy,
                SortOrder = sortOrder,
                RoleFilter = roleFilter
            });
        }



        [HttpGet]
        [Route("api/users/{id:int}")]
        public IHttpActionResult GetUser(int id)
        {
            var user = db.Users
                .Where(u => u.UserId == id)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.Email,
                    u.Role
                }).FirstOrDefault();

            if (user == null) return NotFound();
            return Ok(user);
        }


        [HttpPost]
        [Route("api/users")]
        public IHttpActionResult CreateUser(User user, [FromUri] string confirmPassword)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(user.PasswordHash))
                return BadRequest("Password is required.");

            if (user.PasswordHash != confirmPassword)
                return BadRequest("Passwords do not match.");

            if (db.Users.Any(u => u.Email == user.Email))
                return BadRequest("Email already exists.");

            if (db.Users.Any(u => u.Username == user.Username))
                return BadRequest("Username already exists.");

            user.PasswordHash = HashPassword(user.PasswordHash);
            db.Users.Add(user);
            db.SaveChanges();

            return Created($"api/users/{user.UserId}", new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.Role
            });
        }


        [HttpPut]
        [Route("api/users/{id:int}")]
        public IHttpActionResult UpdateUser(int id, User updatedUser)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = db.Users.Find(id);
            if (user == null) return NotFound();

            // Check for duplicate email (excluding current user)
            if (db.Users.Any(u => u.Email == updatedUser.Email && u.UserId != id))
                return BadRequest("Email already exists.");

            // Check for duplicate username
            if (db.Users.Any(u => u.Username == updatedUser.Username && u.UserId != id))
                return BadRequest("Username already exists.");

            user.Username = updatedUser.Username;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;

            db.SaveChanges();

            return Ok(new
            {
                user.UserId,
                user.Username,
                user.Email,
                user.Role
            });
        }


        [HttpPut]
        [Route("api/users/{id:int}/password")]
        public IHttpActionResult UpdatePassword(int id, [FromUri] string newPassword, [FromUri] string confirmPassword)
        {
            var user = db.Users.Find(id);
            if (user == null) return NotFound();

            if (newPassword != confirmPassword)
                return BadRequest("Passwords do not match.");

            user.PasswordHash = HashPassword(newPassword);
            db.SaveChanges();
            return Ok("Password updated.");
        }

     
        [HttpDelete]
        [Route("api/users/{id:int}")]
        public IHttpActionResult DeleteUser(int id)
        {
            var user = db.Users.Find(id);
            if (user == null) return NotFound();

            db.Users.Remove(user);
            db.SaveChanges();

            return Ok("User deleted.");
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

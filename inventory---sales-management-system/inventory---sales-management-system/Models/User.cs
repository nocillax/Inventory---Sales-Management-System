using inventory___sales_management_system.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        [Required]
        public UserRole Role { get; set; }

        public virtual ICollection<Sale> Sales { get; set; }
    }
}


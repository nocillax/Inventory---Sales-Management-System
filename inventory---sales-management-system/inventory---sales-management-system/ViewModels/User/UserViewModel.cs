using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using inventory___sales_management_system.Enums;


namespace inventory___sales_management_system.ViewModels.User
{
    public class UserViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, ErrorMessage = "Username cannot exceed 100 characters")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public UserRole Role { get; set; }


        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).+$", ErrorMessage = "Password must contain at least one letter and one number.")]
        public string Password { get; set; } 



        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

}


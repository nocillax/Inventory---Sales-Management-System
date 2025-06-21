using ProductModel = inventory___sales_management_system.Models.Product;
using UserModel = inventory___sales_management_system.Models.User;
using CategoryModel = inventory___sales_management_system.Models.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace inventory___sales_management_system.ViewModels.Search
{
    public class GlobalSearchViewModel
    {
        public string Query { get; set; }
        public List<ProductModel> Products { get; set; }
        public List<CategoryModel> Categories { get; set; }
        public List<UserModel> Users { get; set; }
    }

}
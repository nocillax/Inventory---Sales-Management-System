using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace inventory___sales_management_system.ViewModels.Product
{
    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "Product Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public int? CategoryId { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }

    }
}


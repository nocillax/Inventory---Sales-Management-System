using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace inventory___sales_management_system.Models
{
    public class Sale
    {
        public int SaleId { get; set; }

        [Display(Name = "Sale Date")]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Buyer Name")]
        public string BuyerName { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<SaleItem> SaleItems { get; set; }
    }

}
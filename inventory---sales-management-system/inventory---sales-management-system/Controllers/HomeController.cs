using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var role = Session["UserRole"] as string;
            string message;

            if (role == "Manager")
            {
                message = "I am Manager";
            }
            else if (role == "Salesperson")
            {
                message = "I am Salesperson";
            }
            else
            {
                message = "Role not recognized";
            }

            ViewBag.RoleMessage = message;
            return View();
        }
    }


}
using inventory___sales_management_system.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace inventory___sales_management_system.Controllers
{
    public class KeepAliveController : Controller
    {
        [RoleAuthorize("Manager", "Salesperson")]
        [HttpGet]
        [OutputCache(Duration = 0, NoStore = true)]
        public ActionResult Ping()
        {
            // Touches session, extends timeout
            return new EmptyResult();
        }
    }
}
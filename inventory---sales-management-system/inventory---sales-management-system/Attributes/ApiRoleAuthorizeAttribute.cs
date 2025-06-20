using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace inventory___sales_management_system.Attributes
{
    public class ApiRoleAuthorizeAttribute : AuthorizeAttribute
    {
        private readonly string[] _allowedRoles;

        public ApiRoleAuthorizeAttribute(params string[] roles)
        {
            _allowedRoles = roles;
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var userRole = HttpContext.Current?.Session?["UserRole"] as string;

            return userRole != null && _allowedRoles.Contains(userRole);
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            var response = actionContext.Request.CreateResponse(System.Net.HttpStatusCode.Redirect);
            response.Headers.Location = new Uri("/Account/Login", UriKind.Relative);
            actionContext.Response = response;
        }
    }
}
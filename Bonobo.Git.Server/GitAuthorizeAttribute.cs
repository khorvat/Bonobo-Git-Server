using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Security.Principal;
using Bonobo.Git.Server.Security;
using Microsoft.Practices.Unity;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server
{
    public class GitAuthorizeAttribute : AuthorizeAttribute
    {
        [Dependency]
        public IMembershipService MembershipService { get; set; }


        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (IsWindowsUserAuthenticated(filterContext))
                return;

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }
            bool processAuthorization = false;
            string username = null;
            string password = null;
            string auth = filterContext.HttpContext.Request.Headers["Authorization"];

            if (!String.IsNullOrWhiteSpace(UserConfiguration.Current.TrustedHostsCSV) && 
                (String.IsNullOrWhiteSpace(auth) ||
                (!String.IsNullOrWhiteSpace(auth)  && !auth.Contains("Basic"))))
            {
                var config = UserConfiguration.Current.TrustedHosts.FirstOrDefault(p => p.Contains(filterContext.HttpContext.Request.UserHostName ?? filterContext.HttpContext.Request.UserHostName));
                if (!String.IsNullOrWhiteSpace(config))
                {
                    auth = config.Split('=').Skip(1).First();
                    if (!String.IsNullOrEmpty(auth))
                    {
                        username = auth.Substring(0, auth.IndexOf(':'));
                        password = auth.Substring(auth.IndexOf(':') + 1);
                        processAuthorization = true;
                    }
                }
            }
            if (!String.IsNullOrWhiteSpace(auth) && auth.Contains("Basic"))
            {
                byte[] encodedDataAsBytes = Convert.FromBase64String(auth.Replace("Basic ", ""));
                string value = Encoding.ASCII.GetString(encodedDataAsBytes);
                username = value.Substring(0, value.IndexOf(':'));
                password = value.Substring(value.IndexOf(':') + 1);
                processAuthorization = true;
            }

            if (!String.IsNullOrWhiteSpace(filterContext.HttpContext.Request.Url.UserInfo) && String.IsNullOrWhiteSpace(auth))
            {
                auth = filterContext.HttpContext.Request.Url.UserInfo;
                if (!String.IsNullOrEmpty(auth))
                {
                    username = auth.Substring(0, auth.IndexOf(':'));
                    password = auth.Substring(auth.IndexOf(':') + 1);
                    processAuthorization = true;
                }
            }

            if (processAuthorization)
            {
                if (!String.IsNullOrEmpty(username) && !String.IsNullOrEmpty(password) && MembershipService.ValidateUser(username, password))
                {
                    filterContext.HttpContext.User = new GenericPrincipal(new GenericIdentity(username), null);
                }
                else
                {
                    filterContext.Result = new HttpStatusCodeResult(401);
                }
            }
        }


        private bool IsWindowsUserAuthenticated(AuthorizationContext context)
        {
            var windowsIdentity = context.HttpContext.User.Identity as WindowsIdentity;
            return windowsIdentity != null && windowsIdentity.IsAuthenticated;
        }
    }
}
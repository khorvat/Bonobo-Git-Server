﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Bonobo.Git.Server.App_GlobalResources;
using Bonobo.Git.Server.Configuration;
using Bonobo.Git.Server.Models;

namespace Bonobo.Git.Server.Controllers
{
    public class SettingsController : Controller
    {
        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index()
        {
            return View(new GlobalSettingsModel
            {
                AllowAnonymousPush = UserConfiguration.Current.AllowAnonymousPush,
                RepositoryPath = UserConfiguration.Current.Repositories,
                AllowAnonymousRegistration = UserConfiguration.Current.AllowAnonymousRegistration,
                AllowUserRepositoryCreation = UserConfiguration.Current.AllowUserRepositoryCreation,
                DefaultLanguage = UserConfiguration.Current.DefaultLanguage,
                TrustedHostsCSV = UserConfiguration.Current.TrustedHostsCSV
            });
        }

        [HttpPost]
        [WebAuthorizeAttribute(Roles = Definitions.Roles.Administrator)]
        public ActionResult Index(GlobalSettingsModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (Directory.Exists(model.RepositoryPath))
                    {
                        UserConfiguration.Current.AllowAnonymousPush = model.AllowAnonymousPush;
                        UserConfiguration.Current.Repositories = model.RepositoryPath;
                        UserConfiguration.Current.AllowAnonymousRegistration = model.AllowAnonymousRegistration;
                        UserConfiguration.Current.AllowUserRepositoryCreation = model.AllowUserRepositoryCreation;
                        UserConfiguration.Current.DefaultLanguage = model.DefaultLanguage;
                        UserConfiguration.Current.TrustedHostsCSV = model.TrustedHostsCSV;
                        UserConfiguration.Current.Save();

                        this.Session["Culture"] = new CultureInfo(model.DefaultLanguage);

                        ViewBag.UpdateSuccess = true;
                    }
                    else
                    {
                        ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathNotExists);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    ModelState.AddModelError("RepositoryPath", Resources.Settings_RepositoryPathUnauthorized);
                }
            }

            return View(model);
        }
    }
}

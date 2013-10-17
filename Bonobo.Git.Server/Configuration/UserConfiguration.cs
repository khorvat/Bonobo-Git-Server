using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Web;
using System.Linq;
using System.Xml.Serialization;
using System.Text;

namespace Bonobo.Git.Server.Configuration
{
    [XmlRootAttribute(ElementName = "Configuration", IsNullable = false)]
    public class UserConfiguration : ConfigurationEntry<UserConfiguration>
    {      
        public bool AllowAnonymousPush { get; set; }
        public string Repositories { get; set; }
        public bool AllowUserRepositoryCreation { get; set; }
        public bool AllowAnonymousRegistration { get; set; }
        public string DefaultLanguage { get; set; }
        private List<string> _trustedHosts = new List<string>();
        public List<string> TrustedHosts
        {
            get
            {
                return _trustedHosts;
            }
            set
            {
                _trustedHosts = value;
            }
        }

        [XmlIgnore]
        public string TrustedHostsCSV
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                TrustedHosts.ForEach(p => sb.AppendFormat("{0},", p));
                return sb.ToString().Trim(',');
            }
            set
            {
                TrustedHosts.Clear();
                if (!String.IsNullOrWhiteSpace(value))
                {
                    value.Split(',').ToList().ForEach(p => TrustedHosts.Add(p));
                }
            }
        }

        public static void Initialize()
        {
            if (IsInitialized())
                return;

            Current.Repositories = Path.IsPathRooted(ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"]) 
                ? ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"] 
                : HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["DefaultRepositoriesDirectory"]);
            Current.Save();
        }


        private static bool IsInitialized()
        {
            return !String.IsNullOrEmpty(Current.Repositories);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MagicMaids
{
    public static class ConfigEnvironment
    {
        public static string GetConnectionString(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return "";
            }

            return ConfigurationManager.ConnectionStrings[$"{GetEnvironmentPrefix()}{key}"].ConnectionString;
        }

        public static string GetConfigValue(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return "";
            }

            return ConfigurationManager.AppSettings[$"{GetEnvironmentPrefix()}{key}"];
        }

		internal static bool AllowAnonymous
		{
			get
			{
				if (GetEnvironmentPrefix() == "local.")
				{
					return true;
				}

				return false;
			}	
		}

		public static string CurrentHost
		{
			get
			{
				return HttpContext.Current?.Request?.Url.AbsoluteUri;
			}
		}

		public static string Environment
		{
			get
			{
				return envPrefix;
			}
		}


        private static string envPrefix = "";
        private static string GetEnvironmentPrefix()
        {
            if (!String.IsNullOrWhiteSpace(envPrefix))
            {
                return envPrefix;
            }

			if (!CurrentHost.ToLower().Contains("localhost") && !CurrentHost.Contains("127.0.0.1"))
            {
				envPrefix = "prod.";
            }
            else
            {
                envPrefix = "local.";
            }

            return envPrefix;
        }
    }
}
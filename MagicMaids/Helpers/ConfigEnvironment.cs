using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MagicMaids
{
    public static class ConfigEnvironment
    {
        public static string GetConnectionString(HttpContext context, string key)
        {
            if (context == null || context.Request == null || string.IsNullOrWhiteSpace(key))
            {
                return "";
            }

            return ConfigurationManager.ConnectionStrings[$"{GetEnvironmentPrefix(context)}{key}"].ConnectionString;
        }

        public static string GetConfigValue(HttpContext context, string key)
        {
            if (context == null || context.Request == null || string.IsNullOrWhiteSpace(key))
            {
                return "";
            }

            return ConfigurationManager.AppSettings[$"{GetEnvironmentPrefix(context)}{key}"];
        }

		internal static bool AllowAnonymous
		{
			get
			{
				if (GetEnvironmentPrefix(HttpContext.Current) == "local.")
				{
					return true;
				}

				return false;
			}	
		}

        private static string envPrefix = "";
        private static string GetEnvironmentPrefix(HttpContext context)
        {
            if (!String.IsNullOrWhiteSpace(envPrefix))
            {
                return envPrefix;
            }

            string url = context.Request.Url.Host;
            if (url.ToLower().Contains("localhost") || url.Contains("127.0.0.1"))
            {
				envPrefix = "local.";
            }
            else
            {
                envPrefix = "prod.";
            }

            return envPrefix;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MagicMaids
{
    public static class ConfigEnvironment
    {
        public static string GetConnectionString()
        {
			if (IsLocal)
			{
				return ConfigurationManager.ConnectionStrings[$"MagicMaidsContext_Dev"].ConnectionString;
			}

			return ConfigurationManager.ConnectionStrings[$"MagicMaidsContext"].ConnectionString;
        }

   //     public static string GetConfigValue(string key)
   //     {
   //         if (string.IsNullOrWhiteSpace(key))
   //         {
   //             return "";
   //         }

			//return ConfigurationManager.AppSettings[$"{ConfigEnvironment.EnvironmentPrefix}{key}"];
        //}

		internal static bool AllowAnonymous
		{
			get
			{
				//return true;		// Willie hierdie sal die Authentication afsit.
				if (IsLocal)
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

		public static bool IsLocal
		{
			get
			{
				if (HttpContext.Current == null)
				{
					return true;
				}

				return HttpContext.Current.Request.IsLocal;
			}
		}

        public static string EnvironmentPrefix
        {
			get
			{
				Boolean isLive =false;
				Boolean.TryParse(ConfigurationManager.AppSettings["IsLive"], out isLive);

				if (isLive)
				{
					return "prod.";
				}	
				
				return "local.";
			}
        }
    }
}
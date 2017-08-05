#region Using
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using MagicMaids.ViewModels;

using NLog;

using Newtonsoft.Json;
#endregion

namespace MagicMaids
{
	public static class Helpers
	{
		#region Methods, Public
		public static void LogFormValidationErrors(Logger logger, ModelStateDictionary modelState, string callingMethod, Object classInstance = null)
		{
			var enableValidationLogging = System.Configuration.ConfigurationManager.AppSettings["EnableLoggingFormValidationErrors"];
			if (String.IsNullOrWhiteSpace(enableValidationLogging))
				return;

			if (enableValidationLogging == "true")
			{
				if (modelState != null && !modelState.IsValid)
				{
					var errorList = new List<JsonFormValidationError>();
					ModelState _internalState = null;
					foreach (var key in modelState.Keys)
					{
						_internalState = modelState[key];
						foreach (var error in _internalState.Errors)
						{
							errorList.Add(new JsonFormValidationError()
							{
								Key = "",
								Message = $"{error.ErrorMessage} [{key}]"
							});
						}
					}

					String _errors = JsonConvert.SerializeObject(errorList);

					LogHelper _logger = new LogHelper(logger);
					_logger.Log(NLog.LogLevel.Warn, "Form Validation Errors: " + _errors.ToString(), callingMethod, null, classInstance);
				}
			}
		}

		public static bool IsDebug(this HtmlHelper htmlhelper)
		{
			if (HttpContext.Current.IsDebuggingEnabled)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Takes string and returns string value with all the alphanumerics stripped out
		/// </summary>
		/// <returns>parsed string</returns>
		/// <param name="input">input string</param>
		public static string DumbParseDigitsOnly(string input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return "";

			return Regex.Replace(input, "[^0-9]", ""); ;
		}
#endregion
	}
}

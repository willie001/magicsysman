#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using MagicMaids.ViewModels;

using NLog;

using Newtonsoft.Json;
using System.Text;
#endregion

namespace MagicMaids
{
	public static class Helpers
	{
		#region Methods, Public
		public static String FormatModelError(String msgStarter, Exception ex)
		{
			if (ex == null)
				return msgStarter;

			String error = string.Empty;
			if (ex.GetType().Equals(typeof(DbEntityValidationException)))
			{
				error = ParseValidationErrors(ex);
			}
			else
			{
				error = ex.Message;
				if (ex.InnerException != null)
					error += $"; {ex.InnerException.Message}";
			}

			return $"{msgStarter} ({error})";
		}

		public static String ParseValidationErrors(Exception ex)
		{
			if (ex == null || !ex.GetType().Equals(typeof(DbEntityValidationException)))
				return string.Empty;

			var valEx = (DbEntityValidationException)ex;
			var results = string.Empty;
			foreach(DbEntityValidationResult _err in valEx.EntityValidationErrors)
			{
				foreach(DbValidationError _item in _err.ValidationErrors)
				{
					if (results.Length > 0)
						results += ", ";

					results += _item.ErrorMessage.Replace(".","") ;
				}

			}

			return $"Validation failed for one or more entities: {results}";
		}

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
								Key = key,
								Message = $"{error.ErrorMessage}"
							});
						}
					}

					StringBuilder _errors = new StringBuilder();
					foreach (JsonFormValidationError _item in errorList)
					{
						if (_errors.Length > 0)
							_errors.Append(", ");

						_errors.Append($"{_item.Key}: {_item.Message}");

					}
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

		public static Boolean IsValidNumeric(String input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return false;

			var i = 0;
			return Int32.TryParse(input, out i);
		}

		public static Boolean IsValidNumericCommaString(String input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return false;

			var i = 0;
			return Int32.TryParse(input.Replace(",", ""), out i);
		}
#endregion
	}
}

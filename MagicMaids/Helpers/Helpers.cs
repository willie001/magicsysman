#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

using MagicMaids.ViewModels;

using NLog;

using System.Text;
using System.Collections;
#endregion

namespace MagicMaids
{
	public static class Helpers
	{
		#region Enums
		public enum CreditCardType
		{
			Visa,
			MasterCard,
			Discover,
			AmericanExpress,
			DinersClub
		}

		public const String cardRegex = "^(?:(?<Visa>4\\d{3})|(?<MasterCard>5[1-5]\\d{2})|(?<Discover>6011)|(?<DinersClub>(?:3[68]\\d{2})|(?:30[0-5]\\d))|(?<AmericanExpress>3[47]\\d{2}))([ -]?)(?(DinersClub)(?:\\d{6}\\1\\d{4})|(?(Amex)(?:\\d{6}\\1\\d{5})|(?:\\d{4}\\1\\d{4}\\1\\d{3})))$";
		#endregion

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

		public static Int32 ToInt32(String input)
		{
			var i = 0;
			if (Int32.TryParse(input, out i))
			{
				return i;
			}
			else
			{
				return 0;
			};
		}

		public static Boolean IsValidNumericCommaString(String input)
		{
			if (String.IsNullOrWhiteSpace(input))
				return false;

			var i = 0;
			return Int32.TryParse(input.Replace(",", ""), out i);
		}

		// *****************************************************************************************
		//  GetCardTestNumber Method
		//
		/// <summary>
		///     TODO: Gets the card test number.
		///     </summary>
		/// <param name="cardType">
		///     Type of the card.</param>
		/// <returns>
		///     System.String.</returns>
		//
		//  According to PayPal, the valid test numbers that should be used
		//  for testing card transactions are:
		//  Credit Card Type              Credit Card Number
		//  American Express              378282246310005
		//  American Express              371449635398431
		//  American Express Corporate    378734493671000
		//  Diners Club                   30569309025904
		//  Diners Club                   38520000023237
		//  Discover                      6011111111111117
		//  Discover                      6011000990139424
		//  MasterCard                    5555555555554444
		//  MasterCard                    5105105105105100
		//  MasterCard                    2221001234567896  
		//  Visa                          4111111111111111
		//  Visa                          4012888888881881
		//  Src: https://www.paypal.com/en_US/vhelp/paypalmanager_help/credit_card_numbers.htm
		//  Credit: Scott Dorman, http://www.geekswithblogs.net/sdorman
		//
		// *****************************************************************************************
		public static String GetCardTestNumber(CreditCardType cardType)
		{
			//Return bogus CC number that passes Luhn and format tests
			switch (cardType)
			{
				case CreditCardType.AmericanExpress:
					{
						return "3782 822463 10005";
					}
				case CreditCardType.Discover:
					{
						return "6011 1111 1111 1117";
					}
				case CreditCardType.MasterCard:
					{
						return "5105 1051 0510 5100";
					}
				case CreditCardType.Visa:
					{
						return "4111 1111 1111 1111";
					}
				default:
					{
						return null;
					}
			}
		}

		// *****************************************************************************************
		//  GetCardTypeFromNumber Method
		//
		/// <summary>
		///     Gets the card type from number.
		///     </summary>
		/// <param name="cardNum">
		///     The card number.</param>
		/// <returns>
		///     Credic Card Type</returns>
		//
		// *****************************************************************************************
		public static CreditCardType? GetCardTypeFromNumber(String cardNum)
		{
			if ((cardNum.StartsWith("34") || cardNum.StartsWith("37")) && (cardNum.Length == 15))
				return CreditCardType.AmericanExpress;

			else if (cardNum.StartsWith("4") && (cardNum.Length == 13 || cardNum.Length == 16 || cardNum.Length == 19))
				return CreditCardType.Visa;

			else if ((cardNum.StartsWith("51") || cardNum.StartsWith("52")
					  || cardNum.StartsWith("53") || cardNum.StartsWith("54")
					  || cardNum.StartsWith("2") || cardNum.StartsWith("55")) && (cardNum.Length == 16))
				return CreditCardType.MasterCard;

			else if ((cardNum.StartsWith("30") || cardNum.StartsWith("36")
					  || cardNum.StartsWith("38") || cardNum.StartsWith("39")) && (cardNum.Length >= 14 && cardNum.Length <= 19))
				return CreditCardType.DinersClub;

			else if ((cardNum.StartsWith("6011") || cardNum.StartsWith("64") || cardNum.StartsWith("65")) && (cardNum.Length >= 16 && cardNum.Length <= 19))
				return CreditCardType.Discover;
			else
			{
				return null;
			}
			
			//Create new instance of Regex comparer with our
			//credit card regex pattern
			//Regex cardTest = new Regex(cardRegex);

			//Compare the supplied card number with the regex
			//pattern and get reference regex named groups
			//GroupCollection gc = cardTest.Match(cardNum).Groups;

			//Compare each card type to the named groups to 
			//determine which card type the number matches
			//if (gc[CreditCardType.AmericanExpress.ToString()].Success)
			//{
			//	return CreditCardType.AmericanExpress;
			//}
			//if (gc[CreditCardType.MasterCard.ToString()].Success)
			//{
			//	return CreditCardType.MasterCard;
			//}
			//if (gc[CreditCardType.Visa.ToString()].Success)
			//{
			//	return CreditCardType.Visa;
			//}
			//if (gc[CreditCardType.Discover.ToString()].Success)
			//{
			//	return CreditCardType.Discover;
			//}
			//Card type is not supported by our system, return null
			//(You can modify this code to support more (or less)
			// card types as it pertains to your application)
			//return null;
		}
#endregion
	}
}


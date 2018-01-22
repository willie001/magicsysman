#region Using
using System;
using System.Globalization;
using MagicMaids.ViewModels;
#endregion

namespace MagicMaids.Formatters
{
	#region BaseFormatter
	public abstract class BaseFormatter : IFormatProvider, ICustomFormatter
	{
		public abstract string Format(string format, object arg, IFormatProvider formatProvider);

		public virtual object GetFormat(Type formatType)
		{
			if (formatType == typeof(ICustomFormatter))
				return this;
			else
				return null;
		}
	}
	#endregion 

	#region CreditCardFormatter
	public class CreditCardNumberFormatter : BaseFormatter
	{
		public enum CreditCardFormatStyles
		{
			Secure,
			Visible
		}
		public override string Format(string format, object arg, IFormatProvider formatProvider)
		{
			return Format(CreditCardFormatStyles.Secure, arg, formatProvider); 
		}

		public string Format(CreditCardFormatStyles format, object arg, IFormatProvider formatProvider)
		{
			// Check whether this is an appropriate callback             
			if (!this.Equals(formatProvider))
				return null;

			if (arg == null)
			{
				return null;
			}

			if (arg is String)
			{
				string[] cardParts = ((string)arg).Split(new Char[] { ',' });
				if (cardParts.Length != 4)
				{
					throw new InvalidCastException("Invalid card details provided - unable to format.");
				}

				// todo check security for displaying of cc
				format = CreditCardFormatStyles.Visible;

				switch (format)
				{
					case CreditCardFormatStyles.Visible:
						return $"{cardParts[0]}-{cardParts[1]}-{cardParts[2]}-{cardParts[3]}";
					default:
						return $"****-****-****-{cardParts[3]}";
				}
			}
			else
			{
				throw new InvalidCastException("Invalid card details provided - unable to format.");
			}
		}
	}
	#endregion

	#region TelephoneFormatter
	public class TelephoneFormatter : BaseFormatter
	{
		public override string Format(string format, object arg, IFormatProvider formatProvider)
		{
			// Check whether this is an appropriate callback             
			if (!this.Equals(formatProvider))
				return null;

			// Set default format specifier             
			if (string.IsNullOrEmpty(format))
				format = "N";

			string numericString = arg.ToString();

			if (format == "N")
			{
				if (numericString.Length <= 4)
					return numericString;
				else if (numericString.Length == 7)
					return numericString.Substring(0, 3) + "-" + numericString.Substring(3, 4);
				else if (numericString.Length == 10)
					return "(" + numericString.Substring(0, 3) + ") " +
						   numericString.Substring(3, 3) + "-" + numericString.Substring(6);
				else
					throw new FormatException(
							  string.Format("'{0}' cannot be used to format {1}.",
											format, arg.ToString()));
			}
			else if (format == "I")
			{
				if (numericString.Length < 10)
					throw new FormatException(string.Format("{0} does not have 10 digits.", arg.ToString()));
				else
					numericString = "+1 " + numericString.Substring(0, 3) + " " + numericString.Substring(3, 3) + " " + numericString.Substring(6);
			}
			else
			{
				throw new FormatException(string.Format("The {0} format specifier is invalid.", format));
			}
			return numericString;
		}
	}
	#endregion
}

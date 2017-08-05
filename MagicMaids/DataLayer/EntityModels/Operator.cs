#region Using
using System;
#endregion

namespace MagicMaids.EntityModels 
{
	public class Cleaner: BaseModel
	{
		#region Property, Public
		public string Title
		{
			get;
			set;
		}

		public string GivenName
		{
			get;
			set;
		}

		public string Surname
		{
			get;
			set;
		}

		public string FullName
		{
			get
			{
				return $"{GivenName} {Surname} ({Title})";
			}
		}

		public string Zone
		{
			get;
			set;
		}

		public string EmailAddress
		{
			get;
			set;
		}

		public Address PhysicalAddress
		{
			get;
			set;
		}

		public Address PostalAddress
		{
			get;
			set;
		}

		public string PhoneNumber
		{
			get;
			set;
		}

		public string MobileNumber
		{
			get;
			set;
		}

		public decimal ManagementFeePercentage
		{
			get;
			set;
		}

		private string _formattedContactNumbers;
		public string FormattedContactNumbers
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_formattedContactNumbers))
				{
					return _formattedContactNumbers;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (!String.IsNullOrWhiteSpace(PhoneNumber))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<span class='fa fa-phone'></span>&nbsp;");
					_output.Append(PhoneNumber);
				}

				if (!String.IsNullOrWhiteSpace(MobileNumber))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<span class='fa fa-mobile'></span>&nbsp;");
					_output.Append(MobileNumber);
				}

				_formattedContactNumbers = _output.ToString();
				return _formattedContactNumbers;
			}
		}

		#endregion
	}
}

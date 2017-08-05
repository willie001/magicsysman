#region Using
using System;
#endregion

namespace MagicMaids.EntityModels
{
	public enum UserAccountTypes
	{
		Cleaner,
		Staff,
		Master,
		Administrators,
		Franchisor
	}

	public class UserAccount : BaseModel
	{
		#region Properties, Public

		public String UserName
		{
			get;
			set;
		}

		public String FullName
		{
			get;
			set;
		}

		public UserAccountTypes AccountType
		{
			get;
			set;
		}

		private string _accountTypeHtml;
		public string AccountTypeHtml
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_accountTypeHtml))
				{
					return _accountTypeHtml;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (AccountType.Equals(UserAccountTypes.Staff))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-primary\">Staff</div>");
				}

				if (AccountType.Equals(UserAccountTypes.Cleaner))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-inverse-light\">Cleaner</div>");
				}

				if (AccountType.Equals(UserAccountTypes.Master))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-warning\">Master</div>");
				}

				if (AccountType.Equals(UserAccountTypes.Franchisor))
				{
					if (_output.Length > 0) _output.Append("&nbsp;");
					_output.Append("<div class=\"label bg-yellow\">Franchisor</div>");
				}

				if (_output.Length == 0) _output.Append("<div class=\"label bg-danger-dark\">Invalid</div>");

				_accountTypeHtml = _output.ToString();
				return _accountTypeHtml;
			}
		}

		#endregion	
	}
}

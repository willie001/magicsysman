#region Using
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using FluentValidation;
using NLog;
#endregion


namespace MagicMaids.EntityModels
{
	[Table("Logs")]
	public class LogEntry
	{
		#region Fields
		#endregion

		#region Properties, Public
		public Int32 Id
		{
			get;
			set;
		}

		[DataType(DataType.Date)]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy hh:mm:ss}")]
		public DateTime LoggedDate
		{
			get;
			set;
		}

		public string Level
		{
			get;
			set;
		}

		public string Message
		{
			get;
			set;
		}

		public string Exception
		{
			get;
			set;
		}

		public string InnerErrorMessage
		{
			get;
			set;
		}

		public string EventContext
		{
			get;
			set;
		}

		public string ObjectContext
		{
			get;
			set;
		}

		public string UserName
		{
			get;
			set;
		}

		public string URL
		{
			get;
			set;
		}

		public string RequestURL
		{
			get;
			set;
		}

		public string ServerAddress
		{
			get;
			set;
		}

		public string RemoteAddress
		{
			get;
			set;
		}

		public string Logger
		{
			get;
			set;
		}

		public string CallSite
		{
			get;
			set;
		}

		public string MvcAction
		{
			get;
			set;
		}

		private string _formattedAddress;
		public string FormattedAddresses
		{
			get
			{
				if (!String.IsNullOrWhiteSpace(_formattedAddress))
				{
					return _formattedAddress;
				}

				System.Text.StringBuilder _output = new System.Text.StringBuilder();

				if (!String.IsNullOrWhiteSpace(URL))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>"); 
					_output.Append(nameof(URL));
					_output.Append(":</b> ");
					_output.Append(URL);
				}

				if (!String.IsNullOrWhiteSpace(ServerAddress))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(ServerAddress));
					_output.Append(":</b> ");
					_output.Append(ServerAddress);
				}

				if (!String.IsNullOrWhiteSpace(RemoteAddress))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(RemoteAddress));
					_output.Append(":</b> ");
					_output.Append(RemoteAddress);
				}

				if (!String.IsNullOrWhiteSpace(RequestURL))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(RequestURL));
					_output.Append(":</b> ");
					_output.Append(RequestURL);
				}

				if (!String.IsNullOrWhiteSpace(Logger))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(Logger));
					_output.Append(":</b> ");
					_output.Append(Logger);
				}

				if (!String.IsNullOrWhiteSpace(CallSite))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(CallSite));
					_output.Append(":</b> ");
					_output.Append(CallSite);
				}

				if (!String.IsNullOrWhiteSpace(MvcAction))
				{
					if (_output.Length > 0) _output.Append("<br/>");
					_output.Append("<b>");
					_output.Append(nameof(MvcAction));
					_output.Append(":</b> ");
					_output.Append(MvcAction);
				}

				_formattedAddress = _output.ToString();
				return _formattedAddress;
			}
		}

		#endregion
	}
}

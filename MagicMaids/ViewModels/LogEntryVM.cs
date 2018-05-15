#region Using
using System;
using System.ComponentModel.DataAnnotations;

using MagicMaids.EntityModels;
using Newtonsoft.Json;
#endregion

namespace MagicMaids
{
	public class LogEntryViewModel
	{
		#region Constructors
		public LogEntryViewModel()
		{
		}

		public LogEntryViewModel(LogEntry _model)
		{
			if (_model == null)
				return;

			this.Id = _model.Id;
			this.LoggedDate = _model.LoggedDate.ToLocalTime().ToString("dd MMM yyyy (HH:mm:ss)");
			this.Level = _model.Level.ToUpper();
			this.Message = _model.Message;
			this.Exception = _model.Exception;
			this.InnerErrorMessage = _model.InnerErrorMessage;
			this.EventContext = _model.EventContext;
			this.ObjectContext  = (String.IsNullOrWhiteSpace(_model.ObjectContext)) ? "" : JsonConvert.DeserializeObject(_model.ObjectContext).ToString();
			this.UserName = _model.UserName;

			FormatMetadata(_model);
		}
		#endregion

		#region Properties, Public
		public Int32 Id
		{
			get;
			private set;
		}

		public String  LoggedDate
		{
			get;
			private set;
		}

		public string Level
		{
			get;
			private set;
		}

		public string Message
		{
			get;
			private set;
		}

		public string Exception
		{
			get;
			private set;
		}

		public string InnerErrorMessage
		{
			get;
			private set;
		}

		public string EventContext
		{
			get;
			private set;
		}

		public string ObjectContext
		{
			get;
			private set;
		}

		public string UserName
		{
			get;
			private set;
		}

		public String FormattedAddresses
		{
			get;
			private set;
		}
		#endregion

		#region Methods, Private
		private void FormatMetadata(LogEntry entityModel)
		{
			if (entityModel == null)
			{
				FormattedAddresses = string.Empty;
				return;
			}

			System.Text.StringBuilder _output = new System.Text.StringBuilder();

			if (!String.IsNullOrWhiteSpace(entityModel.URL))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.URL));
				_output.Append(":</b> ");
				_output.Append(entityModel.URL);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.ServerAddress))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.ServerAddress));
				_output.Append(":</b> ");
				_output.Append(entityModel.ServerAddress);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.RemoteAddress))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.RemoteAddress));
				_output.Append(":</b> ");
				_output.Append(entityModel.RemoteAddress);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.RequestURL))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.RequestURL));
				_output.Append(":</b> ");
				_output.Append(entityModel.RequestURL);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.Logger))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.Logger));
				_output.Append(":</b> ");
				_output.Append(entityModel.Logger);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.CallSite))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.CallSite));
				_output.Append(":</b> ");
				_output.Append(entityModel.CallSite);
			}

			if (!String.IsNullOrWhiteSpace(entityModel.MvcAction))
			{
				if (_output.Length > 0) _output.Append("<br/>");
				_output.Append("<b>");
				_output.Append(nameof(entityModel.MvcAction));
				_output.Append(":</b> ");
				_output.Append(entityModel.MvcAction);
			}

			FormattedAddresses = "<small>" + _output.ToString() + "</small>";
		}
		#endregion 
	}
}

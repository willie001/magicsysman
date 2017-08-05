#region Using
using System;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion

namespace MagicMaids.ViewModels
{
	
	public class InfoViewModel
	{
		#region Constructors
		public InfoViewModel()
		{
            Errors = new List<JsonFormValidationError>();
        }
		public InfoViewModel(String messageText, InfoMsgTypes msgType)
		{
			Message = messageText;
			MsgType = msgType;
            Errors = new List<JsonFormValidationError>();
        }
		public InfoViewModel(String messageText, Exception ex)
		{
			Message = messageText;
			MsgType = InfoMsgTypes.Error;
			Exception = ex;
            Errors = new List<JsonFormValidationError>();
        }
		#endregion

		#region Properties, Public
		public String Message
		{
			get;
			set;
		}

		public Object DataItem
		{
			get;
			set;
		}

		public InfoMsgTypes MsgType
		{
			get;
			set;
		}

        public string MsgCssClass
        {
            get
            {
                switch(MsgType)
                {
                    case InfoMsgTypes.Error:
                        return "alert alert-danger";

                    case InfoMsgTypes.Validation:
                        return "alert bg-warning-light";

                    case InfoMsgTypes.Warning:
                        return "alert alert-warning";

                    case InfoMsgTypes.Info:
                        return "alert alert-info";

                    case InfoMsgTypes.Success:
                        return "alert alert-success";

                    default:
                        return string.Empty;
                }
            }
        }

        public IEnumerable<JsonFormValidationError> Errors
        {
            get;
            set;
        }

        public Exception Exception
		{
			get;
			set;
		}

		public static implicit operator InfoViewModel(ActionResult v)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Methods, Public
		#endregion

	}

    public class JsonFormValidationError
    {
        public string Key { get; set; }
        public string Message { get; set; }
    }
}

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

		[DataType(DataType.Text)]
		public string Level
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string Message
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string Exception
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string InnerErrorMessage
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string EventContext
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string ObjectContext
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string UserName
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string URL
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string RequestURL
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string ServerAddress
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
		public string RemoteAddress
		{
			get;
			set;
		}

		[DataType(DataType.Text)]
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

		[DataType(DataType.Text)]
		public string MvcAction
		{
			get;
			set;
		}
		#endregion 

	}
}

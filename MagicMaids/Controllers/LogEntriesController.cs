#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.EntityModels;
using MagicMaids.DataAccess;

using NLog;
#endregion

namespace MagicMaids.Controllers
{
	//[RoutePrefix("Settings")]
    public class LogEntriesController : BaseController 
    {
		#region Fields
		#endregion

		#region Constructor
		public LogEntriesController(DBLogsContext dbContext): base()
		{
			LogContext = dbContext;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		#endregion

		#region Properties, Protected
		protected DBLogsContext LogContext { get; private set; }
		#endregion

		#region Methods, Public
		//[Route("LogEntries")]
		public ActionResult LogEntries()
		{
			return View("~/Views/Settings/LogEntries.cshtml");
		}

		public ActionResult LogEntry()
		{
			return View("~/Views/Settings/LogEntry.cshtml");
		}
		#endregion

		#region Service Functions
		[HttpGet]
		public JsonResult GetLogEntries()
		{
			List<LogEntry> _data = new List<LogEntry>();

			_data = LogContext.LogEntries
                 .OrderByDescending(x => x.LoggedDate)
                 .ThenBy(x => x.Id)
				 .ToList();

			List<LogEntryViewModel> _vmList = new List<LogEntryViewModel>();
			foreach(LogEntry _item in _data)
			{
				_vmList.Add(new LogEntryViewModel(_item)); 
			}

			return new JsonNetResult() { Data = new { list = _vmList }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpGet]
		public ActionResult GetLogEntry(Int32? Id)
		{
			//https://msdn.microsoft.com/en-us/data/jj574232.aspx
			LogEntry  _entry = null;
			LogEntryViewModel _dataItem = null;
			if (Id == null)
			{
				ModelState.AddModelError(string.Empty, $"No specific Log ID provided.");
				return JsonFormResponse();
			}
			else
			{
				_entry = LogContext.LogEntries 
									  .Where(x => x.Id == Id)
									  .FirstOrDefault();
				if (_entry == null)
				{
					ModelState.AddModelError(string.Empty, $"Log Entry [{Id.ToString()}] not found.  Please try again.");
					return JsonFormResponse();
				}

				_dataItem = new LogEntryViewModel(_entry);
			}

			return new JsonNetResult() { Data = new { item = _dataItem }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost]
		public ActionResult DeleteLogEntry(Int32? id)
		{
			string _objDesc = "Log Entry";

			if (!id.HasValue)
			{
				ModelState.AddModelError(string.Empty, $"Valid {_objDesc.ToLower()} record not found.");
			}

			try
			{
				LogEntry _entry = new LogEntry() { Id = id ?? 0 };
				LogContext.Entry(_entry).State = EntityState.Deleted;
				LogContext.SaveChanges();

				return JsonSuccessResponse($"{_objDesc} deleted successfully", _entry);
			}
			catch(Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeleteAllLogEntries()
		{
			string _objDesc = "Log Entries";

			try
			{
				LogContext.LogEntries.RemoveRange(LogContext.LogEntries);
				LogContext.SaveChanges();

				return JsonSuccessResponse($"{_objDesc} deleted successfully");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(LogManager.GetCurrentClassLogger(), ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}
		#endregion
	}
}

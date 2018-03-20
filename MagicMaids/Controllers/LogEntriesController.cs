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
		public LogEntriesController(): base()
		{
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
		#endregion

		#region Properties, Protected
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
			List<LogEntryViewModel> _vmList = new List<LogEntryViewModel>();
				
			try
			{
				
				//string debug = "";
				//LogHelper.FormatDebugMessage(ref debug, " | 1 ");
				using (var context = new DBLogsContext())
				{
					var _query = context.LogEntries.AsNoTracking()
									.OrderByDescending(x => x.LoggedDate)
									.ThenBy(x => x.Id);
					
					_data = _query
						.Select(x => x)
						.ToList();
				}
				//LogHelper.FormatDebugMessage(ref debug, " | 2 ");

				foreach (LogEntry _item in _data)
				{
					_vmList.Add(new LogEntryViewModel(_item));
				}
				//LogHelper.FormatDebugMessage(ref debug, " | 3 ");
				//LogHelper.LogRaven(nameof(GetLogEntries), debug);
	
			}
			catch(Exception ex)
			{
				LogHelper log = new LogHelper(LogManager.GetCurrentClassLogger());
				log.Log(LogLevel.Error, $"Error loading log entries", nameof(LogEntries), ex, null);
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
				using (var context = new DBLogsContext())
				{
					_entry = context.LogEntries
							  .Where(x => x.Id == Id)
							  .FirstOrDefault();
				}
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
				using (var context = new DBLogsContext())
				{
					context.Entry(_entry).State = EntityState.Deleted;
					context.SaveChanges();
				}

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
				using (var context = new DBLogsContext())
				{
					context.LogEntries.RemoveRange(context.LogEntries);
					context.SaveChanges();
				}
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

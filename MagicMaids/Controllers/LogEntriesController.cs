#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using MagicMaids.EntityModels;
using MagicMaids.DataAccess;

using System.Data;
using MySql.Data.MySqlClient;
using Dapper;
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
				using (DBManager db = new DBManager())
				{
					_data = db.getConnection().GetList<LogEntry>().OrderByDescending(l => l.LoggedDate).ToList();
				}

				foreach (LogEntry _item in _data)
				{
					_vmList.Add(new LogEntryViewModel(_item));
				}
			}
			catch(Exception ex)
			{
				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error loading log entries", nameof(LogEntries), ex, null);
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
				using (DBManager db = new DBManager())
				{
					_entry = db.getConnection().Get<LogEntry>(Id);
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
				using (DBManager db = new DBManager())
				{
					db.getConnection().Delete<LogEntry>($"where id > {id}");
				}

				return JsonSuccessResponse($"{_objDesc} deleted successfully", "Log Id = " + id);
			}
			catch(Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}

		[HttpPost]
		public ActionResult DeleteAllLogEntries()
		{
			string _objDesc = "Log Entries";

			try
			{
				using (DBManager db = new DBManager())
				{
					db.getConnection().DeleteList<LogEntry>("where id > 0");
				}

				return JsonSuccessResponse($"{_objDesc} deleted successfully");
			}
			catch (Exception ex)
			{
				ModelState.AddModelError(string.Empty, $"Error deleting {_objDesc.ToLower()} ({ex.Message})");

				LogHelper log = new LogHelper();
				log.Log(LogHelper.LogLevels.Error, $"Error deleting {_objDesc.ToLower()}", nameof(LogEntry), ex, null);
			}

			if (!ModelState.IsValid)
			{
				Helpers.LogFormValidationErrors(ModelState, nameof(LogEntry), null);
			}

			return JsonFormResponse();
		}
		#endregion
	}
}

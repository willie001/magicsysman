﻿#region Using
using System;
using System.Text;
using System.Web.Mvc;

using MagicMaids.DataAccess;

using Newtonsoft.Json;

using Dapper;

using System.Linq;
using System.Data.Odbc;
#endregion

namespace MagicMaids.Controllers
{
	public class PagesController : Controller
    {
		#region Methods, Public

		[OutputCache(NoStore = true, Duration = 60, VaryByParam = "*")]
		[AllowAnonymous]
		public ActionResult ConnValidator()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented
			};

			if (HttpContext.Request.Cookies.AllKeys.Contains("timezoneoffset"))
			{
				Session["timezoneoffset"] = HttpContext.Request.Cookies["timezoneoffset"].Value;
			}
			if (HttpContext.Request.Cookies.AllKeys.Contains("timezonename"))
			{
				Session["timezonename"] = HttpContext.Request.Cookies["timezonename"].Value;
			}

			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			try
			{
				stopwatch.Start();
				StringBuilder output = new StringBuilder();

				using (DBManager db = new DBManager())
				{

					var connstring = DBManager.getConnectionStringDisplay();
					TempData["connstring"] = connstring;
					TempData["host"] = ConfigEnvironment.CurrentHost;;
					TempData["environment"] = ConfigEnvironment.EnvironmentPrefix;
					TempData["Anonymous"] = ConfigEnvironment.AllowAnonymous.ToString();
					TempData["IsLocal"] = ConfigEnvironment.IsLocal.ToString();
					TempData["requestdata"] = ConfigEnvironment.CurrentHost;

					string stm = @"SELECT IFNULL(usr,'All Users') user,IFNULL(hst,'All Hosts') host,COUNT(1) Connections 
							FROM
							(
								SELECT user usr, LEFT(host, LOCATE(':', host) - 1) hst
								FROM information_schema.processlist
								WHERE user NOT IN('system user', 'root')
							) A
							WHERE hst = 'localhost'
						 	GROUP BY usr,hst WITH ROLLUP";

					var rows = db.getConnection().Query(stm).ToList();
				
					string _connCounter = (rows.Count > 0) ? rows[0].Connections.ToString() : "0";
				
					output.Append($"Open Connections : {_connCounter}\n");

					output.Append("\n\n");
					output.Append(CultureHelper.DisplayCultureSettings("\n"));
					output.Append("\n\n");

					stm = "SELECT VERSION() as version";
					rows = db.getConnection().Query(stm).ToList();
					string version = rows[0].version.ToString();
					output.Append($"MySQL version : {version}\n");
				
					stm = "SELECT count(*) as testCount from systemsettings";
					rows = db.getConnection().Query(stm).ToList();
					string counter = rows[0].testCount.ToString();
					output.Append($"Record Count : {counter}\n\n");
					output.Append(RenderTestWeeks());

					TempData["results"] = output.ToString();
				}

			}
			catch(OdbcException exception)
			{
				string json = JsonConvert.SerializeObject(ParseOdbcErrorCollection(exception), settings);
				TempData["results"] = json;
			}
			catch (Exception ex)
			{
				string json = JsonConvert.SerializeObject(ex, settings);
				TempData["results"] = json;
			}
			finally
			{
				if (stopwatch != null && stopwatch.IsRunning)
				{
					stopwatch.Stop();
				}

				TempData["timer"] = stopwatch.ElapsedMilliseconds.ToString() + " milliseconds";
			}

			TempData["debugger"] = "-----------------------\n";

			return View();
		}

		private string RenderTestWeeks()
		{
			StringBuilder _weeks = new StringBuilder();
			var _year = DateTime.Now.Year;

			_weeks.Append($"---------- {_year} -------------\n");
			_weeks.Append(RenderTestWeeks(_year, 1, 1));
			_weeks.Append("\n");
			_weeks.Append(RenderTestWeeks(_year, 12, 22));
			_weeks.Append($"---------- {_year+1} -------------\n");
			_weeks.Append(RenderTestWeeks(_year+1, 1, 1));
			_weeks.Append("\n");
			_weeks.Append(RenderTestWeeks(_year+1, 12, 22));
			_weeks.Append("-----------------------\n");
			return _weeks.ToString();
		}

		private string RenderTestWeeks(int year, int month, int day)
		{
			StringBuilder _weeks = new StringBuilder();

			var _hitMonday = false;
			var _expected = NamedColours.WeeksOdd;
			for (int i = day; i <= day+9; i++)
			{
				var _test = new DateTime(year, month, i);
				var _day = _test.DayOfWeek;
				if (_day == DayOfWeek.Monday && i > 1)
				{
					_expected = (_expected == NamedColours.WeeksEven) ? NamedColours.WeeksOdd : NamedColours.WeeksEven;
					_hitMonday = !_hitMonday ;
				}

				_weeks.Append($"{_test.ToShortDateString()} [{_test.DayOfWeek.ToString()}] expected to be '{_expected}'\n");
			}
			return _weeks.ToString();
		}

		private string ParseOdbcErrorCollection(OdbcException exception)
		{
			StringBuilder error = new StringBuilder();
			for (int i = 0; i < exception.Errors.Count; i++)
			{
				error.Append("Index #" + i + "\n" +
					   "Message: " + exception.Errors[i].Message + "\n" +
					   "Native: " + exception.Errors[i].NativeError.ToString() + "\n" +
					   "Source: " + exception.Errors[i].Source + "\n" +
					   "SQL: " + exception.Errors[i].SQLState + "\n");
			}
			return error.ToString();
		}

		[AllowAnonymous]
		public ActionResult Error404(string path)
		{
			Response.StatusCode = 404;

			ViewBag.Path = path;

			return View();
		}

		[AllowAnonymous]
		public ActionResult Error500(string path)
		{
			Response.StatusCode = 500;

			ViewBag.Path = path;

			return View();
		}

		[AllowAnonymous]
		public ActionResult Error(int? errorCode, string path)
		{
			Response.StatusCode = errorCode ?? 0;

			String _view = string.Empty;

			if (ViewData.Model != null && ViewData.Model is System.Web.Mvc.HandleErrorInfo)
			{
				var container = ((HandleErrorInfo)ViewData.Model);
				var ex = container.Exception;
				if (ex != null)
				{
					ViewBag.Message = ex.Message;
				}

			}
			ViewBag.Code = errorCode;
			ViewBag.Path = path;

			switch(errorCode)
			{
				case 404:
					_view = "Error404";
					break;
				case 500:
					_view = "Error500";
					break;
				default:
					_view = "Error";
					break;
			}

			return View(_view);
		}

		#endregion
    }
}

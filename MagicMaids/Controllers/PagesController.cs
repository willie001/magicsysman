#region Using
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

using MagicMaids.DataAccess;
using MagicMaids.EntityModels;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NLog;
#endregion

namespace MagicMaids.Controllers
{
    public class PagesController : Controller
    {
		#region Methods, Public

		[OutputCache(NoStore = true, Duration = 60, VaryByParam = "*")]
		public ActionResult ConnValidator()
		{
			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented
			};

			MagicMaidsInitialiser.CheckConnection();
			var connstring = ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			MySqlConnection connection = null;
			//try
			//{
				stopwatch.Start();

				connection = new MySqlConnection(connstring);
				connection.Ping();
				StringBuilder output = new StringBuilder();

				connection.Open();

				string stm = "SELECT VERSION()";
				MySqlCommand cmd = new MySqlCommand(stm, connection);
				string version = Convert.ToString(cmd.ExecuteScalar());
				output.Append($"MySQL version : {version.ToString()}\n");

				stm = "SELECT count(*) from systemsettings";
				cmd = new MySqlCommand(stm, connection);
				string counter = Convert.ToString(cmd.ExecuteScalar());
				output.Append($"Record Count : {counter.ToString()}\n");
				TempData["results"] = output.ToString();

				connection.Close();

			//}
			//catch (Exception ex)
			//{
			//	string json = JsonConvert.SerializeObject(ex, settings);
			//	TempData["results"] = json;

			//	LogHelper.LogRaven($"Error loading Connection Validator", nameof(ConnValidator), ex, null, null);

			//}
			//finally
			//{
			//	if (connection != null && connection.State == ConnectionState.Open)
			//	{
			//		connection.Close();
			//	}

			//	if (stopwatch != null && stopwatch.IsRunning)
			//	{
			//		stopwatch.Stop();
			//	}

			//	TempData["timer"] = stopwatch.ElapsedMilliseconds.ToString() + " milliseconds";
			//}


			return View();
		}


		public ActionResult Error404(string path)
		{
			Response.StatusCode = 404;

			ViewBag.Path = path;

			return View();
		}

		public ActionResult Error500(string path)
		{
			Response.StatusCode = 500;

			ViewBag.Path = path;

			return View();
		}

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

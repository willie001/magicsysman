#region Using
using System;
using System.Data;
using System.Text;
using System.Web.Mvc;

using MagicMaids.DataAccess;

using Newtonsoft.Json;

using Dapper;

using MySql.Data.MySqlClient;
using System.Linq;
using System.Threading.Tasks;
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

			var connstring = MagicMaidsInitialiser.getConnectionString();
			TempData["connstring"] = connstring;
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			MySqlConnection connection = null;
			try
			{
				stopwatch.Start();
				StringBuilder output = new StringBuilder();

				using (IDbConnection db = MagicMaidsInitialiser.getConnection())
				{
					string stm = "SELECT VERSION() as version";
					var rows = db.Query(stm).ToList();
					string version = rows[0].version.ToString();
					output.Append($"MySQL version : {version.ToString()}\n");

					stm = "SELECT count(*) as testCount from systemsettings";
					rows = db.Query(stm).ToList();
					string counter = rows[0].testCount.ToString();
					output.Append($"Record Count : {counter.ToString()}\n");
					TempData["results"] = output.ToString();
				}

			}
			catch (Exception ex)
			{
				string json = JsonConvert.SerializeObject(ex, settings);
				TempData["results"] = json;

				//var result = Task.Run(() => {
					LogHelper.LogRaven($"Error loading Connection Validator", nameof(ConnValidator), ex, null, null);
				//});
			}
			finally
			{
				if (connection != null && connection.State == ConnectionState.Open)
				{
					connection.Close();
				}

				if (stopwatch != null && stopwatch.IsRunning)
				{
					stopwatch.Stop();
				}

				TempData["timer"] = stopwatch.ElapsedMilliseconds.ToString() + " milliseconds";
			}


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

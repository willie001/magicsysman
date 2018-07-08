using System;
using System.Configuration;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Dapper;
using MagicMaids.DataAccess;
using Newtonsoft.Json;

namespace MagicMaids
{
	public enum InfoMsgTypes
	{
		Info,
		Success,
		Warning,
		Validation,
		Error
	}

	public class LogHelper
	{
		public enum LogLevels
		{
			Info,
			Warning,
			Debug,
			Error
		};

		public LogHelper()
		{
		}

		public static void LogDebugDetails(String callingMethod, params string[] args)
		{
			Boolean enableDebugInfo = true;

			Boolean.TryParse(ConfigurationManager.AppSettings["EnableAdditionalDebugInfo"], out enableDebugInfo);

			if (!enableDebugInfo)
			{
				return;
			}

			LogHelper _logger = new LogHelper();
			_logger.Log(LogLevels.Debug, $"{callingMethod} - Debug Information", callingMethod, null, args, null);
		}

		public static void FormatDebugMessage(ref string messageToDate, string newMessage)
		{
			if (!String.IsNullOrWhiteSpace(newMessage))
			{
				messageToDate += $" {newMessage.Trim()} ({DateTimeWrapper.NowUtc.FormatDatabaseDateTime()})";
			}
		}

		public void Log(LogLevels logLevel, String customMessage, String callingMethod, Exception ex = null, Object classInstance = null, String validationErrors = null)
		{
			String _logDate = "";
			StringBuilder _sql = new StringBuilder();

			Boolean enableExternalErrorLogging = true;
			Boolean.TryParse(ConfigurationManager.AppSettings["EnableExternalExceptionLogging"], out enableExternalErrorLogging);

			try
			{
				_logDate = DateTimeWrapper.NowUtc.FormatDatabaseDateTime();
			}
			catch
			{
				_logDate = DateTime.Now.ToUniversalTime().FormatDatabaseDateTime();
				customMessage += " (*)";
			}

			try
			{
				if (logLevel == LogLevels.Error && enableExternalErrorLogging)
				{
					Bugsnag.AspNet.Client.Current.Notify(ex);
				}

				var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

				string _currentUser = "Not Set";
				string _url = "";
				string _server = "";
				string _remote = "";
				string _mvcAction = "";
				string _requestUrl = "";
				string _logger = version ?? "";
				string _callSite = callingMethod;
				string _event = "";
				string _innerError = "";
				string _error = "";
				string _object = "";
				//<parameter name="@Url" layout="${aspnet-request:serverVariable=HTTP_URL}" />
				//< parameter name = "@ServerAddress" layout = "${aspnet-request:serverVariable=LOCAL_ADDR}" />
				//< parameter name = "@RemoteAddress" layout = "${aspnet-request:serverVariable=REMOTE_ADDR}:${aspnet-request:serverVariable=REMOTE_PORT}" />
				//< parameter name = "@MvcAction" layout = "${aspnet-MVC-Action}" />

				if (HttpContext.Current != null)
				{
					_currentUser = HttpContext.Current.User?.Identity?.Name;
					_requestUrl = HttpContext.Current.Request.RawUrl;
					_url = HttpContext.Current.Request.Url.ToString();
					_remote = HttpContext.Current.Request.UrlReferrer.ToString();
				}

				if (ex != null)
				{
					_error = GetObjectData(ex) + "\n";

					if (ex.InnerException != null)
					{
						_innerError = GetObjectData(ex.InnerException);
					}

					RequestWrapper _request = new RequestWrapper(HttpContext.Current.Request);
					_event = GetObjectData(_request);
				}
				if (!String.IsNullOrWhiteSpace(validationErrors))
				{
					_error += $"VALIDATION ERRORS:\n{validationErrors}";
				}

				_mvcAction = GetObjectData(HttpContext.Current.Request.RequestContext.RouteData.Values.Values);

				if (classInstance != null)
				{
					_object = GetObjectData(classInstance);
				}

				_sql.Append(@"insert into Logs (
			      LoggedDate, Level, Message, UserName,
			      URL, ServerAddress, RemoteAddress,
				  RequestUrl, MvcAction,
			      Logger, CallSite, 
			      EventContext, InnerErrorMessage,
				  Exception, ObjectContext
			    ) values (");
				_sql.Append($"'{_logDate}',");
				_sql.Append($"'{logLevel.ToString()}',");
				_sql.Append($"'{customMessage.Replace("'", "`")}',");
				_sql.Append($"'{_currentUser}',");
				_sql.Append($"'{_url}',");
				_sql.Append($"'{_server}',");
				_sql.Append($"'{_remote}',");
				_sql.Append($"'{_requestUrl}',");
				_sql.Append($"'{_mvcAction}',");
				_sql.Append($"'{_logger}',");
				_sql.Append($"'{_callSite}',");
				_sql.Append($"'{_event.Replace("'", "`")}',");
				_sql.Append($"'{_innerError.Replace("'", "`")}',");
				_sql.Append($"'{_error.Replace("'", "`")}',");
				_sql.Append($"'{_object.Replace("'", "`")}')");
				using (DBManager db = new DBManager())
				{
					db.getConnection().Execute(_sql.ToString());
				}
			}
			catch(Exception bigEx)
			{
				if (enableExternalErrorLogging)
				{
					Bugsnag.AspNet.Client.Current.Notify(bigEx);
				}
			}
		}

		public static string GetObjectData(object instanceClass)
		{
			if (instanceClass == null)
				return String.Empty;

			JsonSerializerSettings settings = new JsonSerializerSettings
			{
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				Formatting = Formatting.Indented, 
			 	NullValueHandling = NullValueHandling.Ignore
			};

			return JsonConvert.SerializeObject(instanceClass, settings);
		}
	}
}

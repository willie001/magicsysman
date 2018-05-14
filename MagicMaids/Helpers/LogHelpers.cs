using System;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using Dapper;
using MagicMaids.DataAccess;
using Newtonsoft.Json;
using SharpRaven;
using SharpRaven.Data;

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

		private static RavenClient LogClient
		{
			get
			{
				if (ravenClient == null)
				{
					ravenClient = new RavenClient("https://ef70d575026049b4a56618bf643e5a38:acc6f63b39614a5fbb24e021b0d2e6ec@sentry.io/306347");
				}
				return ravenClient;
			}
		}
		private static RavenClient ravenClient;

		public static void FormatDebugMessage(ref string messageToDate, string newMessage)
		{
			if (!String.IsNullOrWhiteSpace(newMessage))
			{
				messageToDate += $" {newMessage.Trim()} ({DateTime.Now.ToString("HH:mm:ss:ffff")})";
			}
		}

		public static async Task LogRaven(String callingMethod, String customMessage)
		{
			var isLocal = (HttpContext.Current == null) ? false : HttpContext.Current.Request.IsLocal;
			if (isLocal)
				return;
			
			System.Text.StringBuilder message = new System.Text.StringBuilder();

			if (!String.IsNullOrWhiteSpace(callingMethod))
			{
				message.Append($"Calling Method: {callingMethod} " + Environment.NewLine);
			}

			if (!String.IsNullOrWhiteSpace(customMessage))
			{
				message.Append($"Custom Message: {customMessage} " + Environment.NewLine);
			}

			await LogClient.CaptureAsync(new SentryEvent(message.ToString()));
		}

		public static async Task LogRaven(String customMessage, String callingMethod, Exception ex = null, Object classInstance = null, String validationErrors = null)
		{
			return;
			var isLocal = (HttpContext.Current == null) ? false : HttpContext.Current.Request.IsLocal;
			if (isLocal)
				return;
			//else
			//	LogRaven("TESTING", "THIS IS A TEST");
			
			if (ex != null)
			{
				if (!String.IsNullOrWhiteSpace(customMessage))
				{
					ex.Data.Add("Custom Message", customMessage);
				}

				if (!String.IsNullOrWhiteSpace(callingMethod))
				{
					ex.Data.Add("Calling Method", callingMethod);
				}

				if (classInstance != null)
				{
					ex.Data.Add("Class Instance", classInstance);
				}

				if (validationErrors != null)
				{
					ex.Data.Add("Validation Errors", validationErrors);
				}

				await LogClient.CaptureAsync(new SharpRaven.Data.SentryEvent(ex));

			}
			else
			{
				System.Text.StringBuilder message = new System.Text.StringBuilder();
				if (!String.IsNullOrWhiteSpace(customMessage))
				{
					message.Append($"Custom Message: {customMessage} <br/> ");
				}

				if (!String.IsNullOrWhiteSpace(callingMethod))
				{
					message.Append($"Calling Method: {callingMethod}  <br/> ");
				}

				if (classInstance != null)
				{
					message.Append($"Class Instance: {GetObjectData(classInstance)}  <br/> ");
				}

				if (validationErrors != null)
				{
					message.Append($"Validation Errors: {validationErrors}  <br/> ");
				}

				await LogClient.CaptureAsync(new SentryEvent(message.ToString()));
			}
		}

		public void Log(LogLevels logLevel, String customMessage, String callingMethod, Exception ex = null, Object classInstance = null, String validationErrors = null)
		{
			try
			{
				var result = Task.Run(() => {
					return LogRaven(customMessage, callingMethod, ex, classInstance, validationErrors);
				});

				string _currentUser = "Not Set";
				string _url = "";
				string _server = "";
				string _remote = "";
				string _mvcAction = "";
				string _requestUrl = "";
				string _logger = "";
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
					_error = GetObjectData(ex);
					if (ex.InnerException != null)
					{
						_innerError = GetObjectData(ex.InnerException);
					}

					RequestWrapper _request = new RequestWrapper(HttpContext.Current.Request);
					_event = GetObjectData(_request);
				}

				_mvcAction = GetObjectData(HttpContext.Current.Request.RequestContext.RouteData.Values.Values);

				if (classInstance != null)
				{
					_object = GetObjectData(classInstance);
					if (!String.IsNullOrWhiteSpace(validationErrors))
						_object += $"; {validationErrors}";
				}

				StringBuilder _sql = new StringBuilder();
				_sql.Append(@"insert into Logs (
			      LoggedDate, Level, Message, UserName,
			      URL, ServerAddress, RemoteAddress,
				  RequestUrl, MvcAction,
			      Logger, CallSite, 
			      EventContext, InnerErrorMessage,
				  Exception, ObjectContext
			    ) values (");
				_sql.Append($"'{DateTimeWrapper.FormatDateTimeForDatabase(DateTimeWrapper.Now.ToDateTimeUtc())}',");
				_sql.Append($"'{logLevel.ToString()}',");
				_sql.Append($"'{customMessage}',");
				_sql.Append($"'{_currentUser}',");
				_sql.Append($"'{_url}',");
				_sql.Append($"'{_server}',");
				_sql.Append($"'{_remote}',");
				_sql.Append($"'{_requestUrl}',");
				_sql.Append($"'{_mvcAction}',");
				_sql.Append($"'{_logger}',");
				_sql.Append($"'{_callSite}',");
				_sql.Append($"'{_event}',");
				_sql.Append($"'{_innerError}',");
				_sql.Append($"'{_error}',");
				_sql.Append($"'{_object}')");
				using (DBManager db = new DBManager())
				{
					db.getConnection().Execute(_sql.ToString());
				}
			}
			catch(Exception bigEx)
			{
				HttpContext.Current.Response.Write(bigEx.Message);
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

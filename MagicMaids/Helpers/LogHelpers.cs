using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
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
		private Logger internalLogger;

		public LogHelper(Logger logger)
		{
			internalLogger = logger;
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

		public void Log(LogLevel logLevel, String customMessage, String callingMethod, Exception ex = null, Object classInstance = null, String validationErrors = null)
		{
			LogRaven(customMessage, callingMethod, ex, classInstance, validationErrors);

			if (internalLogger == null)
				internalLogger = LogManager.GetCurrentClassLogger();
			
			LogEventInfo eventInfo = new LogEventInfo(logLevel, internalLogger.Name, customMessage);
			if (ex != null)
				eventInfo.Exception = ex;

			if (classInstance != null)
			{
				eventInfo.Properties["ObjectContextData"] = LogHelper.GetObjectData(classInstance);
				if (!String.IsNullOrWhiteSpace(validationErrors))
					eventInfo.Properties["ObjectContextData"] += $"; {validationErrors}";
			}

			eventInfo.Properties["CustomCallSite"] = callingMethod;

			internalLogger.Log(eventInfo);
		}

		public static string GetObjectData(object instanceClass)
		{
			if (instanceClass == null)
				return String.Empty;

			return JsonConvert.SerializeObject(instanceClass);
		}


	}
}

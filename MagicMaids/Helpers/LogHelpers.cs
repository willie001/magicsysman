using System;
using Newtonsoft.Json;
using NLog;

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

		public void Log(LogLevel logLevel, String customMessage, String callingMethod, Exception ex = null, Object classInstance = null)
		{
			if (internalLogger == null)
				internalLogger = LogManager.GetCurrentClassLogger();
			
			LogEventInfo eventInfo = new LogEventInfo(logLevel, internalLogger.Name, customMessage);
			if (ex != null)
				eventInfo.Exception = ex;

			if (classInstance != null)
			{
				eventInfo.Properties["ObjectContextData"] = LogHelper.GetObjectData(classInstance);
			}
			eventInfo.Properties["CustomCallSite"] = callingMethod;

			internalLogger.Log(eventInfo);
		}

		public static string GetObjectData(object instanceClass)
		{
			if (instanceClass == null)
				return "";

			return JsonConvert.SerializeObject(instanceClass);
		}


	}
}

#region Using
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MagicMaids.EntityModels;
using MySql.Data.MySqlClient;
using NLog;
#endregion

namespace MagicMaids.DataAccess
{
	// Not currently up to spec
    public class MagicMaidsInitialiser : CreateDatabaseIfNotExists<MagicMaidsContext>
    {
   //     protected override void Seed(MagicMaidsContext context)
   //     {
			//if (!context.DefaultSettings.Any())
			//{
			//	try
			//	{
			//		AddData(context);
			//		context.SaveChanges();
			//		context.Dispose();
			//	}
			//	catch (Exception ex)
			//	{
			//		var logger = LogManager.GetCurrentClassLogger();
			//		logger.Log(LogLevel.Warn, ex, "Error in database initialisation");
			//	}
			//}
        //}

        //private void AddData(MagicMaidsContext context)
        //{
        //    var systemSettings = new List<SystemSetting>
        //    {
        //        new SystemSetting
        //        {
        //            SettingName = "Management Fee (%)",
        //            SettingValue = "4.5"
        //        }
        //    };

        //    context.DefaultSettings.AddRange(systemSettings);
        //}

		public static string getConnection()
		{
			return ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
		}

		private static Boolean HasPinged
		{
			get;
			set;
		}

		public static void CheckConnection(MySqlConnection connection)
		{
			try
			{
				if (!connection.Ping())
				{
					connection.Open();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.InnerException.ToString());
				LogHelper _logger = new LogHelper(LogManager.GetCurrentClassLogger());
				_logger.Log(LogLevel.Warn, "Database connection not valid!!!: " + ex.Message, nameof(CheckConnection), ex, null);

			}
		}

    }
}

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
    class MagicMaidsInitialiser : CreateDatabaseIfNotExists<MagicMaidsContext>
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

		private static Boolean HasPinged
		{
			get;
			set;
		}

		public static void CheckConnection()
		{
//			using (var context = new MagicMaidsContext())
//			{
//				context.Database.CommandTimeout = 180;
//				MySqlConnection conn = (MySqlConnection)context.Database.Connection;
//				if (conn == null)
//				{
//					var connString = ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
//					if (String.IsNullOrWhiteSpace(connString))
//					{
//						return;
//					}

//					conn = new MySqlConnection(connString);
//				}

//				try
//				{
//7					if (!HasPinged && !conn.Ping())
			//		{
			//			conn.Open();
			//			HasPinged = true;
			//		}
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.WriteLine(ex.InnerException.ToString());
			//		LogHelper _logger = new LogHelper(LogManager.GetCurrentClassLogger());
			//		_logger.Log(LogLevel.Warn, "Database connection not valid!!!: " + ex.Message, nameof(CheckConnection), ex, null);

			//	}
			//}
		}

    }
}

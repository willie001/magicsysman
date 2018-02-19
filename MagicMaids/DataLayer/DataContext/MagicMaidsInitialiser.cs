#region Using
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;

using MagicMaids.EntityModels;
using MySql.Data.MySqlClient;
using NLog;
#endregion

namespace MagicMaids.DataAccess
{
	// Not currently up to spec
    class MagicMaidsInitialiser : CreateDatabaseIfNotExists<MagicMaidsContext>
    {
        protected override void Seed(MagicMaidsContext context)
        {
            if (!context.DefaultSettings.Any())
            {
                try
                {
                    AddData(context);
                    context.SaveChanges();
                }
                catch (Exception ex)
                {
					var logger = LogManager.GetCurrentClassLogger();
					logger.Log(LogLevel.Warn, ex, "Error in database initialisation");
                }
            }
        }

        private void AddData(MagicMaidsContext context)
        {
            var systemSettings = new List<SystemSetting>
            {
                new SystemSetting
                {
                    SettingName = "Management Fee (%)",
                    SettingValue = "4.5"
                }
            };

            context.DefaultSettings.AddRange(systemSettings);
            context.SaveChanges();
        }

		public static void CheckConnection(DbContext context)
		{
			if (context == null)
			{
				return ;
			}

			context.Database.CommandTimeout = 180;
			MySqlConnection conn = (MySqlConnection)context.Database.Connection;
			if (conn == null)
			{
				var connString = ConfigurationManager.ConnectionStrings["MagicMaidsDBConn"].ConnectionString;
				if (String.IsNullOrWhiteSpace(connString))
				{
					return ;
				}

				conn = new MySqlConnection(connString);
			}

			try
			{
				if (!conn.Ping())
				{
					conn.Open();
				}
			}
			catch(Exception ex)
			{
				LogHelper _logger = new LogHelper(LogManager.GetCurrentClassLogger());
				_logger.Log(LogLevel.Info, "Database connection not valid!!!: " + ex.Message, nameof(CheckConnection), ex, null);

			}
		}

    }
}

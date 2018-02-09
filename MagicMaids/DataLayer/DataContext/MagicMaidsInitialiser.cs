#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

using MagicMaids.EntityModels;

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

		public static bool CheckConnection(DbContext context)
		{
			try
			{
				context.Database.Connection.Open();
				context.Database.Connection.Close();
			}
			catch (SqlException ex)
			{
				LogHelper _logger = new LogHelper(LogManager.GetCurrentClassLogger());
				_logger.Log(NLog.LogLevel.Fatal, "Database connection not valid!!!: " + ex.Message, nameof(CheckConnection), ex, null);
				return false;
			}
			return true;
		}
    }
}

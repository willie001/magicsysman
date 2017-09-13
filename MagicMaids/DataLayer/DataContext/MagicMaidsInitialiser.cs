#region Using
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    }
}

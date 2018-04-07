﻿#region Using
using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;

using MagicMaids.EntityModels;
using MySql.Data.MySqlClient;
using NLog;

#endregion

namespace MagicMaids.DataAccess 
{
	[DbConfigurationType(typeof(MySql.Data.Entity.MySqlEFConfiguration))]
	public class DBLogsContext : DbContext
	{
		#region Constructors
		public DBLogsContext(): base(nameOrConnectionString: MagicMaidsInitialiser.getConnection())
		{
			((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;

			this.Configuration.LazyLoadingEnabled = false;

			LogEntries = Set<LogEntry>();

			Log = LogManager.GetLogger(GetType().FullName);
		}
		#endregion

		#region Properties, Protected
		protected Logger Log { get; private set; }

		public DbSet<LogEntry> LogEntries
		{
			get;
			set;
		}
		#endregion

		#region Methods, Public

		#endregion
	}
}

#region Using
using System;
using System.Data.Entity;
using System.Diagnostics;

using MagicMaids.EntityModels;
using MySql.Data.Entity;
using NLog;

#endregion

namespace MagicMaids.DataAccess 
{
	[DbConfigurationType(typeof(MySqlEFConfiguration))]
	public class DBLogsContext : DbContext
	{
		#region Constructors
		public DBLogsContext()
				: base(nameOrConnectionString: "MagicMaidsDBConn")
		{
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

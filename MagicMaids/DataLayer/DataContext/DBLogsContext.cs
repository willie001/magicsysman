#region Using
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;

using MagicMaids.EntityModels;
using NLog;

#endregion

namespace MagicMaids.DataAccess 
{
	public class DBLogsContext : DbContext
	{
		#region Constructors
		public DBLogsContext()
			: base("MagicMaidsDBConn")
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

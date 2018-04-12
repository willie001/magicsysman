//#region Using
//using System;
//using System.Configuration;
//using System.Diagnostics;

//using MagicMaids.EntityModels;
//using MySql.Data.MySqlClient;
//using NLog;

//#endregion

//namespace MagicMaids.DataAccess 
//{
//	public class DBLogsContext : DbContext
//	{
//		#region Constructors
//		public DBLogsContext(): base(nameOrConnectionString: MagicMaidsInitialiser.getConnection())
//		{
//			MagicMaidsInitialiser.CheckConnection((MySqlConnection)Database.Connection);
//			((IObjectContextAdapter)this).ObjectContext.CommandTimeout = 180;

//			this.Configuration.LazyLoadingEnabled = false;

//			LogEntries = Set<LogEntry>();

//			Log = LogManager.GetLogger(GetType().FullName);
//		}
//		#endregion

//		#region Properties, Protected
//		protected Logger Log { get; private set; }

//		public DbSet<LogEntry> LogEntries
//		{
//			get;
//			set;
//		}
//		#endregion

//		#region Methods, Public

//		#endregion
//	}
//}

#region Using
using System;
using System.Configuration;
using System.Data.Common;
using System.Data.Odbc;
using System.Web;
using MySql.Data.MySqlClient;
#endregion

namespace MagicMaids.DataAccess
{
	public class DBManager: IDisposable 
	{
		#region Fields
		private string _connectionString = "";
		//private MySqlConnection _connection;
		private DbConnection _connection;

		public string debugInternal = "";
		#endregion

		#region Properties
		private bool IsOdbcConnection
		{
			get
			{
				return (_connectionString.ToLower().Contains("odbc"));
			}
		}

		public bool Connected
		{
			get
			{
				debugInternal += "| g ";
				if (IsOdbcConnection)
				{
					return !(_connection.State == System.Data.ConnectionState.Closed);
				}
				else
				{
					return !(_connection.State == System.Data.ConnectionState.Closed) && ((MySqlConnection)_connection).Ping();
				}
			}
		}
		#endregion 

		#region Constructor
		public DBManager()
		{
			//_connectionString = ConfigurationManager.ConnectionStrings[$"MagicMaidsContext"].ConnectionString;
			_connectionString = ConfigEnvironment.GetConnectionString();
        }
		#endregion

		#region Methods
		public void Dispose()
		{
			Dispose(true);
		}

		public void Dispose(bool disposing)
		{

			if (disposing)
			{
				if (_connection != null && _connection.State == System.Data.ConnectionState.Open)
				{
					_connection.Close();
					_connection.Dispose();
					if (!IsOdbcConnection)
					{
						((MySqlConnection)_connection).ClearAllPoolsAsync();
					}
				}
			}
		}

		public DbConnection getConnection()
		{
			//if (_connection != null && _connection.State == System.Data.ConnectionState.Broken)
			//{
			//	_connection.Close();
			//	_connection.Dispose();
			//	if (!IsOdbcConnection)
			//	{
			//		((MySqlConnection)_connection).ClearPoolAsync((MySqlConnection)_connection);
			//	}
			//}

			Open();
			while(_connection.State == System.Data.ConnectionState.Connecting)
			{
				// do nothing while connecting
			}

			return _connection;
		}

		public void Open()
		{
			if (_connection == null || !Connected)
			{
				debugInternal += "| h ";
				if (!IsOdbcConnection)
				{
					_connection = new MySqlConnection(getConnectionString());
				}
				else
				{
					_connection = new OdbcConnection(getConnectionString());
				}
				_connection.Open();	
			}
		}

		public void Close()
		{
			if (_connection != null || Connected)
			{
				Dispose();
			}
		}

		public String getConnectionString()
		{
			return _connectionString;
		}

		public static string getConnectionStringDisplay()
		{
			DBManager dB = new DBManager();
			var _conn =  dB.getConnectionString();

			return _conn.StringScrubber("Password=", ";", "*");
		}

		#endregion 

	}
}

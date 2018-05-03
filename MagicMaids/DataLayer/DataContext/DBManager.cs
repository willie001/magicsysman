﻿#region Using
using System;
using System.Configuration;
using System.Data.Odbc;
using MySql.Data.MySqlClient;
#endregion

namespace MagicMaids.DataAccess
{
	public class DBManager: IDisposable 
	{
		#region Fields
		private string _connectionString = "";
		private MySqlConnection _connection;
		#endregion

		#region Properties
		public bool Connected
		{
			get
			{
				return !(_connection.State == System.Data.ConnectionState.Closed) && _connection.Ping();
			}
		}
		#endregion 

		#region Constructor
		public DBManager()
		{
			_connectionString = ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
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
				if (_connection.State == System.Data.ConnectionState.Open)
				{
					_connection.Close();
					_connection.Dispose();
					_connection.ClearAllPoolsAsync();
				}
			}
		}

		public MySqlConnection getConnection()
		{
			if (_connection != null && _connection.State == System.Data.ConnectionState.Broken)
			{
				_connection.Close();
				_connection.Dispose();
				_connection.ClearPoolAsync(_connection);
			}

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
				_connection = new MySqlConnection(getConnectionString());
				_connection.Open();	
			}
		}

		public String getConnectionString()
		{
			return _connectionString;
		}

		public static string getConnectionStringDisplay()
		{
			DBManager dB = new DBManager();
			return dB.getConnectionString();
		}

		#endregion 

	}
}

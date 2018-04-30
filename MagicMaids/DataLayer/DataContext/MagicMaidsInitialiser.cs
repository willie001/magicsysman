#region Using
using System;
using System.Configuration;
using System.Data.Odbc;
using MySql.Data.MySqlClient;
#endregion

namespace MagicMaids.DataAccess
{
    public class MagicMaidsInitialiser 
    {
		public static MySqlConnection getConnection()
		{
			//return new OdbcConnection(getConnectionString());
			return new MySqlConnection(getConnectionString());
		}

		public static String getConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
		}
    }
}

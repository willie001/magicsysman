#region Using
using System;
using System.Configuration;
using MySql.Data.MySqlClient;
#endregion

namespace MagicMaids.DataAccess
{
    public class MagicMaidsInitialiser 
    {
		public static MySqlConnection getConnection()
		{
			return new MySqlConnection(ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString);
		}

		public static String getConnectionString()
		{
			return ConfigurationManager.ConnectionStrings["MagicMaidsContext"].ConnectionString;
		}
    }
}

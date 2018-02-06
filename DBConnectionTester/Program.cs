using System;
using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;


namespace DBConnectionTester
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var connectionString = "Server=magicdry.a2hosted.com;Database=magicdry_db;Uid=magic_maids;Pwd=dQ6gd6^5;sslmode=None;";

			MySqlConnection connection = new MySqlConnection(connectionString);

			try

			{

				connection.Open();

				string stm = "SELECT VERSION()";
				MySqlCommand cmd = new MySqlCommand(stm, connection);
				string version = Convert.ToString(cmd.ExecuteScalar());
				Console.WriteLine("MySQL version : {0}", version);

				stm = "SELECT count(*) from systemsettings";
				cmd = new MySqlCommand(stm, connection);
				string counter = Convert.ToString(cmd.ExecuteScalar());
				Console.WriteLine("# of settings : {0}", counter);

				connection.Close();

			}
			catch (MySqlException ex)
			{
				string sqlErrorMessage = "Message: " + ex.Message + "\n" +
				"Source: " + ex.Source + "\n" +
				"Number: " + ex.Number;
				Console.WriteLine(sqlErrorMessage);
			}
			catch (Exception ex)
			{
				string sqlErrorMessage = "Message: " + ex.Message + "\n" +
														 "Source: " + ex.Source + "\n";
				Console.WriteLine(sqlErrorMessage);

			}
			finally
			{
				if (connection.State == ConnectionState.Open)
				{
					connection.Close();
				}
			}


		}
	}
}

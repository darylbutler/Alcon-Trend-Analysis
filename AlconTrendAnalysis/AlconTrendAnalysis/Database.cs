using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace AlconTrendAnalysis
{
	class Database
	{
		public static string DatabasePath = "data.db";
		public static bool Loaded = false;  // True once we've successfully loaded a database into memory.  Set to false to invalidate a database

		public static SQLiteConnection InMemoryDatabase;
		public static bool Modified;

		#region Database Maintenance
		// Loads Database from Hardrive into memory
		private static SQLiteConnection Connect()
		{
			if (InMemoryDatabase == null || InMemoryDatabase.State != System.Data.ConnectionState.Open || !Loaded)
			{
				// Release database in memory if it has been invalided with Loaded
				if (!Loaded && (InMemoryDatabase != null))
					InMemoryDatabase.Dispose();

				// Start Open off new Database file
				InMemoryDatabase = new SQLiteConnection("Data Source=:memory:");
				InMemoryDatabase.Open();

				// Load the database
				SQLiteConnectionStringBuilder conn = new SQLiteConnectionStringBuilder();
				conn.DataSource = DatabasePath;
				conn.Version = 3;
				using (var fileDB = new SQLiteConnection(conn.ConnectionString, true))
				{
					fileDB.Open();
					fileDB.BackupDatabase(InMemoryDatabase, "main", "main", -1, null, 0);
					Modified = false;
					Loaded = true;
				}
			}

			return InMemoryDatabase;
		}
		// Saves the Database from Memory to the Harddrive
		public static void SaveDatabase()
		{
			if (InMemoryDatabase != null && InMemoryDatabase.State == System.Data.ConnectionState.Open)
			{
				// Load the database
				SQLiteConnectionStringBuilder conn = new SQLiteConnectionStringBuilder();
				conn.DataSource = DatabasePath;
				conn.Version = 3;
				using (var fileDB = new SQLiteConnection(conn.ConnectionString, true))
				{
					fileDB.Open();
					InMemoryDatabase.BackupDatabase(fileDB, "main", "main", -1, null, 0);
					Modified = false;
				}
			}
		}
		// Create a new blank Database on the harddrive
		public void CreateDatabase()
		{
			string sql =  
				@"
				BEGIN TRANSACTION;
				CREATE TABLE `tags` (
					`id`	INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
					`name`	TEXT NOT NULL UNIQUE,
					`desc`	TEXT
				);
				CREATE TABLE `data` (
					`time`	INTEGER NOT NULL UNIQUE,
					`tag_id`	INTEGER,
					`value`	REAL,
					PRIMARY KEY(time)
				);
				COMMIT;"
			;

			// Create a brand new database file
			if (File.Exists(DatabasePath))
				File.Delete(DatabasePath);


			string path = new Uri(DatabasePath).LocalPath;
			SQLiteConnection.CreateFile(path);

			using (SQLiteConnection connection = Connect())
			{
				if (connection.State != System.Data.ConnectionState.Open) connection.Open();
				using (SQLiteCommand cmd = new SQLiteCommand(connection))
				{
					// Create the tables
					cmd.CommandText = sql;
					cmd.ExecuteNonQuery();
				}
			}
		}
		#endregion

		#region -- Getters
		/// <summary>
		/// Gets the last sec time we have stored in the database
		/// </summary>
		/// <returns>long - time in seconds</returns>
		public static long GetLastTime()
		{
			SQLiteConnection connection = Connect();

			using (SQLiteCommand cmd = new SQLiteCommand(connection))
			{
				cmd.CommandText = "SELECT IFNULL((SELECT MAX(time) FROM data), -1 );";
				cmd.CommandType = System.Data.CommandType.Text;

				return (long)cmd.ExecuteScalar();
			}
		}
		public static long TagExists(string tagName)
		{
			SQLiteConnection connection = Connect();

			using (SQLiteCommand cmd = new SQLiteCommand(connection))
			{
				cmd.CommandText = "SELECT id FROM tags WHERE name=@param1;";
				cmd.CommandType = System.Data.CommandType.Text;
				cmd.Parameters.AddWithValue("@param1", tagName);

				object val = cmd.ExecuteScalar();

				return val == null ? -1 : (long)val;
			}
		}
		#endregion

		#region UpdateFromCSV
		public long AddTag(string name, string desc)
		{
			SQLiteConnection connection = Connect();

			using (SQLiteCommand cmd = new SQLiteCommand(connection))
			{
				cmd.CommandText = "UPDATE tags SET name=@paramName WHERE id=@paramDesc;";
				cmd.CommandType = System.Data.CommandType.Text;
				cmd.Parameters.AddWithValue("@paramName", name);
				cmd.Parameters.AddWithValue("@paramDesc", desc);
				cmd.ExecuteNonQuery();
			}
			Modified = true;
			return connection.LastInsertRowId;
		}
		public void UpdateFromCSV(string file)
		{
			if (!File.Exists(file))
				return;

			long lastTime = GetLastTime();
			int updateFrom = -1;

			using (StreamReader reader = new StreamReader(file))
			{
				// Read first line to find the min and max second times
				string line = reader.ReadLine();
				long[] sectimes = line.Substring(20).Split(',').Cast<long>().ToArray();  // Substring 20 = drop DESC & SECTIME

				// Find if/where we need to update
				if (sectimes.Last() <= lastTime)
					return;	// No update needed
				else
				{
					// Find the update ID to start from
					for (int loopX = 0; loopX < sectimes.Count(); loopX++)
					{
						 if (sectimes[loopX] > updateFrom)
						{
							updateFrom = loopX;
							break;
						}
					}

					if (updateFrom < 0)
						throw new Exception("UpdateFrom is wrong! Not Possible!");
				}

				// Now, every line is a new tag, followed by values
				while (!reader.EndOfStream)
				{
					string[] row = reader.ReadLine().Split(',');
					// 0 = tag description
					// 1 = tag name
					// 2+ = value @ sectime	@ index - 2

					// See if tag exists and, if it doesn't, add it
					long tagID = TagExists(row[1]);
					if (tagID < 0)
						tagID = AddTag(row[1], row[0]);

					// See where we need to update from

				}
			}
		}
		#endregion
	}
}

using FFGlobalVar;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace FFscDB
{
	/// <summary>
	/// Basic function to real handling of DB
	/// 
	/// exists db
	/// creating db
	/// connecting db
	/// write record
	/// remove record
	/// reset db
	/// close connection db
	/// </summary>
	public class DBBase
	{
		string path;
		string name;
		string master;
		string currentName;

		string[] tables;

		SqlConnection sqlConnection;

		public DBBase()
		{
			this.path = ProgEnv.PathStoreDB.dbPath;
			this.name = ProgEnv.PathStoreDB.dbName;
			this.master = ProgEnv.PathStoreDB.masterName;
			this.tables = ProgEnv.PathStoreDB.dbTables;

			this.Connect();
		}
		public DBBase(bool reset, string dbPath)
		{
			this.path = dbPath;
			Connect();
			if (Exists())
			{
				if (reset)
				{
					Delete();
					Create();
				}
			}
			else
			{
				Create();
			}
		}

		public bool Exists()
		{
			try
			{
				using (SqlCommand sqlCommand = new SqlCommand(Query.exists(this.name), this.sqlConnection))
				{
					var res = sqlCommand.ExecuteScalar();
					return (res != DBNull.Value);
				}
			}
			catch
			{
				return false;
			}
		}

		public void Create()
		{
			try
			{

				using (SqlCommand cmd = new SqlCommand(Query.createDB(path, name), this.sqlConnection))
				{
					cmd.ExecuteNonQuery();
				}
				//Console.WriteLine("Database created");
				if (currentName == master)
				{
					int tryNum = 5;
					while (tryNum > 0)
					{
						//Console.WriteLine("Reconnection");
						try
						{
							this.Close();
							this.Connect();//reconnect with name

							if (currentName == master)
							{
								//Console.WriteLine("Error Reconnection");
								//if the database cannot respond at the connection, there's an error
								throw new ProgEnv.ProgErrs.DBNotResponding();
							}
						}
						catch 
						{
							tryNum--;
							continue;
						}
						//if is connected
						break;
					}

					if (currentName == master)
					{
						//Console.WriteLine("Error Reconnection");
						//if the database cannot respond at the connection, there's an error
						throw new ProgEnv.ProgErrs.DBNotResponding();
					}
					
				}
				//then create tables
				this.createTables();
			}
			catch
			{
				//Console.WriteLine("Error creating");
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		private void createTables()
		{
			try
			{
				foreach (var table in this.tables)
				{
					using (SqlCommand cmd = new SqlCommand(Query.createTable(table), this.sqlConnection))
					{
						cmd.ExecuteNonQuery();
					}
				}
				//Console.WriteLine("Tables created");
			}
			catch(Exception e)
			{
				//Console.WriteLine("Error creating tables: " + e.Message);
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		public void Connect()
		{
			this.Close();
			try
			{
				//Console.WriteLine("Connection as name");
				this.currentName = name;
				this.sqlConnection = new SqlConnection(Query.connectionString(currentName));
				this.sqlConnection.Open();
				//Console.WriteLine("Connected as name"); 
			}
			catch (Exception e)
			{
				//Console.WriteLine("Connection as name error: " + e.Message);
				try
				{
					//Console.WriteLine("Connection as master");
					this.currentName = master;
					this.sqlConnection = new SqlConnection(Query.connectionString(currentName));
					this.sqlConnection.Open();
					//Console.WriteLine("Connected as master");
				}
				catch
				{
					//Console.WriteLine("Not connected ");
					throw new ProgEnv.ProgErrs.DBNotResponding();
				}
			}
		}

		public void Connect(string dbName)
		{
			//overload to choose the dbName for the connection
			try
			{
				this.currentName = dbName;
				this.sqlConnection = new SqlConnection(Query.connectionString(dbName));
				this.sqlConnection.Open();
			}
			catch
			{
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}

		}

		public void Close()
		{
			try
			{
				if (this.sqlConnection.State == System.Data.ConnectionState.Open)
				{
					this.sqlConnection.Dispose();
					this.sqlConnection.Close();
				}
			}
			catch { }
		}

		public void Write(string table, string pathToSave, string nameToSave)
		{
			try
			{
				using (SqlCommand cmd = new SqlCommand(Query.write(table, pathToSave, nameToSave), this.sqlConnection))
				{
					cmd.ExecuteNonQuery();
				}
			}
			catch
			{
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		public void Remove(string table, string pathSaved)
		{
			try
			{
				using (SqlCommand cmd = new SqlCommand(Query.remove(table, pathSaved), this.sqlConnection))
				{
					cmd.ExecuteNonQuery();
				}
				//this.Close();
				//this.Connect();
			}
			catch
			{
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		public void Update(string query)
		{
			try
			{
				using (SqlCommand cmd = new SqlCommand(query, this.sqlConnection))
				{
					cmd.ExecuteNonQuery();
				}
			}
			catch
			{
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		private List<Tuple<string, string>> ReadInside(string query)
		{
			List<Tuple<string, string>> output = new List<Tuple<string, string>>();

			try
			{
				using (SqlCommand cmd = new SqlCommand(query, this.sqlConnection))
				{
					cmd.CommandTimeout = 0; //sometimes require lots of time
					using (SqlDataReader reader = cmd.ExecuteReader(System.Data.CommandBehavior.Default))
					{
						while (reader.Read())
						{
							var tmp = new Tuple<string, string>(reader[1].ToString(), reader[2].ToString());
							output.Add(tmp);
						}
					}
				}
				return output;
			}
			catch
			{
				throw new ProgEnv.ProgErrs.DBNotResponding();
			}
		}

		public List<Tuple<string, string>> Read(string table, string pathSaved)
		{
			return ReadInside(Query.read(table, pathSaved));
		}

		//returns all path founded
		public Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>> Search(string queryFiles, string queryDirs)
		{
			var res = ReadInside(queryFiles);
			var dres = ReadInside(queryDirs);

			return new Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>>(res, dres);
		}

		public void Delete()
		{
			try
			{
				try
				{
					using (SqlCommand cmd = new SqlCommand(Query.deleteTables(ProgEnv.PathStoreDB.dbTables), this.sqlConnection))
					{
						cmd.ExecuteNonQuery();
					}
				}
				catch { }

				this.Close();
				this.Connect(master); //connect with master and drop database, then create db again and change connection string 

				using (SqlCommand cmd = new SqlCommand(Query.delete(name), this.sqlConnection))
				{
					cmd.ExecuteNonQuery();
				}
			}
			catch
			{
				try
				{
					File.Delete(ProgEnv.PathStoreDB.dbPath + ".mdf");
					File.Delete(ProgEnv.PathStoreDB.dbPath + ".ldf");
				}
				catch
				{
					throw new ProgEnv.ProgErrs.DBNotResponding();
				}
			}
		}
	}
}

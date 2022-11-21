using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFGlobalVar
{
	public static class Query
	{
		//public static Func<string, string> connectionString = (name) => @$"Server=(localdb)\MSSQLLocalDb;Integrated security=SSPI;database={name}";
		public static Func<string, string> connectionString = (name) => @$"Data Source=(localdb)\MSSQLLocalDb;Integrated security=SSPI;database={name};";
		//public static Func<string, string> connectionString = (path) => @"Server=(local)\netsdk;database=master";
		//public static Func<string, string> connectionString = (path) => @"Server=(local)\netsdk;Data Source=(localdb)\\MSSQLLocalDb;Integrated Security=true;AttachDbFileName=" + path+".mdf";
		//public static Func<string, string> connectionString = (path) => @"Data Source=(localdb)\MSSQLLocalDb;Integrated Security=true;AttachDbFileName=" + path+".mdf";
		
		public static Func<string, string> exists = (name) => @"SELECT db_id('" + name + "')";


		public static Func<string, string, string> createDB = (path, name) =>	"CREATE DATABASE "+name+" ON PRIMARY " +
																"(NAME = "+name+"_Data, " +
																"FILENAME = '"+ path +".mdf', " +
																"SIZE = 2MB, MAXSIZE = 1000MB, FILEGROWTH = 10%)" +
																"LOG ON (NAME = "+name+"_Log, " +
																"FILENAME = '"+ path +".ldf', " +
																"SIZE = 1MB, " +
																"MAXSIZE = 50MB, " +
																"FILEGROWTH = 10%);";

		public static Func<string, string> createTable = (table) => "CREATE TABLE "+ table + " (" +
																	" Id INT NOT NULL IDENTITY(1,1),"+
																	" Path VARCHAR(max) NOT NULL," + //path, pk
																	" Name VARCHAR(max) NOT NULL," +	//name
																	" PRIMARY KEY (Id));";

		public static Func<string, string, string, string> write = (table, path, name) =>	"INSERT INTO " + table +
																							" (Path, Name) VALUES ('"+path+"', '"+name+"');";

		public static Func<string, string, string> remove = (table, path) => "DELETE FROM " + table + " WHERE Path LIKE '" + path + "';";

		public static Func<string, string, string> read = (table, path) =>	"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED; "+
																			"SELECT * FROM " + table + " WHERE Path = '" + path + "';";

		public static Func<string, string> delete = (name) =>	$"USE master;" +
																$"ALTER DATABASE [{name}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;" +
																$"DROP DATABASE {name};";

		public static Func<string, string, string, string> updatePath = (table, oldPath, newPath) => $"UPDATE {table} " +
																									$"SET Path = REPLACE(Path, '{oldPath}', '{newPath}') " +
																									$"WHERE Path LIKE '{oldPath}\\%';";

		public static Func<string, string, string, string> rename = (table, oldPath, newPath) => $"UPDATE {table} " +
																									$"SET Path = {newPath} " +
																									$"WHERE Path LIKE '{oldPath}';";

		public static Func<string[], string> deleteTables = (tables) => $"DROP TABLE {tables[0]}; " +
																		$" DROP TABLE {tables[1]};";
																				

		public static Func<string, string, string, string> searchNormal = (table, path, name) =>
																			"SELECT * FROM " + table + " WHERE " +
																			"Path LIKE '" + path + "%' AND Path NOT LIKE '" +path+"%\\%' "+
																			"AND Name LIKE '" + name + "' "+
																			"ORDER BY Path DESC;";

		public static Func<string, string, string, string> searchRecursive= (table, path, name) =>
																			"SELECT * FROM " + table + " WHERE " +
																			"Path LIKE '" + path + "%' AND Name LIKE '" + name + "' " +
																			"ORDER BY Path DESC;";
	}
}

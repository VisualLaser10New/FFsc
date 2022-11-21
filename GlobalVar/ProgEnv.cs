using System;
using System.Collections.Generic;
using System.Linq;

namespace FFGlobalVar
{
	public static class ProgEnv
	{
		public static string[] folders = { "Config", "Data" };

		public static string xmlConfigPath = $".\\{folders[0]}\\config.xml";
		public static string xsdConfigPath = $".\\{folders[0]}\\config.xsd";
		public static string hashConfigPath = $".\\{folders[0]}\\hash.hs";

		public static class PathStoreDB
		{
			public static string dbName = "PathStore";
			public static string masterName = "";
			public static string dbPath = $"{Environment.CurrentDirectory}\\{folders[1]}\\{dbName}";
			public static string[] dbTables = { "FilesTable", "DirsTable" };
		}


		public static class RegexPattern
		{
			public static string pathRegex = @"^[A-Z]{1}:\\\\(?:([^\\\<\:\""\/\|\?\*]+\\)+)?$";
			public static string fileRegex = @"^[A-Z]{1}:\\\\(?:([^\\\<\:\""\/\|\?\*]+\\)+)?.+\.[^\\\<\:\""\/\|\?\*]+$";
		}

		public static class Sentences
		{
			public static Func<string, string> fileStoredOK = (path) => $"File/Dir \"{path}\" stored succesfull\n";
			public static Func<string, string> fileStoredERR = (path) => $"File/Dir \"{path}\" stored error\n";
			public static Func<string, string> fileExcludedInfo = (path) => $"File/Dir \"{path}\" is excluded\n";

			public static Func<string, string> fileRemovedOK = (path) => $"File/Dir \"{path}\" removed succesfull\n";
			public static Func<string, string> fileRemovedERR = (path) => $"File/Dir \"{path}\" removed error\n";

			public static Func<string, string> fileRenamedOK = (path) => $"File/Dir \"{path}\" renamed succesfull\n";
			public static Func<string, string> fileRenamedERR = (path) => $"File/Dir \"{path}\" renamed error\n";


			public static Func<string, string> foundFor = (table) => $"{table} found:\n\n";


			public static string scannerIntro = "The disk will be scanned now, this may take a while\n";
			public static string noItemFound = "No item found, try to repeat the search changing args\n";
			public static string done = "Done\n";
			public static string waitSeconds = "Wait some seconds...\n";

			public static class Analyzer 
			{
				public static Func<string, string> changeInfo = (path) => $"File/Dir \"{path}\" changed\n";
				public static Func<string, string> createInfo = (path) => $"File/Dir \"{path}\" created\n";
				public static Func<string, string> deleteInfo = (path) => $"File/Dir \"{path}\" deleted\n";
				public static Func<string, string, string> renameInfo = (oldPath , newPath) => $"File/Dir \n\tfrom: \"{oldPath}\"\n\tto: \"{newPath}\"\n  renamed\n";
				public static string startAnalyzing = "Analyze started, wait for a file changing...\n";
			}
		}

		public static class ProgErrs
		{
			public class XmlConfigNotValid : Exception
			{
				public XmlConfigNotValid() : base("Xml config not Valid or Impossible to read config file")
				{
					return;
				}
			}

			public class ProgramCorrupted : Exception
			{
				public ProgramCorrupted() : base("The program was corrupted, please re-install it")
				{
					return;
				}
			}

			public class CannotWriteHash : Exception
			{
				public CannotWriteHash() : base("Cannot write hash on properly file, please restart program")
				{
					return;
				}
			}

			public class MustResetDB : Exception
			{
				public MustResetDB() : base("The database requires a reset")
				{
					return;
				}
			}

			public class DBNotResponding : Exception
			{
				public DBNotResponding() : base("The database doesn't respond, please restart the program")
				{
					return;
				}
			}

			public class ArgumentsNotValid : Exception
			{
				public ArgumentsNotValid() : base("Arguments not valid, please read the docs\n")
				{
					return;
				}
			}

			public class PathNotExists : Exception
			{
				public PathNotExists() : base("Path is not valid\n")
				{
					return;
				}
			}
		}
	}
}
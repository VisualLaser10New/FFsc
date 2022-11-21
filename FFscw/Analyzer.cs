using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFGlobalVar;

namespace FFscw
{
    public class Analyzer
    {
		Config config;
		FFscDB.DBBase bBase;
		bool verboseLog;

		public Analyzer(Config conf, ref FFscDB.DBBase database)
		{
			this.config = conf;
			this.bBase = database;
		}

		public void Analyze(bool verbose = true)
		{
			List<Task> tasks = new List<Task>();
			this.verboseLog = verbose;
			foreach(var conf in config.configList)
			{
				if (conf.active && Directory.Exists(conf.root))
				{
					tasks.Add(Task.Run(() =>
					{
						analyzeRoot(conf.root);
					}));
				}
			}
			//wait all tasks
			Task.WaitAll(tasks.ToArray());
		}


		private void analyzeRoot(string root)
		{
			using var watcher = new FileSystemWatcher(root);

			watcher.NotifyFilter = NotifyFilters.Attributes
										 | NotifyFilters.CreationTime
										 | NotifyFilters.DirectoryName
										 | NotifyFilters.FileName
										 | NotifyFilters.LastAccess
										 | NotifyFilters.LastWrite
										 | NotifyFilters.Security
										 | NotifyFilters.Size;

			watcher.Changed += OnChanged;
			watcher.Created += OnCreated;
			watcher.Deleted += OnDeleted;
			watcher.Renamed += OnRenamed;
			watcher.Error += OnError;

			//watcher.Filter = "*.*";
			watcher.IncludeSubdirectories = true;
			watcher.EnableRaisingEvents = true;

			while (true)
			{
				watcher.WaitForChanged(WatcherChangeTypes.All);
			}
		}

		private void OnChanged(object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType != WatcherChangeTypes.Changed)
			{
				return;
			}

			printLog(ProgEnv.Sentences.Analyzer.changeInfo(e.FullPath));
		}

		private void OnCreated(object sender, FileSystemEventArgs e)
		{
			printLog(ProgEnv.Sentences.Analyzer.createInfo(e.FullPath));
			string table = getTable(e.FullPath);

			if(!respectConf(e.FullPath, e.Name,  table))
			{
				printLog(ProgEnv.Sentences.fileExcludedInfo(e.FullPath));
				return;
			}

			try
			{
				bBase.Write(table, e.FullPath, e.Name);
				printLog(ProgEnv.Sentences.fileStoredOK(e.FullPath));
			}
			catch
			{
				printLog(ProgEnv.Sentences.fileStoredERR(e.FullPath));
				return;
			}
		}

		private void OnDeleted(object sender, FileSystemEventArgs e)
		{
			printLog(ProgEnv.Sentences.Analyzer.deleteInfo(e.FullPath));

			string table = getTable(e.FullPath);
			if (!respectConf(e.FullPath, e.Name,  table))
			{
				printLog(ProgEnv.Sentences.fileExcludedInfo(e.FullPath));
				return;
			}

			try
			{
				bBase.Remove(table, e.FullPath);
				printLog(ProgEnv.Sentences.fileRemovedOK(e.FullPath));


				if(table == ProgEnv.PathStoreDB.dbTables[1])
				{
					//if is a folder delete also what are inside
					bBase.Remove(ProgEnv.PathStoreDB.dbTables[0], e.FullPath + "\\%"); //files
					bBase.Remove(ProgEnv.PathStoreDB.dbTables[1], e.FullPath + "\\%"); //dirs
				}
			}
			catch
			{
				printLog(ProgEnv.Sentences.fileRemovedERR(e.FullPath));
				return;
			}
		}

		private void OnRenamed(object sender, RenamedEventArgs e)
		{
			printLog(ProgEnv.Sentences.Analyzer.renameInfo(e.OldFullPath, e.FullPath));
			string table = getTable(e.FullPath);

			if (!respectConf(e.OldFullPath, e.OldName, table))
			{
				printLog(ProgEnv.Sentences.fileExcludedInfo(e.OldFullPath));
				return;
			}

			try
			{
				//update the dir
				//bBase.Remove(table, e.OldFullPath);
				//bBase.Write(table, e.FullPath, e.Name);
				bBase.Update(Query.rename(table, e.OldFullPath, e.FullPath));
				printLog(ProgEnv.Sentences.fileRenamedOK(e.FullPath));

				if(table == ProgEnv.PathStoreDB.dbTables[1])
				{
					//if is a folder replace also what are inside
					bBase.Update(Query.updatePath(ProgEnv.PathStoreDB.dbTables[0],
													e.OldFullPath,
													e.FullPath));
					bBase.Update(Query.updatePath(ProgEnv.PathStoreDB.dbTables[1],
													e.OldFullPath,
													e.FullPath));
				}
			}
			catch
			{
				printLog(ProgEnv.Sentences.fileRenamedERR(e.FullPath));
				return;
			}
		}

		private void OnError(object sender, ErrorEventArgs e) =>
			PrintException(e.GetException());

		private void PrintException(Exception? ex)
		{
			if (ex != null)
			{
				Console.WriteLine($"Message: {ex.Message}");
				Console.WriteLine("Stacktrace:");
				Console.WriteLine(ex.StackTrace);
				Console.WriteLine();
				PrintException(ex.InnerException);
			}
		}

		private void printLog(string message)
		{
			if(this.verboseLog)
			{
				Console.Write(message);
			}
		}

		private string getTable(string path)
		{
			if(File.Exists(path))
			{
				return ProgEnv.PathStoreDB.dbTables[0];
			}
			else if(Directory.Exists(path))
			{
				return ProgEnv.PathStoreDB.dbTables[1];
			}
			else 
			{
				if (bBase.Read(ProgEnv.PathStoreDB.dbTables[0],path).Count > 0)
				{
					return ProgEnv.PathStoreDB.dbTables[0];
				}
				else
				{
					return ProgEnv.PathStoreDB.dbTables[1];
				}
			}
		}

		private bool respectConf(string path, string name, string table)
		{
			if (path == null || path == "")
			{
				return false;
			}
			else
			{
				bool isDir = table == ProgEnv.PathStoreDB.dbTables[1];

				if (isDir)
				{
					if (this.config.configList.Count(conf => conf.dirConfs.respectConfig(path, name)==false) > 0)
					{
						return false;
					}
					return true;
				}
				else
				{
					if (this.config.configList.Count(conf => conf.fileConfs.respectConfig(path, name)==false) > 0)
					{
						return false;
					}
					return true;
				}
			}
		}
    }
}

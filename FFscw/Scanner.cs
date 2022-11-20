using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FFGlobalVar;


namespace FFscw
{
	public class Scanner
	{
		Config config;
		FFscDB.DBBase bBase;

		public Scanner(Config config, FFscDB.DBBase database) 
		{
			this.config = config;
			this.bBase = database;
		}

		public void Scan(bool verbose = true)
		{
			foreach(var conf in this.config.configList)
			{
				if (conf.active && Directory.Exists(conf.root))
				{
					this.nowConf = conf;
					recursiveScanner(conf.root, verbose);
				}
			}
		}


		ConfigBase nowConf;
		private void recursiveScanner(string path, bool verbose)
		{
			string[] dirl;
			try
			{
				var file = Directory.GetFiles(path);
				if (file.Count() > 0)
				{
					saveDB(ProgEnv.PathStoreDB.dbTables[0], file.ToList(), verbose);
				}
			}
			catch { }

			try
			{
				dirl = Directory.GetDirectories(path);
			}
			catch
			{
				return;
			}

			try
			{
				if (dirl.Length != 0 && dirl is not null)
				{
					saveDB(ProgEnv.PathStoreDB.dbTables[1], dirl.ToList(), verbose);
					foreach (var dire in dirl)
					{
						recursiveScanner(dire, verbose);
					}
				}
			}
			catch
			{
				return;
			}
		}

		private void saveDB(string table, List<string> contents, bool verbose)
		{
			ExInc tmpConf;
			string outputStr = "";

			if (table == ProgEnv.PathStoreDB.dbTables[0])
			{
				tmpConf = this.nowConf.fileConfs;
			}
			else if (table == ProgEnv.PathStoreDB.dbTables[1])
			{
				tmpConf = this.nowConf.dirConfs;
			}
			else
			{
				return;
			}


			foreach(var path in contents)
			{
				string name = "";
				try
				{
					name = path.Split("\\").Last();
				}
				catch
				{
					return;
				}

				try
				{
					if (tmpConf.respectConfig(path, name))
					{
						bBase.Write(table, path, name);
						outputStr += ProgEnv.Sentences.fileStoredOK(path);
					}
					else
					{
						outputStr += ProgEnv.Sentences.fileExcludedInfo(path);
					}
				}
				catch
				{
					outputStr += ProgEnv.Sentences.fileStoredERR(path);
				}
			}

			if (verbose)
			{
				Console.Write(outputStr);
			}
		}
	}
}

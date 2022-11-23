using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using FFGlobalVar;
using FFscDB;

namespace FFsc
{
	/// <summary>
	/// File Finder System Console
	/// This program is a command callable from terminal.
	/// Calling it starts and search file and other from the DB.
	/// </summary>

	/// <params>
	///	ffsc.exe
	///	
	/// -p --path : path from where search (optional)
	/// -s --searching : regex pattern of what are searching
	/// -r --recursive : if there is search recursive
	/// -h --help : print man
	/// 
	/// </params>
	internal class Program
	{
		static int Main(string[] args)
		{

			var parameters = Parser.Default.ParseArguments<ParamsOption>(args);

			try
			{
				parameters.WithParsed(options =>
					search(options.Path, options.Searching, options.Recursive)

				).WithNotParsed(_ =>
				{
					throw new ProgEnv.ProgErrs.ArgumentsNotValid();
				});
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				exit(1);
			}

			exit(0);
			return 0;
		}

		static void search(string path, string search, bool recursive)
		{
			DBBase bBase;

			try
			{
				if (!Directory.Exists(path))
				{
					throw new ProgEnv.ProgErrs.PathNotExists();
				}
				bBase = new DBBase();
				if (!bBase.Exists())
				{
					throw new ProgEnv.ProgErrs.DBNotResponding();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				exit(1);
				return;
			}

			string fileQuery = recursive ? Query.searchRecursive(ProgEnv.PathStoreDB.dbTables[0], path, search) :
											Query.searchNormal(ProgEnv.PathStoreDB.dbTables[0], path, search);
			string dirQuery = recursive ? Query.searchRecursive(ProgEnv.PathStoreDB.dbTables[1], path, search) :
											Query.searchNormal(ProgEnv.PathStoreDB.dbTables[1], path, search);

			var res = bBase.Search(fileQuery, dirQuery);
			printResult(res);
		}

		static void printResult(Tuple<List<Tuple<string, string>>, List<Tuple<string, string>>> res)
		{
			//probably the most brazen method in all the program
			if (res.Item1.Count() == 0 && res.Item2.Count() == 0)
			{
				Console.Write(ProgEnv.Sentences.noItemFound);
				exit(1);
			}

			if (res.Item1.Count() > 0)
			{
				Console.Write(ProgEnv.Sentences.foundFor(res.Item1.Count(),ProgEnv.PathStoreDB.dbTables[0].Replace("Table","")));
				printList(res.Item1);
			}

			if (res.Item2.Count() > 0)
			{
				Console.Write(ProgEnv.Sentences.foundFor(res.Item2.Count(), ProgEnv.PathStoreDB.dbTables[1].Replace("Table", "")));
				printList(res.Item2);
			}

			static void printList(List<Tuple<string, string>> tuples)
			{
				foreach (var el in from el in tuples select el.Item1)
				{
					Console.WriteLine(el);
				}
			}
			Console.Write(ProgEnv.Sentences.done);
		}

		static void exit(int code)
		{
#if DEBUG
			Console.ReadKey();
#endif
			Environment.Exit(code);
		}
	}
}


		/*static List<string> filesAll = new List<string>();

		static void Analyze(string path, ref Dir dirp)
		{
			string[] dirl;
			try
			{
				dirp.files = Directory.GetFiles(path).ToList();
				filesAll.AddRange(dirp.files);
				dirl = Directory.GetDirectories(path);
			}
			catch
			{
				return;
			}

			if (dirl.Length != 0 && dirl is not null)
			{
				foreach (var dire in dirl)
				{
					Dir tmp = new Dir(dire);
					Analyze(dire, ref tmp);
					dirp.dirs.Add(tmp);
				}
			}
		}*/


	/*class Dir
	{
		public string name;
		public List<Dir> dirs = new List<Dir>();

		public List<string> files;

		public Dir(string path)
		{
			this.name = path;
		}
	}*/
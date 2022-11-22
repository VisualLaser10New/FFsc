using FFscDB;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FFGlobalVar;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace FFscw
{

	/// <summary>
	/// This program is a daemon, w stands for worker.
	/// It uses a config file to know what Disks are to be analyzed, which folder will be skipped.
	/// 
	/// It starts when Windows starts: if it's the first time scans all selected disks,
	/// else intercepts the IO filesystem calling and update the DB.
	/// </summary>
	/// 
	/// <params>
	/// -r reset database
	/// -v verbose
	/// </params>
	internal class Program
	{
		static Config config;
		static DBBase bBase;
		static bool verbosity;

		static void Main(string[] args)
		{
			screen(false);
			Console.Write(ProgEnv.Sentences.waitSeconds);

			try
			{
				createFolders();
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				exit(1);
			}

			try
			{
				//try connect DB
				bBase = new DBBase();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				exit(1);
			}

			//loading config
			if(!loadConfig() || config == null)
			{
				exit(1);
			}


			if (args.ToList().Contains("-r"))
			{
				//kill other instances of applications
				killOtherInstances();
				
				//reset the database
				resetDB();
			}
			else
			{
				if(Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1)
				{
					//if there are other istance of this program
					exit(1);
				}
			}

			if (args.ToList().Contains("-v"))
			{
				verbosity = true;
			}

			try
			{
				if (!bBase.Exists())
				{
					//if database not exist
					firstStart();
				}
				Console.Write(ProgEnv.Sentences.Analyzer.startAnalyzing);
				Analyzer analyzer = new Analyzer(config, ref bBase);
				analyzer.Analyze(verbose:verbosity);
				
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				exit(1);
			}
			Console.Write(ProgEnv.Sentences.done);

			exit(0);
		}

		static void firstStart()
		{
			//Console.WriteLine("firstStart");
			bBase.Create();
			screen(true);
			//Console.WriteLine("Scanning");
			Scanner sc = new Scanner(config, bBase);
			sc.Scan(verbose:verbosity);

			screen(false);
		}

		static bool loadConfig()
		{
			try
			{
				config = new Config();
				return true;
			}
			catch(ProgEnv.ProgErrs.MustResetDB)
			{
				//db to reset
				try
				{
					resetDB();
					config = new Config();
					return true;
				}
				catch(Exception e)
				{
					Console.WriteLine(e.Message);
					return false;
				}
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message); 
				return false;
			}
		}

		static void resetDB()
		{
			try
			{
				if (bBase.Exists())
				{
					bBase.Delete();
				}
			}
			catch
			{
				return;
			}
		}

		

		static void screen(bool show)
		{
			[DllImport("kernel32.dll")]
			static extern IntPtr GetConsoleWindow();

			[DllImport("user32.dll")]
			static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

			IntPtr h = GetConsoleWindow();
			ShowWindow(h, show == false ? 0 : 5);
		}

		static void exit(int code)
		{
			GC.Collect(); //run garbage collector
			try
			{
				bBase.Close();
			}
			catch { }
#if DEBUG
			Console.ReadKey();
#endif
			Environment.Exit(code);
		}

		static void createFolders()
		{
			foreach(var el in ProgEnv.folders)
			{
				try
				{
					if (!Directory.Exists(el))
					{
						Directory.CreateDirectory(el);
					}
				}
				catch
				{
					throw new ProgEnv.ProgErrs.ProgramCorrupted();
				}
			}
		}

		static void killOtherInstances()
		{
			var thisProcess = Process.GetCurrentProcess();
			var all = Process.GetProcessesByName(thisProcess.ProcessName);

			foreach(var oth in from oth in all 
							   where oth.Id != thisProcess.Id 
							   select oth)
			{
				oth.Kill();
			}
		}
	}
}

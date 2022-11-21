using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace FFsc
{
	public class ParamsOption
	{
		private string _path;

		[Option('p', "path", Required = true, HelpText = "Set the path from searching")]
		public string Path { 
			get { return _path; } 
			set
			{
				if (value == null || value == String.Empty)
				{
					_path = Environment.CurrentDirectory;
				}
				else
				{
					_path = value.Replace("\"", "");
				}
				_path += _path.EndsWith("\\") ? "" : "\\";
			} 
		}

		[Option('s', "searching", Required = true, HelpText = "Set the regex pattern of what are searching")]
		public string Searching { get; set; }

		[Option('r', "recursive", Required = false, HelpText = "Enable recursive search")]
		public bool Recursive { get; set; }

		[Usage(ApplicationAlias = "FFsc")]
		public static IEnumerable<Example> Examples
		{
			get
			{
				return new List<Example>() {
					new Example("Search example.txt", new ParamsOption{Path= @"C:\Users\user1", Searching=@"example.txt"}),
					new Example("Search all txt files", new ParamsOption{Path= @"C:\Users\user1", Searching=@"%.txt", Recursive=true}),
					new Example("Don't type the \\ at the end in path", ""),
				  };
			}
		}
	}
}

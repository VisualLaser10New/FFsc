using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Reflection.Metadata.Ecma335;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using FFGlobalVar;

namespace FFscw
{
	public class ExInc
	{
		public List<string> excludePath = new List<string>();
		public List<string> excludeName = new List<string>();

		public List<string> includePath = new List<string>();
		public List<string> includeName = new List<string>();


		protected bool setPathBase(string path, string reg, bool exclude)
		{
			if (exclude)
			{
				if (Regex.IsMatch(path, reg))
				{
					this.excludePath.Add(path);
					return true;
				}
				return false;
			}
			else
			{
				if (Regex.IsMatch(path, reg))
				{
					this.includePath.Add(path);
					return true;
				}
				return false;
			}
		}

		protected bool setNameBase(string name, bool exclude)
		{
			if (exclude)
			{
				this.excludeName.Add(name);
				return true;
			}
			else
			{
				this.includeName.Add(name);
				return true;
			}
		}
		
		public bool respectConfig(string path, string name)
		{
			if(includePath.StartsWith(path) || includeName.Contains(name))
			{
				return true;
			}
			else if(excludePath.StartsWith(path) || excludeName.Contains(name))
			{
				return false;
			}
			else
			{
				return true;
			}
		}
	}

	public interface ExIncFunc
	{
		bool setPathExclude(string path);
		bool setNameExclude(string name);

		bool setPathInclude(string path);
		bool setNameInclude(string name);
	}

	public class DirConf: ExInc, ExIncFunc
	{
		public bool setPathExclude(string path)
		{
			return setPathBase(path, ProgEnv.RegexPattern.pathRegex, true);
		}

		public bool setNameExclude(string name)
		{
			return setNameBase(name, true);
		}


		public bool setPathInclude(string path)
		{
			return setPathBase(path, ProgEnv.RegexPattern.pathRegex, false);
		}

		public bool setNameInclude(string name)
		{
			return setNameBase(name, false);
		}
	}

	public class FileConf: ExInc, ExIncFunc 
	{
		public bool setPathExclude(string path)
		{
			return setPathBase(path, ProgEnv.RegexPattern.fileRegex, true);
		}

		public bool setNameExclude(string name)
		{
			return setNameBase(name, true);
		}


		public bool setPathInclude(string path)
		{
			return setPathBase(path, ProgEnv.RegexPattern.fileRegex, false);
		}

		public bool setNameInclude(string name)
		{
			return setNameBase(name, false);
		}
	}

	public class ConfigBase
	{
		public int id;
		public bool active;

		private string _root;
		public string root
		{
			set
			{
				if(Regex.IsMatch(value, ProgEnv.RegexPattern.pathRegex))
				{
					_root = value;
				}
				else
				{
					_root = "";
				}
			}
			get
			{
				return _root;
			}
		}
		
		public DirConf dirConfs = new DirConf();
		public FileConf fileConfs = new FileConf();
	}

	public class Config
	{
		public List<ConfigBase> configList = new List<ConfigBase>();

		public Config()
		{
			
			if(!File.Exists(ProgEnv.xsdConfigPath) || !File.Exists(ProgEnv.xmlConfigPath))
			{
				//xml || xsd file not found
				throw new ProgEnv.ProgErrs.ProgramCorrupted();
			}

			if(!validateConfig() || this.configList.Count() <= 0) // validate and read config
			{
				throw new ProgEnv.ProgErrs.XmlConfigNotValid();
			}

			string nowHash = CalcMD5(ProgEnv.xmlConfigPath);
			if (isSameHash(nowHash, ProgEnv.hashConfigPath))
			{
				return;
			}
			else
			{
				//file edited
				try
				{
					//write new hash to file
					File.WriteAllText(ProgEnv.hashConfigPath, nowHash);
				}
				catch
				{
					throw new ProgEnv.ProgErrs.CannotWriteHash();
				}

				//stands to delete current db and create new one
				throw new ProgEnv.ProgErrs.MustResetDB();
			}
			
		}

		private bool validateConfig()
		{
			//load config and validate with xsd if found
			XmlReaderSettings settings = new XmlReaderSettings();
			XmlReader reader;

			try
			{
				settings.Schemas.Add(@"http://www.w3.org/2001/XMLSchema", ProgEnv.xsdConfigPath);
				settings.ValidationType = ValidationType.Schema;


				reader = XmlReader.Create(ProgEnv.xmlConfigPath, settings);
				XmlDocument document = new XmlDocument();
				document.Load(reader);
			

				ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);

				// the following call to Validate succeeds
		
				document.Validate(eventHandler);
			}
			catch
			{
				return false;
			}
			try
			{
				this.readConfig();
				if (configList.Count() > 0)
					return true;
				else
					return false;
			}
			catch
			{
				return false;
			}
		}

		private static void ValidationEventHandler(object sender, ValidationEventArgs e)
		{

		}

		private void readConfig()
		{
			//load config, hash it, check previous hash in file hash.txt if exists
			//if hash is the same continue, else delete DB, update hash.txt and re-scann

			//two or more roots' config cannot have the same beginning

			List<XmlReader> xmlConfs = new List<XmlReader>();

			XmlReader readAll = XmlReader.Create(ProgEnv.xmlConfigPath);
			readAll.ReadToDescendant("config");
			do
			{
				ConfigBase tmp = new ConfigBase();


				readAll.MoveToFirstAttribute();
				tmp.id = Int32.Parse(readAll.Value);

				readAll.MoveToNextAttribute();
				tmp.active = Boolean.Parse(readAll.Value);

				readAll.MoveToNextAttribute();
				tmp.root = readAll.Value;
				if (tmp.root == "")
				{
					//if root is empty
					continue;
				}
				else
				{
					//if root is already present
					var m1 = this.configList.Where(p => p.root.StartsWith(tmp.root)).Count();
					var m2 = this.configList.Where(p => tmp.root.StartsWith(p.root)).Count();

					if (m1 > 0 || m2 > 0)
					{
						continue;
					}
				}

				readAll.MoveToElement();
				XmlReader reader = XmlReader.Create(new StringReader(readAll.ReadOuterXml()));
				

				DirConf tmpdirConf = new DirConf();
				reader.ReadToFollowing("excluded"); //excluded dirs

				while (reader.ReadToFollowing("exclude"))
				{
					reader.MoveToFirstAttribute();
					if (reader.Name == "path")
					{
						tmpdirConf.setPathExclude(reader.Value);
					}
					else if (reader.Name == "name")
					{
						tmpdirConf.setNameExclude(reader.Value);
					}
				}

				reader.ReadToFollowing("included"); //included dirs
				while (reader.ReadToFollowing("include"))
				{
					reader.MoveToFirstAttribute();
					if (reader.Name == "path")
					{
						tmpdirConf.setPathInclude(reader.Value);
					}
					else if (reader.Name == "name")
					{
						tmpdirConf.setNameInclude(reader.Value);
					}
				}
				tmp.dirConfs = tmpdirConf;



				FileConf tmpfileConf = new FileConf();
				reader.ReadToFollowing("excluded"); //excluded files

				while (reader.ReadToFollowing("exclude"))
				{
					reader.MoveToFirstAttribute();
					if (reader.Name == "path")
					{
						tmpfileConf.setPathExclude(reader.Value);
					}
					else if (reader.Name == "name")
					{
						tmpfileConf.setNameExclude(reader.Value);
					}
				}

				reader.ReadToFollowing("included"); //included files
				while (reader.ReadToFollowing("include"))
				{
					reader.MoveToFirstAttribute();
					if (reader.Name == "path")
					{
						tmpfileConf.setPathInclude(reader.Value);
					}
					else if (reader.Name == "name")
					{
						tmpfileConf.setNameInclude(reader.Value);
					}
				}
				tmp.fileConfs = tmpfileConf;

				this.configList.Add(tmp);
			} while (readAll.ReadToFollowing("config"));

			//config file read
		}

		private bool isSameHash(string nowHash, string hashfilename)
		{
			string oldHash;
			try
			{
				oldHash = File.ReadAllText(hashfilename).Replace("\n", "");
			}
			catch
			{
				return false;
			}

			return nowHash == oldHash;
		}
		
		private string CalcMD5(string filename)
		{
			using (var md5 = MD5.Create())
			{
				try
				{
					using (var stream = File.OpenRead(filename))
					{
						var hash = md5.ComputeHash(stream);
						return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
					}
				}
				catch (Exception ex)
				{
					return "";
				}
			}
		}	 
	}
}
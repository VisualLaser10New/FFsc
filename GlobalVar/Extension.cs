using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFGlobalVar
{
	public static class Extension
	{
		public static bool StartsWith(this List<string> list, string str)
		{
			foreach (var _ in list.Where(el => el.StartsWith(str)).Select(el => new { }))
			{
				return true;
			}
			return false;
		}
	}
}

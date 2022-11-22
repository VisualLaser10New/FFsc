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
			foreach (var _ in list.Where(el => el.TrimEnd('\\').StartsWith(str.TrimEnd('\\'))).Select(el => new { }))
			{
				return true;
			}
			return false;
		}

		public static bool StartsWithList(this string str, List<string> list)
		{
			foreach (var _ in list.Where(el => str.TrimEnd('\\').StartsWith(el.TrimEnd('\\'))).Select(el => new { }))
			{
				return true;
			}
			return false;
		}

	}
}

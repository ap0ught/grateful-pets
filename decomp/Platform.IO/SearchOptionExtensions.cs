using System;
using System.IO;

namespace Platform.IO;

public static class SearchOptionExtensions
{
	public static System.IO.SearchOption ToSystemIOEquivalent(this SearchOption option)
	{
		return option switch
		{
			SearchOption.TopDirectoryOnly => System.IO.SearchOption.TopDirectoryOnly, 
			SearchOption.AllDirectories => System.IO.SearchOption.AllDirectories, 
			_ => throw new ArgumentOutOfRangeException("option", option, null), 
		};
	}
}

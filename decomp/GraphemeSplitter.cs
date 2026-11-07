using System;

public static class GraphemeSplitter
{
	public static ReadOnlySpan<char> GetNextPrintableSpan(ReadOnlySpan<char> In, ref int Index)
	{
		if (Index >= In.Length)
		{
			return default;
		}
		return In.Slice(Index++, 1);
	}
}

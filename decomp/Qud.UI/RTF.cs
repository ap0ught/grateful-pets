using ConsoleLib.Console;
using Cysharp.Text;
using XRL.UI;

namespace Qud.UI;

public static class RTF
{
	public static string FormatToRTF(string s, string opacity = "FF", int blockWrap = -1, bool stripFormatting = false)
	{
		if (Options.StripUIColorText | stripFormatting)
		{
			s = ColorUtility.StripFormatting(s);
		}
		Utf16ValueStringBuilder sb = ZString.CreateStringBuilder();
		try
		{
			if (blockWrap > 0)
			{
				s = BlockWrap(s, blockWrap, 5000);
			}
			Sidebar.FormatToRTF(s, ref sb, opacity);
			return sb.ToString();
		}
		finally
		{
			sb.Dispose();
		}
	}

	public static string BlockWrap(string s, int width, int maxLines)
	{
		return new TextBlock(s, width, maxLines).ToString();
	}
}

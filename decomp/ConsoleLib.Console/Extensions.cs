using System.Text;

namespace ConsoleLib.Console;

public static class Extensions
{
	public static StringBuilder AppendMarkupNode(this StringBuilder SB, string shader, string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SB;
		}
		return SB.Append("{{").Append(shader).Append("|")
			.Append(text)
			.Append("}}");
	}
}

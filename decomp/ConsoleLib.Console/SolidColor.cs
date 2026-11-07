using System.Text;
using XRL;

namespace ConsoleLib.Console;

public class SolidColor : IMarkupShader
{
	public char? Foreground;

	public char? Background;

	public char? LightTone;

	public char? DarkTone;

	public SolidColor(string Name, char? Foreground = 'y', char? Background = 'k', char? LightTone = null, char? DarkTone = null)
		: base(Name)
	{
		this.Foreground = Foreground;
		this.Background = Background;
		this.LightTone = LightTone;
		this.DarkTone = DarkTone;
	}

	public override char? GetForegroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
	{
		return Foreground;
	}

	public override char? GetBackgroundColor(char ch, int localPos, int localLen, int totalPos, int totalLen)
	{
		return Background;
	}

	public void AssembleCode(StringBuilder SB)
	{
		if (Foreground.HasValue)
		{
			SB.Append('&').Append(Foreground);
		}
		if (Background.HasValue)
		{
			SB.Append('^').Append(Background);
		}
	}

	public string AssembleCode()
	{
		StringBuilder stringBuilder = The.StringBuilder;
		AssembleCode(stringBuilder);
		return stringBuilder.ToString();
	}
}

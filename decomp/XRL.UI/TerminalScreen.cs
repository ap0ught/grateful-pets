using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.Language;

namespace XRL.UI;

public class TerminalScreen
{
	public string RenderedText;

	public string MainText;

	public List<string> Options = new List<string>();

	public List<int> OptionLines = new List<int>();

	public int HackOption = -1;

	public string HackText;

	public List<string> HackOptions;

	public string RenderedTextForModernUI => StringFormat.ClipText(MainText, 67, KeepNewlines: true);

	public int LicensesRemaining => Terminal.LicensesRemaining;

	public CyberneticsTerminal Terminal => CyberneticsTerminal.Instance;

	public virtual void Back()
	{
		Terminal.CurrentScreen = null;
	}

	public virtual void TextComplete()
	{
	}

	public virtual void Activate()
	{
	}

	protected virtual void OnUpdate()
	{
	}

	protected virtual void ClearOptions()
	{
		Options.Clear();
		HackOption = -1;
	}

	public void Update()
	{
		OnUpdate();
		if (Terminal.HackActive)
		{
			if (MainText != null && MainText != HackText && Terminal.LowLevelHack)
			{
				MainText = TextFilters.Leet(MainText);
				HackText = MainText;
			}
			bool flag = false;
			int i = 0;
			for (int count = Options.Count; i < count; i++)
			{
				if (i != HackOption && Options[i] != null && (HackOptions == null || i >= HackOptions.Count || HackOptions[i] != Options[i]))
				{
					if (Terminal.LowLevelHack)
					{
						Options[i] = TextFilters.Leet(Options[i]);
					}
					flag = true;
				}
			}
			if (flag)
			{
				HackOptions = new List<string>(Options);
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StringFormat.ClipText(MainText, 67, KeepNewlines: true)).Append("\n\n");
		int num = stringBuilder.ToString().Split('\n').Length;
		StringBuilder stringBuilder2 = new StringBuilder();
		int num2 = 65;
		for (int j = 0; j < Options.Count; j++)
		{
			if (Terminal.Selected == j)
			{
				stringBuilder.Append('>');
			}
			else
			{
				stringBuilder.Append(' ');
			}
			stringBuilder2.Clear();
			if (j == HackOption)
			{
				stringBuilder2.Append("{{R|CTRL-ENTER. ").Append(StringFormat.ClipText(Options[j], 60, KeepNewlines: true)).Append("}}");
				Markup.Transform(stringBuilder2);
			}
			else
			{
				stringBuilder2.Append((char)num2++).Append(". ").Append(StringFormat.ClipText("{{|" + Options[j] + "}}", 60, KeepNewlines: true));
			}
			stringBuilder2.Append('\n');
			stringBuilder.Append(stringBuilder2);
			OptionLines.Add(num);
			num += stringBuilder2.ToString().Count((char ch) => ch == '\n');
		}
		RenderedText = stringBuilder.ToString();
	}

	public virtual void BeforeRender(ScreenBuffer buffer, ref string footerText)
	{
	}
}

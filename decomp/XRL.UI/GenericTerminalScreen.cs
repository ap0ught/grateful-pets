using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;

namespace XRL.UI;

public class GenericTerminalScreen
{
	public string renderedText;

	public string mainText;

	public List<string> Options = new List<string>();

	public List<int> OptionLines = new List<int>();

	public string RenderedTextForModernUI => StringFormat.ClipText(mainText, 67, KeepNewlines: true);

	public GenericTerminal terminal => GenericTerminal.instance;

	public virtual void Back()
	{
		terminal.currentScreen = null;
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

	public void Update()
	{
		OnUpdate();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(StringFormat.ClipText("{{|" + mainText + "}}", 67, KeepNewlines: true)).Append("\n\n");
		int num = stringBuilder.ToString().Split('\n').Length;
		StringBuilder stringBuilder2 = new StringBuilder();
		for (int i = 0; i < Options.Count; i++)
		{
			if (terminal.nSelected == i)
			{
				stringBuilder.Append('>');
			}
			else
			{
				stringBuilder.Append(' ');
			}
			stringBuilder2.Length = 0;
			stringBuilder2.Append((char)(65 + i)).Append(". ").Append(StringFormat.ClipText("{{|" + Options[i] + "}}", 60, KeepNewlines: true))
				.Append("\n");
			stringBuilder.Append(stringBuilder2);
			OptionLines.Add(num);
			num += stringBuilder2.ToString().Count((char ch) => ch == '\n');
		}
		renderedText = stringBuilder.ToString();
	}

	public virtual void BeforeRender(ScreenBuffer buffer, ref string FooterText)
	{
		FooterText = "";
	}
}

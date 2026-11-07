using System;
using System.Collections.Generic;
using System.Xml;
using ConsoleLib.Console;
using Qud.UI;
using XRL.UI;
using XRL.World;

namespace XRL.Help;

[HasModSensitiveStaticCache]
public class XRLManual
{
	private string CurrentPage;

	private int ScrollPosition;

	private int IndexPosition;

	private TextConsole Console;

	private ScreenBuffer Buffer;

	public List<XRLManualPage> Page = new List<XRLManualPage>();

	public Dictionary<string, XRLManualPage> Pages = new Dictionary<string, XRLManualPage>();

	public XRLManual(TextConsole _Console)
	{
		Console = _Console;
		Buffer = ScreenBuffer.GetScrapBuffer1();
		Pages.Clear();
		foreach (XmlDataHelper item in DataManager.YieldXMLStreamsWithRoot("help"))
		{
			while (item.Read())
			{
				if (item.Name == "topic" && item.NodeType != XmlNodeType.EndElement)
				{
					string text = item["name"];
					XRLManualPage xRLManualPage = new XRLManualPage(item.ReadString())
					{
						Topic = text
					};
					Page.Add(xRLManualPage);
					Pages.Add(text, xRLManualPage);
				}
			}
		}
	}

	private void RenderPage(string PageName, int StartingLine)
	{
		int num = 1;
		if (StartingLine > Pages[PageName].Lines.Count - 23)
		{
			StartingLine = Pages[PageName].Lines.Count - 23;
		}
		if (StartingLine <= 0)
		{
			StartingLine = 0;
		}
		for (int i = StartingLine; i < Pages[PageName].Lines.Count; i++)
		{
			if (num >= 24)
			{
				break;
			}
			Buffer.Goto(2, num);
			Buffer.Write(Pages[PageName].LinesStripped[i]);
			num++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ &W" + PageName + "&y ]");
		Buffer.Goto(45, 24);
		Buffer.Write(" [&W5&y or &WEnter&y] Back to Index ");
		if (Pages[PageName].Lines.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)Pages[PageName].Lines.Count / 23.0);
			int num2 = (int)Math.Ceiling((double)(Pages[PageName].Lines.Count + 23) / 23.0);
			_ = 0;
			if (num2 <= 0)
			{
				num2 = 1;
			}
			int num3 = 23 / num2;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = (int)((double)(23 - num3) * ((double)StartingLine / (double)(Pages[PageName].Lines.Count - 23)));
			num4++;
			for (int k = num4; k < num4 + num3; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ColorUtility.Bright(7), 0);
			}
		}
	}

	private void RenderIndex(int ScrollPosition)
	{
		int num = ScrollPosition / 21 * 21;
		if (num > Pages.Keys.Count - 21)
		{
			num = Pages.Keys.Count - 21;
		}
		if (num <= 0)
		{
			num = 0;
		}
		List<string> list = new List<string>();
		foreach (string key in Pages.Keys)
		{
			list.Add(key);
		}
		int num2 = 2;
		for (int i = num; i < Pages.Keys.Count; i++)
		{
			if (num2 >= 23)
			{
				break;
			}
			Buffer.Goto(2, num2);
			if (i == ScrollPosition)
			{
				Buffer.Write("&W" + list[i] + "&k");
			}
			else
			{
				Buffer.Write(list[i]);
			}
			num2++;
		}
		Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		Buffer.Goto(2, 0);
		Buffer.Write("[ &WHelp Index&y ]");
		Buffer.Goto(45, 24);
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			Buffer.Write(" [&W" + ControlManager.getCommandInputDescription("Accept", mapGlyphs: false) + "&y] Select Topic ");
		}
		else
		{
			Buffer.Write(" [&W>&y or &WEnter&y] Select Topic ");
		}
		Buffer.Goto(15, 24);
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			Buffer.Write(" [&W" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "&y] Exit Help ");
		}
		else
		{
			Buffer.Write(" [&W5&y] Exit Help ");
		}
		if (list.Count > 23)
		{
			for (int j = 1; j < 24; j++)
			{
				Buffer.Goto(79, j);
				Buffer.Write(177, ColorUtility.Bright((ushort)0), 0);
			}
			_ = (int)Math.Ceiling((double)Pages.Keys.Count / 23.0);
			int num3 = (int)Math.Ceiling((double)(Pages.Keys.Count + 23) / 23.0);
			_ = 0;
			if (num3 <= 0)
			{
				num3 = 1;
			}
			int num4 = 23 / num3;
			if (num4 <= 0)
			{
				num4 = 1;
			}
			int num5 = (int)((double)(23 - num4) * ((double)num / (double)(Pages.Keys.Count - 23)));
			num5++;
			for (int k = num5; k < num5 + num4; k++)
			{
				Buffer.Goto(79, k);
				Buffer.Write(219, ColorUtility.Bright(7), 0);
			}
		}
	}

	public void ShowHelp(string DefaultPage)
	{
		if (Options.ModernUI)
		{
			try
			{
				GameManager.Instance.PushGameView("ModernHelp");
				_ = SingletonWindowBase<HelpScreen>.instance.HelpMenu(DefaultPage).Result;
				return;
			}
			catch (Exception x)
			{
				MetricsManager.LogError("Error showing modern help menu", x);
				return;
			}
			finally
			{
				GameManager.Instance.PopGameView();
			}
		}
		GameManager.Instance.PushGameView("Help");
		CurrentPage = DefaultPage;
		ScrollPosition = 0;
		while (true)
		{
			Event.ResetPool();
			Buffer.Clear();
			if (CurrentPage != "")
			{
				RenderPage(CurrentPage, ScrollPosition);
			}
			else
			{
				RenderIndex(IndexPosition);
			}
			Console.DrawBuffer(Buffer);
			Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
			if (CurrentPage == "")
			{
				if (keys == Keys.NumPad2)
				{
					IndexPosition++;
				}
				if (keys == Keys.Next || keys == Keys.Next || keys == Keys.NumPad3 || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next)
				{
					IndexPosition += 23;
				}
				if (keys == Keys.NumPad8)
				{
					IndexPosition--;
				}
				if (keys == Keys.Prior || keys == Keys.Back || keys == Keys.NumPad9 || Keyboard.RawCode == Keys.Back || Keyboard.RawCode == Keys.Prior)
				{
					IndexPosition -= 23;
				}
				if (keys == Keys.End)
				{
					IndexPosition = Pages.Keys.Count - 1;
				}
				if (keys == Keys.Home)
				{
					IndexPosition = 0;
				}
			}
			else
			{
				if (keys == Keys.NumPad2)
				{
					ScrollPosition++;
				}
				if (keys == Keys.Next || keys == Keys.Next || keys == Keys.NumPad3 || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next)
				{
					ScrollPosition += 23;
				}
				if (keys == Keys.NumPad8)
				{
					ScrollPosition--;
				}
				if (keys == Keys.Prior || keys == Keys.Back || keys == Keys.NumPad9 || Keyboard.RawCode == Keys.Back || Keyboard.RawCode == Keys.Prior)
				{
					ScrollPosition -= 23;
				}
				if (keys == Keys.End)
				{
					ScrollPosition = Pages[CurrentPage].Lines.Count - 23;
				}
				if (keys == Keys.Home)
				{
					ScrollPosition = 0;
				}
			}
			if (CurrentPage == "" && IndexPosition >= Pages.Keys.Count)
			{
				IndexPosition = Pages.Keys.Count - 1;
			}
			if (CurrentPage != "" && ScrollPosition > Pages[CurrentPage].Lines.Count - 23)
			{
				ScrollPosition = Pages[CurrentPage].Lines.Count - 23;
			}
			if (IndexPosition < 0)
			{
				IndexPosition = 0;
			}
			if (ScrollPosition < 0)
			{
				ScrollPosition = 0;
			}
			if ((keys == Keys.Escape || keys == Keys.NumPad5 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && CurrentPage == "")
			{
				break;
			}
			if ((keys == Keys.Escape || keys == Keys.NumPad5 || keys == Keys.Escape || keys == Keys.Enter || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && CurrentPage != "")
			{
				CurrentPage = "";
			}
			else
			{
				if ((keys != Keys.OemPeriod && keys != Keys.Enter && keys != Keys.Space) || !(CurrentPage == ""))
				{
					continue;
				}
				List<string> list = new List<string>();
				foreach (string key in Pages.Keys)
				{
					list.Add(key);
				}
				CurrentPage = list[IndexPosition];
				ScrollPosition = 0;
			}
		}
		GameManager.Instance.PopGameView();
	}
}

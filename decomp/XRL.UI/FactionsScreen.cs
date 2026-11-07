using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World;
using XRL.World.Effects;

namespace XRL.UI;

public class FactionsScreen : IScreen
{
	public static string FormatFactionReputation(string sFaction)
	{
		int valueOrDefault = (The.Game?.PlayerReputation?.Get(sFaction)).GetValueOrDefault();
		char color = Reputation.GetColor(valueOrDefault);
		return $"{{{{{color}|{$"{valueOrDefault,5}"}}}}}";
	}

	private static int WriteFaction(ScreenBuffer SB, GameObject GO, string sFaction, int bSelected, int x, int y)
	{
		Faction faction = Factions.Get(sFaction);
		if (!faction.Visible)
		{
			return 0;
		}
		if (bSelected == 1)
		{
			SB.Goto(x, y);
			SB.Write("> ");
		}
		else
		{
			SB.Goto(x + 2, y);
		}
		TextBlock textBlock = new TextBlock(faction.DisplayName, 30, 5);
		for (int i = 0; i < textBlock.Lines.Count; i++)
		{
			SB.Goto(x, y + i);
			if (bSelected == 1)
			{
				SB.Write("{{W|" + textBlock.Lines[i] + "}}");
			}
			else
			{
				SB.Write(textBlock.Lines[i]);
			}
		}
		if (bSelected == 2)
		{
			SB.Goto(x + 30, y);
			SB.Write("> ");
		}
		else
		{
			SB.Goto(x + 32, y);
		}
		SB.Write(FormatFactionReputation(sFaction));
		return textBlock.Lines.Count;
	}

	public string GetHeaderMessage()
	{
		if (The.Player != null && The.Player.HasEffect(typeof(WakingDream)))
		{
			return "{{C|The reputations of your former life are a mere memory and no longer relevant.}}";
		}
		return null;
	}

	public bool IsRelevant()
	{
		if (The.Player != null)
		{
			return !The.Player.HasEffect(typeof(WakingDream));
		}
		return false;
	}

	public static List<string> getFactionsByName()
	{
		List<string> list = new List<string>();
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible)
			{
				list.Add(item.Name);
			}
		}
		list.Sort(new FactionNameComparer());
		return list;
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Factions");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		List<string> factionsByName = new List<string>();
		int topRow = 0;
		int cursorRow = 0;
		int num = 0;
		foreach (Faction item in Factions.Loop())
		{
			if (item.Visible)
			{
				factionsByName.Add(item.Name);
			}
		}
		factionsByName.Sort(new FactionNameComparer());
		Keys keys = Keys.None;
		bool flag = false;
		TextBlock TB = null;
		int tbStart = 0;
		int num2 = 23;
		int nDisplayed = 18;
		int num3 = 0;
		int num4 = -1;
		int num5 = -1;
		Action SetUpTextBlock = () =>
		{
			TB = new TextBlock(Faction.GetRepPageDescription(factionsByName[cursorRow]), 28, 9999);
			tbStart = 0;
		};
		Action action = () =>
		{
			cursorRow = Math.Max(cursorRow - 1, 0);
			if (cursorRow == topRow - 1)
			{
				topRow = Math.Max(topRow - 1, 0);
			}
			SetUpTextBlock();
		};
		Action action2 = () =>
		{
			cursorRow = Math.Min(cursorRow + 1, factionsByName.Count - 1);
			if (cursorRow >= topRow + nDisplayed)
			{
				topRow = Math.Min(topRow + (1 + (18 - nDisplayed)), factionsByName.Count - 1);
			}
			SetUpTextBlock();
		};
		SetUpTextBlock();
		string text = "< {{W|7}} Equipment | Quests {{W|9}} >";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Equipment | Quests {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(35, 0);
			scrapBuffer.Write("[ {{W|Reputation}} ]");
			scrapBuffer.Goto(79 - ColorUtility.StripFormatting(text).Length, 24);
			scrapBuffer.Write(text);
			scrapBuffer.Goto(60, 0);
			scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "}} to exit ");
			int num6 = 3;
			string headerMessage = GetHeaderMessage();
			if (!headerMessage.IsNullOrEmpty())
			{
				TextBlock textBlock = new TextBlock(headerMessage, 73, 5);
				for (int num7 = 0; num7 < textBlock.Lines.Count; num7++)
				{
					scrapBuffer.WriteAt(3, num6++, textBlock.Lines[num7]);
				}
				num6++;
			}
			scrapBuffer.Goto(5, num6);
			scrapBuffer.Write("{{W|Faction}}");
			scrapBuffer.Goto(35, num6);
			scrapBuffer.Write("{{W|Reputation}}");
			num6 += 2;
			num2 = 23 - num6;
			for (int num8 = 0; num8 + tbStart < TB.Lines.Count && num8 < num2; num8++)
			{
				scrapBuffer.Goto(50, num6 + num8);
				scrapBuffer.Write(TB.Lines[num8 + tbStart]);
			}
			if (TB.Lines.Count > num2)
			{
				int num9 = (int)((float)num2 / (float)TB.Lines.Count * 23f);
				int num10 = (int)((float)tbStart / (float)TB.Lines.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num10, 79, 1 + num10 + num9, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			if (num == 2)
			{
				scrapBuffer.Goto(48, num6);
				scrapBuffer.Write("{{Y|>}}");
			}
			nDisplayed = 23 - num6;
			for (int num11 = topRow; num11 < topRow + nDisplayed && num11 < factionsByName.Count; num11++)
			{
				int num12 = ((num11 != cursorRow) ? WriteFaction(scrapBuffer, GO, factionsByName[num11], 0, 3, num6) : WriteFaction(scrapBuffer, GO, factionsByName[num11], 1 + num, 3, num6));
				num6 += num12;
				if (num12 > 0)
				{
					nDisplayed -= num12 - 1;
				}
				num3 = num11;
			}
			if (num4 != -1)
			{
				if (topRow + nDisplayed > num4 && cursorRow > 0)
				{
					action();
					continue;
				}
				num4 = -1;
			}
			if (num5 != -1)
			{
				if (cursorRow - nDisplayed < num5 && cursorRow < factionsByName.Count - 1)
				{
					action2();
					continue;
				}
				num5 = -1;
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			keys = Keyboard.getvk(MapDirectionToArrows: true);
			if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
			{
				GameManager.Instance.PopGameView();
				return ScreenReturn.Previous;
			}
			if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
			{
				GameManager.Instance.PopGameView();
				return ScreenReturn.Next;
			}
			switch (keys)
			{
			case Keys.Escape:
			case Keys.NumPad5:
				GameManager.Instance.PopGameView();
				return ScreenReturn.Exit;
			case Keys.Right:
			case Keys.NumPad6:
				num++;
				break;
			}
			if (keys == Keys.Left || keys == Keys.NumPad4)
			{
				num--;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (num > 2)
			{
				num = 2;
			}
			if (keys == Keys.Up || keys == Keys.NumPad8)
			{
				if (num < 2)
				{
					action();
				}
				else if (tbStart > 0)
				{
					tbStart--;
				}
			}
			if (keys == Keys.Down || keys == Keys.NumPad2)
			{
				if (num < 2)
				{
					action2();
				}
				else if (tbStart < TB.Lines.Count - num2)
				{
					tbStart++;
				}
			}
			if (keys == Keys.Prior)
			{
				if (num < 2)
				{
					if (cursorRow == topRow)
					{
						num4 = cursorRow;
					}
					else
					{
						cursorRow = topRow;
						SetUpTextBlock();
					}
				}
				else
				{
					tbStart = Math.Max(tbStart - num2, 0);
				}
			}
			if (keys == Keys.Next)
			{
				if (num < 2)
				{
					if (cursorRow == num3)
					{
						num5 = cursorRow;
					}
					else
					{
						cursorRow = num3;
						SetUpTextBlock();
					}
				}
				else
				{
					tbStart = Math.Max(Math.Min(tbStart + num2, TB.Lines.Count - num2), 0);
				}
			}
			if (keys == Keys.Enter || keys == Keys.Space)
			{
				if (num == 0)
				{
					factionsByName.Sort(new FactionNameComparer());
				}
				if (num == 1)
				{
					factionsByName.Sort(new FactionRepComparer());
				}
			}
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}

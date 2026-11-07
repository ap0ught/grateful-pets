using System.Collections.Generic;
using System.Linq;
using System.Text;
using ConsoleLib.Console;
using XRL.Core;
using XRL.World;

namespace XRL.UI;

public class QuestLog : IScreen
{
	public static List<string> GetLinesForQuest(Quest Q, bool IncludeTitle = true, bool Clip = true, int ClipWidth = 74)
	{
		List<string> list = new List<string>();
		if (IncludeTitle)
		{
			list.Add(Q.DisplayName);
			list.Add("");
		}
		foreach (QuestStep item in Q.StepsByID.Values.OrderBy((QuestStep x) => x.Ordinal))
		{
			if (item.Hidden)
			{
				continue;
			}
			StringBuilder stringBuilder = Event.NewStringBuilder();
			int MaxClippedWidth = (Clip ? ClipWidth : int.MaxValue);
			if (item.Failed)
			{
				stringBuilder.Append("{{red|X}} {{black|");
			}
			else if (item.Finished)
			{
				stringBuilder.Append("{{green|û}} {{white|");
			}
			else
			{
				stringBuilder.Append("{{white|ù ");
			}
			if (item.Optional)
			{
				stringBuilder.Append("Optional: ");
			}
			stringBuilder.Append(item.Name).Append("}}");
			List<string> list2 = StringFormat.ClipTextToArray(Event.FinalizeString(stringBuilder), Clip ? (ClipWidth - 3) : int.MaxValue, out MaxClippedWidth);
			for (int num = 0; num < list2.Count; num++)
			{
				if (num == 0)
				{
					list.Add("{{white|" + list2[num] + "}}");
				}
				if (num > 0)
				{
					list.Add("   {{y|" + list2[num] + "}}");
				}
			}
			if (!item.Finished || !item.Collapse)
			{
				foreach (string item2 in StringFormat.ClipTextToArray(item.Text, Clip ? (ClipWidth - 3) : int.MaxValue, out MaxClippedWidth, KeepNewlines: true))
				{
					list.Add("   {{y|" + item2 + "}}");
				}
			}
			list.Add("");
		}
		if (!string.IsNullOrEmpty(Q.BonusAtLevel))
		{
			foreach (string item3 in Q.BonusAtLevel.CachedCommaExpansion())
			{
				list.Add("  Bonus reward for completing this quest by level &C" + item3 + "&y.");
			}
		}
		return list;
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Quests");
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1(bLoadFromCurrent: true);
		bool flag = false;
		int num = 0;
		List<string> list = new List<string>();
		list.Add("");
		foreach (Quest value in XRLCore.Core.Game.Quests.Values)
		{
			if (!XRLCore.Core.Game.FinishedQuests.ContainsKey(value.ID))
			{
				list.AddRange(GetLinesForQuest(value));
				list.Add("");
			}
		}
		string text = "< {{W|7}} Factions | Journal {{W|9}} >";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Factions | Journal {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
		}
		while (!flag)
		{
			Event.ResetPool();
			scrapBuffer.Clear();
			scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			scrapBuffer.Goto(37, 0);
			scrapBuffer.Write("[ &WQuests&y ]");
			scrapBuffer.Goto(79 - ColorUtility.StripFormatting(text).Length, 24);
			scrapBuffer.Write(text);
			scrapBuffer.Goto(60, 0);
			scrapBuffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Cancel", mapGlyphs: false) + "}} to exit ");
			int num2 = 1;
			for (int i = 0; i < 23; i++)
			{
				scrapBuffer.Goto(4, num2 + i);
				if (num + i < list.Count)
				{
					scrapBuffer.Write(list[num + i]);
				}
			}
			if (list.Count > 23)
			{
				int num3 = (int)(23f / (float)list.Count * 23f);
				int num4 = (int)((float)num / (float)list.Count * 23f);
				scrapBuffer.Fill(79, 1, 79, 23, 177, ColorUtility.MakeColor(ColorUtility.Bright(TextColor.Black), TextColor.Black));
				scrapBuffer.Fill(79, 1 + num4, 79, 1 + num4 + num3, 177, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			else
			{
				switch (keys)
				{
				case Keys.NumPad2:
					num++;
					break;
				case Keys.Prior:
					num -= 23;
					break;
				case Keys.Next:
					num += 23;
					break;
				}
			}
			if (num > list.Count - 23)
			{
				num = list.Count - 23;
			}
			if (num < 0)
			{
				num = 0;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5)
			{
				GameManager.Instance.PopGameView();
				return ScreenReturn.Exit;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
			{
				GameManager.Instance.PopGameView();
				return ScreenReturn.Previous;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
			{
				GameManager.Instance.PopGameView();
				return ScreenReturn.Next;
			}
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}

using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.World;

namespace XRL.UI;

public class FullscreenPicker
{
	public const int RESULT_CANCEL = -1;

	public const int RESULT_NOOP = -2;

	private int nSelected;

	private TextConsole C;

	private ScreenBuffer SB;

	private List<FullscreenPickerEntry> Entries;

	public FullscreenPicker(ScreenBuffer SB, TextConsole C, List<FullscreenPickerEntry> Entries)
	{
		this.SB = SB;
		this.Entries = Entries;
		this.C = C;
	}

	public int DoInput()
	{
		Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
		if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Select:"))
		{
			nSelected = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
			return nSelected;
		}
		if (keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Random"))
		{
			nSelected = Stat.Random(0, Entries.Count - 1);
		}
		if (keys == Keys.Enter)
		{
			keys = Keys.Space;
		}
		if (keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
		{
			return -1;
		}
		if (keys >= Keys.A && keys <= Keys.Z)
		{
			int num = (int)(keys - 65);
			if (num <= Entries.Count - 1)
			{
				return num;
			}
		}
		switch (keys)
		{
		case Keys.Space:
			return nSelected;
		case Keys.MouseEvent:
			if (Keyboard.CurrentMouseEvent.Event.StartsWith("Pick:"))
			{
				return int.Parse(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
			}
			break;
		}
		if (keys == Keys.NumPad8 && nSelected > 0)
		{
			nSelected--;
		}
		if (keys == Keys.NumPad2 && nSelected < Entries.Count - 1)
		{
			nSelected++;
		}
		return -2;
	}

	public void RenderFrame()
	{
		Event.ResetPool();
		GameManager.Instance.ClearRegions();
		SB.Clear();
		SB.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		SB.Goto(20, 0);
		SB.Write("[ &WCharacter Creation &y- &YChoose Genotype&y ]");
		SB.Goto(60, 24);
		SB.Write(" &WESC&y to go back ");
		int num = 2;
		for (int i = 0; i < Entries.Count; i++)
		{
			int y = num;
			if (nSelected == i)
			{
				SB.Goto(1, num);
				SB.Write("&Y> ");
			}
			SB.Goto(3, num);
			SB.Write("&W" + (char)(65 + i) + "&y) " + Entries[i].Title);
			num++;
			for (int j = 0; j < Entries[i].Lines.Count; j++)
			{
				SB.Goto(3, num);
				SB.Write(Entries[i].Lines[j]);
				num++;
			}
			GameManager.Instance.AddRegion(3, y, 40, num, "Pick:" + i, null, "Select:" + i);
			num++;
		}
		SB.Goto(3, 24);
		SB.Write(" press &WF1&y for a random selection ");
		GameManager.Instance.AddRegion(3, 24, 40, 24, "Random");
		C.DrawBuffer(SB);
	}
}

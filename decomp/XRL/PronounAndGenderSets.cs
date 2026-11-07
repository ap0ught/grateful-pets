using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World;

namespace XRL;

public static class PronounAndGenderSets
{
	private static TextConsole _Console;

	public static void AssignGender(GameObject Player, out Gender ChosenGender, out Gender CustomGender, string Spec = null)
	{
		if (!string.IsNullOrEmpty(Spec))
		{
			if (Spec.Contains(","))
			{
				ChosenGender = Gender.Get(Spec.Split(',').GetRandomElement());
			}
			else
			{
				ChosenGender = Gender.Get(Spec);
			}
			CustomGender = null;
		}
		else if (Gender.EnableGeneration && Stat.Random(1, 10) == 1)
		{
			ChosenGender = (CustomGender = Gender.Generate());
		}
		else
		{
			ChosenGender = Gender.GetAnyGenericPersonalSingular();
			CustomGender = null;
		}
	}

	public static void AssignGender(GameObject Player)
	{
		AssignGender(Player, out var ChosenGender, out var _);
		Player.SetGender(ChosenGender.Register());
	}

	public static void AssignPronounSet(GameObject Player, out PronounSet ChosenPronounSet, out PronounSet CustomPronounSet, bool Always = false)
	{
		if (Always || Stat.Random(1, 10) == 1)
		{
			if (PronounSet.EnableGeneration && Stat.Random(1, 10) == 1)
			{
				ChosenPronounSet = (CustomPronounSet = PronounSet.Generate());
				return;
			}
			ChosenPronounSet = PronounSet.GetAnyGenericPersonalSingular();
			CustomPronounSet = null;
		}
		else
		{
			ChosenPronounSet = null;
			CustomPronounSet = null;
		}
	}

	public static void AssignPronounSet(GameObject Player)
	{
		AssignPronounSet(Player, out var ChosenPronounSet, out var _);
		if (ChosenPronounSet == null)
		{
			Player.ClearPronounSet();
		}
		else
		{
			Player.SetPronounSet(ChosenPronounSet.Register());
		}
	}

	public static void AssignGenderAndPronounSet(GameObject Player, out Gender ChosenGender, out PronounSet ChosenPronounSet, out Gender CustomGender, out PronounSet CustomPronounSet)
	{
		AssignGender(Player, out ChosenGender, out CustomGender);
		AssignPronounSet(Player, out ChosenPronounSet, out CustomPronounSet);
	}

	public static void AssignGenderAndPronounSet(GameObject Player)
	{
		AssignGender(Player);
		AssignPronounSet(Player);
	}

	public static void ShowChangePronounSet(GameObject Player)
	{
		if (_Console == null)
		{
			_Console = Popup._TextConsole;
		}
		ScreenBuffer screenBuffer = ScreenBuffer.create(80, 25);
		GameManager.Instance.PushGameView("ChangePronounSet");
		Keyboard.ClearMouseEvents();
		int num = 0;
		PronounSet pronounSet = Player.GetPronounSet();
		PronounSet ChosenPronounSet = pronounSet;
		PronounSet pronounSet2 = null;
		List<PronounSet> list = PronounSet.GetAllGenericPersonal();
		if (ChosenPronounSet != null && !list.Contains(ChosenPronounSet))
		{
			list = new List<PronounSet>(list);
			list.Add(ChosenPronounSet);
		}
		if (ChosenPronounSet != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] == ChosenPronounSet)
				{
					num = i + 1;
					break;
				}
			}
		}
		while (true)
		{
			screenBuffer.Clear();
			GameManager.Instance.ClearRegions();
			PronounSet pronounSet3 = null;
			if (num != 0)
			{
				pronounSet3 = ((num > list.Count) ? pronounSet2 : list[num - 1]);
			}
			else
			{
				pronounSet3 = null;
			}
			screenBuffer.Goto(3, 2);
			screenBuffer.Write("&WPronoun Set&y");
			int num2 = 4;
			screenBuffer.Goto(3, num2);
			if (ChosenPronounSet == null)
			{
				screenBuffer.Write("&k^Y<from gender>&y&k");
			}
			else
			{
				screenBuffer.Write("<from gender>");
			}
			GameManager.Instance.AddRegion(3, num2, 30, num2, "Select::0");
			num2++;
			for (int j = 0; j < list.Count; j++)
			{
				screenBuffer.Goto(3, num2);
				string text = list[j].GetShortName();
				if (list[j] == ChosenPronounSet)
				{
					text = "{{&k^Y|" + text + "}}";
				}
				screenBuffer.Write(text);
				GameManager.Instance.AddRegion(3, num2, 30, num2, "Select:" + list[j].Name + ":" + (j + 1));
				num2++;
			}
			if (pronounSet2 != null)
			{
				screenBuffer.Goto(3, num2);
				string text2 = pronounSet2.GetShortName();
				if (pronounSet2 == ChosenPronounSet)
				{
					text2 = "{{&k^Y|" + text2 + "}}";
				}
				screenBuffer.Write(text2);
				GameManager.Instance.AddRegion(3, num2, 30, num2, "Select:" + pronounSet2.Name + ":" + (list.Count + 1));
				num2++;
			}
			screenBuffer.Goto(3, ++num2);
			if (pronounSet3 == ChosenPronounSet)
			{
				if (PronounSet.EnableGeneration)
				{
					screenBuffer.Write("&Wenter &yto copy and customize");
				}
			}
			else
			{
				screenBuffer.Write("&Wenter &yto select");
			}
			screenBuffer.Goto(3, ++num2);
			screenBuffer.Write("&WF1 &yfor random");
			if (PronounSet.EnableGeneration)
			{
				screenBuffer.Goto(3, ++num2);
				screenBuffer.Write("&WF2 &yto generate");
			}
			screenBuffer.Goto(2, 4 + num);
			screenBuffer.Write("&Y>");
			TextBlock textBlock = new TextBlock((pronounSet3 != null) ? pronounSet3.GetBasicSummary() : Player.GetGender().GetBasicSummary(), 45, 22);
			screenBuffer.SingleBox(33, 23 - textBlock.Lines.Count, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			for (int k = 0; k < textBlock.Lines.Count; k++)
			{
				screenBuffer.Goto(34, 24 - textBlock.Lines.Count + k);
				screenBuffer.Write(textBlock.Lines[k].PadRight(45));
			}
			screenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			screenBuffer.Goto(28, 0);
			screenBuffer.Write("[ &YChange Pronoun Set&y ]");
			screenBuffer.Goto(3, 24);
			if (ChosenPronounSet != pronounSet)
			{
				screenBuffer.Write(" press {{hotkey|Space}} to keep changes and continue ");
			}
			else
			{
				screenBuffer.Write(" press {{hotkey|Space}} to return ");
			}
			_Console.DrawBuffer(screenBuffer);
			Event.ResetPool();
			Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
			if ((keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && (ChosenPronounSet == pronounSet || Popup.ShowYesNo("Are you sure you want to discard your changes?") == DialogResult.Yes))
			{
				GameManager.Instance.PopGameView();
				return;
			}
			if (keys == Keys.Space || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "SilentContinue"))
			{
				break;
			}
			if (keys == Keys.Enter)
			{
				if (ChosenPronounSet == pronounSet3)
				{
					if (PronounSet.EnableGeneration)
					{
						PronounSet pronounSet4 = ((ChosenPronounSet != null) ? new PronounSet(ChosenPronounSet) : new PronounSet(Player.GetGender()));
						if (pronounSet4.CustomizeAsync().Result)
						{
							pronounSet2 = PronounSet.GetIfExists(pronounSet4.Name) ?? pronounSet4;
							ChosenPronounSet = pronounSet2;
							num = list.Count + 1;
							for (int l = 0; l < list.Count; l++)
							{
								if (list[l] == pronounSet2)
								{
									num = l + 1;
									pronounSet2 = null;
									break;
								}
							}
						}
					}
				}
				else
				{
					ChosenPronounSet = pronounSet3;
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Select:"))
			{
				string[] array = Keyboard.CurrentMouseEvent.Event.Split(':');
				ChosenPronounSet = ((!(array[1] == "")) ? PronounSet.Get(array[1]) : null);
				num = Convert.ToInt32(array[2]);
			}
			if (keys == Keys.F1)
			{
				AssignPronounSet(Player, out ChosenPronounSet, out var CustomPronounSet, Always: true);
				if (CustomPronounSet != null)
				{
					pronounSet2 = CustomPronounSet;
				}
				num = 0;
				if (pronounSet2 == ChosenPronounSet)
				{
					num = list.Count;
				}
				else
				{
					for (int m = 0; m < list.Count; m++)
					{
						if (list[m] == ChosenPronounSet)
						{
							num = m + 1;
							break;
						}
					}
				}
			}
			if (keys == Keys.F2 && PronounSet.EnableGeneration)
			{
				ChosenPronounSet = (pronounSet2 = PronounSet.Generate());
				num = list.Count + 1;
			}
			if (keys == Keys.NumPad8 && num > 0)
			{
				num--;
			}
			if (keys == Keys.NumPad2 && (num < list.Count || (pronounSet2 != null && num < list.Count + 1)))
			{
				num++;
			}
		}
		if (ChosenPronounSet == null)
		{
			Player.ClearPronounSet();
		}
		else if (pronounSet == null || ChosenPronounSet.Name != pronounSet.Name)
		{
			Player.SetPronounSet(ChosenPronounSet.Register());
			Player.ModIntProperty("PronounSetTick", 1);
		}
		GameManager.Instance.PopGameView();
	}

	public static void ShowPickGenderAndPronounSet(GameObject Player, string Spec = null)
	{
		ScreenBuffer screenBuffer = ScreenBuffer.create(80, 25);
		GameManager.Instance.PushGameView("PickGenderAndPronounSet");
		Keyboard.ClearMouseEvents();
		int num = 0;
		int num2 = 0;
		Gender gender = Player.GetGender();
		Gender gender2 = gender;
		Gender ChosenGender = gender;
		Gender gender3 = null;
		PronounSet pronounSet = Player.GetPronounSet();
		PronounSet ChosenPronounSet = pronounSet;
		PronounSet pronounSet2 = null;
		List<PronounSet> allGenericPersonal = PronounSet.GetAllGenericPersonal();
		bool flag = false;
		bool enableGeneration = PronounSet.EnableGeneration;
		List<Gender> list;
		if (!string.IsNullOrEmpty(Spec))
		{
			string[] array = Spec.Split(',');
			list = new List<Gender>(array.Length);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (text == "generate")
				{
					flag = Gender.EnableGeneration;
				}
				else
				{
					list.Add(Gender.Get(text));
				}
			}
			if (list.Count == 0)
			{
				MetricsManager.LogError("no usable base genders");
				Popup.ShowFail("Cannot proceed, no usable base genders");
				GameManager.Instance.PopGameView();
				return;
			}
		}
		else
		{
			list = Gender.GetAllGenericPersonalSingular();
			flag = Gender.EnableGeneration;
		}
		if (gender != null)
		{
			num2 = list.IndexOf(gender);
			if (num2 == -1)
			{
				gender3 = gender;
				num2 = list.Count;
			}
		}
		if (pronounSet != null && !allGenericPersonal.Contains(pronounSet))
		{
			pronounSet2 = pronounSet;
		}
		while (true)
		{
			screenBuffer.Clear();
			GameManager.Instance.ClearRegions();
			Gender gender4 = null;
			PronounSet pronounSet3 = null;
			switch (num)
			{
			case 0:
				gender4 = ((num2 == list.Count) ? gender3 : list[num2]);
				if (gender2 == null)
				{
					gender2 = gender4;
				}
				if (ChosenGender == null)
				{
					ChosenGender = gender2;
				}
				break;
			case 1:
				if (num2 != 0)
				{
					pronounSet3 = ((num2 > allGenericPersonal.Count) ? pronounSet2 : allGenericPersonal[num2 - 1]);
				}
				else
				{
					pronounSet3 = null;
				}
				break;
			}
			screenBuffer.Goto(3, 2);
			screenBuffer.Write("&WGender&y");
			if (PronounSet.EnableSelection)
			{
				screenBuffer.Goto(42, 2);
				screenBuffer.Write("&WPronoun Set&y");
			}
			int num3 = 4;
			int num4 = 4;
			for (int j = 0; j < list.Count; j++)
			{
				screenBuffer.Goto(3, num3);
				string text2 = Grammar.MakeTitleCase(list[j].Name);
				if (list[j] == ChosenGender)
				{
					text2 = "&k^Y" + text2 + "&y^k";
				}
				screenBuffer.Write(text2);
				GameManager.Instance.AddRegion(3, num3, 30, num3, "Select:" + list[j].Name + ":0:" + j);
				num3++;
			}
			if (gender3 != null)
			{
				screenBuffer.Goto(3, num3);
				string text3 = Grammar.MakeTitleCase(gender3.Name);
				if (gender3 == ChosenGender)
				{
					text3 = "&k^Y" + text3 + "&y^k";
				}
				screenBuffer.Write(text3);
				GameManager.Instance.AddRegion(3, num3, 30, num3, "Select:" + gender3.Name + ":0:" + list.Count);
				num3++;
			}
			if (PronounSet.EnableSelection)
			{
				screenBuffer.Goto(42, num4);
				if (ChosenPronounSet == null)
				{
					screenBuffer.Write("&k^Y<from gender>&y&k");
				}
				else
				{
					screenBuffer.Write("<from gender>");
				}
				GameManager.Instance.AddRegion(42, num4, 70, num4, "Select::1:0");
				num4++;
				for (int k = 0; k < allGenericPersonal.Count; k++)
				{
					screenBuffer.Goto(42, num4);
					string text4 = allGenericPersonal[k].GetShortName();
					if (allGenericPersonal[k] == ChosenPronounSet)
					{
						text4 = "{{&k^Y|" + text4 + "}}";
					}
					screenBuffer.Write(text4);
					GameManager.Instance.AddRegion(3, num4, 30, num4, "Select:" + allGenericPersonal[k].Name + ":1:" + (k + 1));
					num4++;
				}
				if (pronounSet2 != null)
				{
					screenBuffer.Goto(42, num4);
					string text5 = pronounSet2.GetShortName();
					if (pronounSet2 == ChosenPronounSet)
					{
						text5 = "{{&k^Y|" + text5 + "}}";
					}
					screenBuffer.Write(text5);
					GameManager.Instance.AddRegion(3, num3, 30, num3, "Select:" + pronounSet2.Name + ":1:" + (allGenericPersonal.Count + 1));
					num4++;
				}
			}
			switch (num)
			{
			case 0:
				if (gender4 == ChosenGender)
				{
					if (flag)
					{
						screenBuffer.Goto(3, ++num3);
						screenBuffer.Write("&Wenter &yto copy and customize");
					}
				}
				else
				{
					screenBuffer.Goto(3, ++num3);
					screenBuffer.Write("&Wenter &yto select");
				}
				screenBuffer.Goto(3, ++num3);
				screenBuffer.Write(PronounSet.EnableSelection ? "&WF1 &yfor both random" : "&WF1 &yfor random");
				if (flag)
				{
					screenBuffer.Goto(3, ++num3);
					screenBuffer.Write("&WF2 &yto generate gender");
				}
				break;
			case 1:
				if (pronounSet3 == ChosenPronounSet)
				{
					if (enableGeneration)
					{
						screenBuffer.Goto(42, ++num4);
						screenBuffer.Write("&Wenter &yto copy and customize");
					}
				}
				else
				{
					screenBuffer.Goto(42, ++num4);
					screenBuffer.Write("&Wenter &yto select");
				}
				screenBuffer.Goto(42, ++num4);
				screenBuffer.Write("&WF1 &yfor both random");
				if (enableGeneration)
				{
					screenBuffer.Goto(42, ++num4);
					screenBuffer.Write("&WF2 &yto generate pronoun set");
				}
				break;
			}
			switch (num)
			{
			case 0:
			{
				screenBuffer.Goto(2, 4 + num2);
				screenBuffer.Write("&Y>");
				TextBlock textBlock2 = new TextBlock(gender4.GetBasicSummary(), 45, 22);
				screenBuffer.SingleBox(33, 23 - textBlock2.Lines.Count, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				for (int m = 0; m < textBlock2.Lines.Count; m++)
				{
					screenBuffer.Goto(34, 24 - textBlock2.Lines.Count + m);
					screenBuffer.Write(textBlock2.Lines[m].PadRight(45));
				}
				break;
			}
			case 1:
			{
				screenBuffer.Goto(41, 4 + num2);
				screenBuffer.Write("{{Y|>}}");
				TextBlock textBlock = new TextBlock(pronounSet3?.GetBasicSummary() ?? ChosenGender.GetBasicSummary(), 38, 22);
				screenBuffer.SingleBox(0, 23 - textBlock.Lines.Count, 39, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				for (int l = 0; l < textBlock.Lines.Count; l++)
				{
					screenBuffer.Goto(1, 24 - textBlock.Lines.Count + l);
					screenBuffer.Write(textBlock.Lines[l].PadRight(38));
				}
				break;
			}
			}
			screenBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
			if (PronounSet.EnableSelection)
			{
				screenBuffer.Goto(13, 0);
				screenBuffer.Write("[ &WCharacter Creation &y- &YChoose Gender and Pronoun Set&y ]");
			}
			else
			{
				screenBuffer.Goto(21, 0);
				screenBuffer.Write("[ &WCharacter Creation &y- &YChoose Gender&y ]");
			}
			screenBuffer.Goto(3, 24);
			if (ChosenGender != gender || ChosenPronounSet != pronounSet)
			{
				screenBuffer.Write(" press {{hotkey|Space}} to keep changes and continue ");
			}
			else
			{
				screenBuffer.Write(" press {{hotkey|Space}} to return ");
			}
			_Console.DrawBuffer(screenBuffer);
			Event.ResetPool();
			Keys keys = Keyboard.getvk(MapDirectionToArrows: true);
			if ((keys == Keys.Escape || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")) && ((ChosenGender == gender && ChosenPronounSet == pronounSet) || Popup.ShowYesNo("Are you sure you want to discard your changes?") == DialogResult.Yes))
			{
				GameManager.Instance.PopGameView();
				return;
			}
			if (keys == Keys.Space || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "SilentContinue"))
			{
				break;
			}
			if (keys == Keys.Enter)
			{
				switch (num)
				{
				case 0:
					if (ChosenGender == gender4)
					{
						if (flag)
						{
							Gender gender5 = new Gender(ChosenGender);
							if (gender5.CustomizeAsync().Result)
							{
								ChosenGender = (gender3 = gender5);
								num2 = list.Count;
							}
						}
					}
					else
					{
						ChosenGender = gender4;
					}
					break;
				case 1:
					if (ChosenPronounSet == pronounSet3)
					{
						PronounSet pronounSet4 = ((ChosenPronounSet != null) ? new PronounSet(ChosenPronounSet) : new PronounSet(ChosenGender));
						if (!pronounSet4.CustomizeAsync().Result)
						{
							break;
						}
						pronounSet2 = PronounSet.GetIfExists(pronounSet4.Name) ?? pronounSet4;
						ChosenPronounSet = pronounSet2;
						num2 = allGenericPersonal.Count + 1;
						for (int n = 0; n < allGenericPersonal.Count; n++)
						{
							if (allGenericPersonal[n] == pronounSet2)
							{
								num2 = n + 1;
								pronounSet2 = null;
								break;
							}
						}
					}
					else
					{
						ChosenPronounSet = pronounSet3;
					}
					break;
				}
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Select:"))
			{
				num = 0;
				string[] array3 = Keyboard.CurrentMouseEvent.Event.Split(':');
				if (array3[2] == "0")
				{
					ChosenGender = Gender.Get(array3[1]);
					num2 = Convert.ToInt32(array3[3]);
				}
				else if (array3[2] == "1")
				{
					ChosenPronounSet = ((!(array3[1] == "")) ? PronounSet.Get(array3[1]) : null);
					num2 = Convert.ToInt32(array3[3]);
				}
			}
			if (keys == Keys.F1)
			{
				if (PronounSet.EnableSelection)
				{
					AssignGenderAndPronounSet(Player, out ChosenGender, out ChosenPronounSet, out var CustomGender, out var CustomPronounSet);
					if (CustomGender != null)
					{
						gender3 = CustomGender;
					}
					if (CustomPronounSet != null)
					{
						pronounSet2 = CustomPronounSet;
					}
				}
				else
				{
					AssignGender(Player, out ChosenGender, out var CustomGender2);
					if (CustomGender2 != null)
					{
						gender3 = CustomGender2;
					}
				}
				num2 = 0;
				switch (num)
				{
				case 0:
				{
					if (gender3 == ChosenGender)
					{
						num2 = list.Count;
						break;
					}
					for (int num6 = 0; num6 < list.Count; num6++)
					{
						if (list[num6] == ChosenGender)
						{
							num2 = num6;
							break;
						}
					}
					break;
				}
				case 1:
				{
					if (pronounSet2 == ChosenPronounSet)
					{
						num2 = allGenericPersonal.Count;
						break;
					}
					for (int num5 = 0; num5 < allGenericPersonal.Count; num5++)
					{
						if (allGenericPersonal[num5] == ChosenPronounSet)
						{
							num2 = num5;
							break;
						}
					}
					break;
				}
				}
			}
			if (keys == Keys.F2)
			{
				switch (num)
				{
				case 0:
					if (flag)
					{
						ChosenGender = (gender3 = Gender.Generate());
						num2 = list.Count;
					}
					break;
				case 1:
					if (enableGeneration)
					{
						ChosenPronounSet = (pronounSet2 = PronounSet.Generate());
						num2 = allGenericPersonal.Count + 1;
					}
					break;
				}
			}
			if (keys == Keys.NumPad8 && num2 > 0)
			{
				num2--;
			}
			if (keys == Keys.NumPad2)
			{
				switch (num)
				{
				case 0:
					if (num2 < list.Count - 1 || (gender3 != null && num2 < list.Count))
					{
						num2++;
					}
					break;
				case 1:
					if (num2 < allGenericPersonal.Count || (pronounSet2 != null && num2 < allGenericPersonal.Count + 1))
					{
						num2++;
					}
					break;
				}
			}
			if (keys == Keys.NumPad4 && PronounSet.EnableSelection)
			{
				if (num == 1)
				{
					num = 0;
				}
				num2 = Math.Min(num2, (gender3 == null) ? (list.Count - 1) : list.Count);
			}
			if (keys == Keys.NumPad6 && PronounSet.EnableSelection)
			{
				if (num == 0)
				{
					num = 1;
				}
				num2 = Math.Min(num2, (pronounSet2 == null) ? allGenericPersonal.Count : (allGenericPersonal.Count + 1));
			}
		}
		Player.SetGender(ChosenGender.Register());
		if (ChosenPronounSet == null)
		{
			Player.ClearPronounSet();
		}
		else
		{
			Player.SetPronounSet(ChosenPronounSet.Register());
		}
		GameManager.Instance.PopGameView();
	}
}

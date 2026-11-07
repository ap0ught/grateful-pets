using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.Wish;
using XRL.World;

namespace XRL;

/// This class is not used in the base game.
[Serializable]
[HasWishCommand]
public abstract class SifrahGame
{
	public enum GameOutcome
	{
		CriticalFailure,
		Failure,
		PartialSuccess,
		Success,
		CriticalSuccess
	}

	public static bool Installed;

	public const int DEFAULT_SLOTS = 5;

	public const int RIGHT_SIDE_WIDTH = 30;

	public string Description;

	public string CorrectTokenSound;

	public string IncorrectTokenSound;

	public string CriticalFailureSound = "Sounds/Creatures/VO/sfx_robot_generic_vo_die";

	public string FailureSound = "Sounds/Abilities/sfx_ability_sunderMind_dig_fail";

	public string PartialSuccessSound = "sfx_light_refract";

	public string SuccessSound = "Sounds/Abilities/sfx_ability_sunderMind_dig_success";

	public string CriticalSuccessSound = "Sounds/Grenade/sfx_grenade_flashbang_explode";

	public int MaxTurns;

	public int Turn;

	public int CorrectTokenSoundDelay = 150;

	public int IncorrectTokenSoundDelay = 150;

	public float CorrectTokenSoundVolume = 1f;

	public float IncorrectTokenSoundVolume = 1f;

	public float CriticalFailureSoundVolume = 1f;

	public float FailureSoundVolume = 1f;

	public float PartialSuccessSoundVolume = 1f;

	public float SuccessSoundVolume = 1f;

	public float CriticalSuccessSoundVolume = 1f;

	public bool ShowContextObjectIcon = true;

	public bool HadNoPause;

	public bool Finished;

	public List<SifrahSlot> Slots;

	public List<SifrahToken> Tokens;

	public int SlotSelected;

	public int Powerup;

	public List<SifrahEffectChance> EffectChances;

	private static string[] status = new string[6];

	private static StringBuilder KeySB = new StringBuilder();

	public bool Solved
	{
		get
		{
			if (Slots != null)
			{
				int i = 0;
				for (int count = Slots.Count; i < count; i++)
				{
					if (!Slots[i].Solved)
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public int SlotsSolved
	{
		get
		{
			int num = 0;
			if (Slots != null)
			{
				int i = 0;
				for (int count = Slots.Count; i < count; i++)
				{
					if (Slots[i].Solved)
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	public int PercentSolved
	{
		get
		{
			if (Slots == null || Slots.Count == 0)
			{
				return 0;
			}
			return SlotsSolved * 100 / Slots.Count;
		}
	}

	public int TotalChoicesNeededForTurn
	{
		get
		{
			if (Slots == null || Slots.Count == 0)
			{
				return 0;
			}
			return Slots.Count - SlotsSolved;
		}
	}

	public int ChoicesMadeForTurn
	{
		get
		{
			int num = 0;
			if (Slots != null || Slots.Count == 0)
			{
				int i = 0;
				for (int count = Slots.Count; i < count; i++)
				{
					if (Slots[i].CurrentMove != -1 && !Slots[i].Solved)
					{
						num++;
					}
				}
			}
			return num;
		}
	}

	public bool TurnReady => ChoicesMadeForTurn >= TotalChoicesNeededForTurn;

	public virtual string[] GetStatus()
	{
		CalculateOutcomeChances(out var Success, out var Failure, out var PartialSuccess, out var CriticalSuccess, out var CriticalFailure);
		status[0] = "Chances:";
		status[1] = " {{g|Success             " + $"{Success,3}%";
		status[2] = " {{G|Exceptional success " + $"{CriticalSuccess,3}%";
		status[3] = " {{W|Partial success     " + $"{PartialSuccess,3}%";
		status[4] = " {{K|Failure             " + $"{Failure,3}%";
		status[5] = " {{R|Critical failure    " + $"{CriticalFailure,3}%";
		return status;
	}

	public int GetTimesChosen(SifrahToken Token, SifrahSlot Except = null)
	{
		int num = 0;
		foreach (SifrahSlot slot in Slots)
		{
			if (slot.CurrentMove != -1 && Tokens[slot.CurrentMove] == Token && slot != Except)
			{
				num++;
			}
		}
		return num;
	}

	public void Render(GameObject ContextObject)
	{
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer1();
		scrapBuffer.Clear();
		scrapBuffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
		int num = 48 / Slots.Count;
		int num2 = (num - 1) / 2;
		int i = 0;
		int num3;
		int num4;
		for (int count = Slots.Count; i < count; i++)
		{
			num3 = 1 + i * num + num2;
			num4 = 4;
			SifrahSlot sifrahSlot = Slots[i];
			scrapBuffer.Goto(num3, num4);
			sifrahSlot.WriteTo(scrapBuffer);
			sifrahSlot.X = num3;
			sifrahSlot.Y = num4;
			num4 += 2;
			int num5 = Math.Max(0, sifrahSlot.Moves.Count - 12);
			int j = num5;
			for (int count2 = sifrahSlot.Moves.Count; j < count2; j++)
			{
				scrapBuffer.Goto(num3, num4 + j - num5);
				int num6 = sifrahSlot.Moves[j];
				SifrahToken sifrahToken = Tokens[num6];
				if (num6 == sifrahSlot.Token)
				{
					sifrahToken.WriteTo(scrapBuffer, "&G", 'g');
				}
				else if (sifrahToken.EliminatedAt == -1 || sifrahToken.EliminatedAt > j)
				{
					sifrahToken.WriteTo(scrapBuffer, "&W", 'w');
				}
				else
				{
					sifrahToken.WriteTo(scrapBuffer, "&R", 'r');
				}
			}
			num4 = 22;
			scrapBuffer.Goto(num3, num4);
			if (!sifrahSlot.Solved || SlotSelected == i)
			{
				if (sifrahSlot.CurrentMove == -1)
				{
					if (SlotSelected == i)
					{
						scrapBuffer.Write("&k^Y?");
					}
					else
					{
						scrapBuffer.Write("&Y?");
					}
				}
				else
				{
					SifrahToken sifrahToken2 = Tokens[sifrahSlot.CurrentMove];
					if (SlotSelected == i)
					{
						sifrahToken2.WriteHighlightedTo(scrapBuffer);
					}
					else
					{
						sifrahToken2.WriteTo(scrapBuffer);
					}
					sifrahToken2.X = num3;
					sifrahToken2.Y = num4;
				}
			}
			GameManager.Instance.AddRegion(num3, 4, num3, 22, "Slot:" + i);
		}
		num3 = 49;
		num4 = 2;
		IRenderable renderable = ((ShowContextObjectIcon && ContextObject != null) ? ContextObject.RenderForUI() : null);
		if (!Description.IsNullOrEmpty())
		{
			int num7 = 30;
			if (renderable != null)
			{
				num7 -= 3;
			}
			foreach (string line in new TextBlock(Description, num7, 6).Lines)
			{
				scrapBuffer.Goto(num3, num4++);
				scrapBuffer.Write(line);
			}
			num4++;
		}
		if (renderable != null)
		{
			scrapBuffer.Goto(76, 2);
			scrapBuffer.Write("   ");
			scrapBuffer.Goto(77, 2);
			scrapBuffer.Write(renderable);
		}
		scrapBuffer.Goto(num3, num4++);
		scrapBuffer.Write("Turn: " + Turn + "/" + MaxTurns);
		scrapBuffer.Goto(num3, num4++);
		scrapBuffer.Write("Solved: " + SlotsSolved + "/" + Slots.Count);
		if (Finished)
		{
			scrapBuffer.WriteAt(num3, num4++, Solved ? "{{G|Completed!}}" : "{{R|Incomplete.}}");
			scrapBuffer.WriteAt(24, 24, " press {{W|Space}} or {{W|Enter}} to continue ");
		}
		else
		{
			scrapBuffer.WriteAt(num3, num4++, "Choices made: " + ChoicesMadeForTurn + "/" + TotalChoicesNeededForTurn);
			if (TurnReady)
			{
				if (Options.PrereleaseInputManager)
				{
					scrapBuffer.WriteAt(27, 24, " press {{W|Z}} to complete turn ");
				}
				else
				{
					scrapBuffer.WriteAt(25, 24, " press {{W|Space}} to complete turn ");
				}
			}
			else
			{
				scrapBuffer.WriteAt(2, 24, " [{{W|\u001b\u001a}}-change slot] ");
				scrapBuffer.WriteAt(19, 24, " [{{W|Enter}}-choose option] ");
				scrapBuffer.WriteAt(46, 24, " [{{W|Space}}-complete turn] ");
				scrapBuffer.WriteAt(68, 24, " [{{W|?}}-help] ");
			}
		}
		string[] array = GetStatus();
		if (array != null && array.Length != 0)
		{
			num4++;
			string[] array2 = array;
			foreach (string s in array2)
			{
				scrapBuffer.WriteAt(num3, num4++, s);
			}
		}
		if (GetInsight())
		{
			num4++;
			scrapBuffer.WriteAt(num3, num4++, "{{W|I}} to use insight");
		}
		scrapBuffer.Draw();
	}

	public void ProcessTurn(GameObject ContextObject)
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			SifrahSlot sifrahSlot = Slots[i];
			if (!sifrahSlot.Solved)
			{
				int currentMove = sifrahSlot.CurrentMove;
				if (currentMove != -1)
				{
					sifrahSlot.Moves.Add(currentMove);
					Powerup += Tokens[currentMove].GetPowerup(this, sifrahSlot, ContextObject);
					Tokens[currentMove].UseToken(this, sifrahSlot, ContextObject);
				}
			}
		}
		int j = 0;
		for (int count2 = Slots.Count; j < count2; j++)
		{
			SifrahSlot sifrahSlot2 = Slots[j];
			if (sifrahSlot2.Solved)
			{
				continue;
			}
			int currentMove2 = sifrahSlot2.CurrentMove;
			if (currentMove2 == -1 || currentMove2 == sifrahSlot2.Token)
			{
				continue;
			}
			SifrahToken sifrahToken = Tokens[currentMove2];
			if (sifrahToken.EliminatedAt != -1)
			{
				continue;
			}
			bool flag = false;
			int k = 0;
			for (int count3 = Slots.Count; k < count3; k++)
			{
				if (k != j && !Slots[k].Solved && Slots[k].Token == currentMove2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				sifrahToken.EliminatedAt = Turn - 1;
			}
		}
		int num = 0;
		int l = 0;
		for (int count4 = Slots.Count; l < count4; l++)
		{
			SifrahSlot slot = Slots[l];
			int dy = Math.Max(0, slot.Moves.Count - 12) + slot.Moves.Count + 1;
			if (slot.Solved)
			{
				if (slot.SolvedOnTurn != Turn || CorrectTokenSound.IsNullOrEmpty() || !Options.Sound)
				{
					continue;
				}
				if (num > 0)
				{
					Task.Delay(num).ContinueWith((Task t) =>
					{
						SoundManager.PlaySound(CorrectTokenSound, 0f, CorrectTokenSoundVolume);
						CombatJuice.punch(slot.GetLocation(0, dy), slot.GetLocation(0, dy - 1));
					});
				}
				else
				{
					SoundManager.PlaySound(CorrectTokenSound, 0f, CorrectTokenSoundVolume);
					CombatJuice.punch(slot.GetLocation(0, dy), slot.GetLocation(0, dy - 1));
				}
				num += CorrectTokenSoundDelay;
				continue;
			}
			if (slot.CurrentMove != -1 && !IncorrectTokenSound.IsNullOrEmpty() && Options.Sound)
			{
				if (num > 0)
				{
					Task.Delay(num).ContinueWith((Task t) =>
					{
						SoundManager.PlaySound(IncorrectTokenSound, 0f, IncorrectTokenSoundVolume);
						CombatJuice.punch(slot.GetLocation(0, dy), slot.GetLocation(0, dy + 1));
					});
				}
				else
				{
					SoundManager.PlaySound(IncorrectTokenSound, 0f, IncorrectTokenSoundVolume);
					CombatJuice.punch(slot.GetLocation(0, dy), slot.GetLocation(0, dy + 1));
				}
				num += IncorrectTokenSoundDelay;
			}
			slot.CurrentMove = -1;
		}
		int num2 = 0;
		for (int count5 = Tokens.Count; num2 < count5; num2++)
		{
			SifrahToken sifrahToken2 = Tokens[num2];
			sifrahToken2.UsabilityCheckedThisTurn = false;
			sifrahToken2.DisabledThisTurn = false;
		}
		if (Solved || Turn >= MaxTurns)
		{
			Finished = true;
			SlotSelected = -1;
		}
		else
		{
			Turn++;
			SlotSelected = MoveNeeded();
		}
	}

	public int MoveNeeded()
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			SifrahSlot sifrahSlot = Slots[i];
			if (!sifrahSlot.Solved && sifrahSlot.CurrentMove == -1)
			{
				return i;
			}
		}
		return -1;
	}

	private void MoveRight(bool CheckCurrentMove = false)
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			SlotSelected++;
			if (SlotSelected >= Slots.Count)
			{
				SlotSelected = 0;
			}
			if (!Slots[SlotSelected].Solved && (!CheckCurrentMove || Slots[SlotSelected].CurrentMove == -1))
			{
				return;
			}
		}
		if (CheckCurrentMove)
		{
			MoveRight();
		}
	}

	private void MoveLeft(bool CheckCurrentMove = false)
	{
		int i = 0;
		for (int count = Slots.Count; i < count; i++)
		{
			SlotSelected--;
			if (SlotSelected < 0)
			{
				SlotSelected = Slots.Count - 1;
			}
			if (!Slots[SlotSelected].Solved && (!CheckCurrentMove || Slots[SlotSelected].CurrentMove == -1))
			{
				return;
			}
		}
		if (CheckCurrentMove)
		{
			MoveLeft();
		}
	}

	public bool AnyUsableTokens(GameObject ContextObject)
	{
		int i = 0;
		for (int count = Tokens.Count; i < count; i++)
		{
			if (!Tokens[i].Eliminated && !Tokens[i].GetDisabled(this, null, ContextObject))
			{
				return true;
			}
		}
		return false;
	}

	public bool MakeMoveForSlot(int SlotNumber, GameObject ContextObject)
	{
		SifrahSlot sifrahSlot = Slots[SlotNumber];
		if (sifrahSlot.Solved)
		{
			Popup.ShowFail("You have already chosen the correct option for " + sifrahSlot.Description + ".");
			return false;
		}
		while (true)
		{
			string[] array = new string[Tokens.Count];
			char[] array2 = new char[Tokens.Count];
			IRenderable[] array3 = new IRenderable[Tokens.Count];
			List<int> list = null;
			char c = 'a';
			int num = sifrahSlot.CurrentMove;
			int i = 0;
			for (int count = Tokens.Count; i < count; i++)
			{
				SifrahToken sifrahToken = Tokens[i];
				string text = sifrahToken.GetDescription(this, sifrahSlot, ContextObject);
				bool flag = false;
				if (sifrahToken.Eliminated)
				{
					text += " [eliminated]";
					flag = true;
				}
				else if (sifrahToken.GetDisabled(this, sifrahSlot, ContextObject))
				{
					flag = true;
					if (sifrahToken.DisabledThisTurn)
					{
						text += " [disabled this turn]";
					}
				}
				else if (num == -1)
				{
					num = i;
				}
				if (flag)
				{
					text = "{{K|" + ColorUtility.StripFormatting(text) + "}}";
					array3[i] = Tokens[i].GetAlternate("&K", 'K');
				}
				else
				{
					array3[i] = Tokens[i];
				}
				array[i] = text;
				if (c <= 'z')
				{
					array2[i] = c++;
				}
				else
				{
					array2[i] = ' ';
				}
				if (sifrahToken.Eliminated)
				{
					if (list == null)
					{
						list = new List<int>();
					}
					list.Add(i);
				}
			}
			int num2 = Popup.PickOption("Use which option for " + sifrahSlot.Description + "?", null, "", "Sounds/UI/ui_notification", array, array2, array3, null, null, null, null, 0, 60, num, -1, AllowEscape: true);
			if (num2 < 0)
			{
				break;
			}
			SifrahToken sifrahToken2 = Tokens[num2];
			if (list != null && list.Contains(num2))
			{
				Popup.ShowFail("You have already eliminated " + sifrahToken2.Description + " as a possibility.");
			}
			else if (sifrahToken2.CheckTokenUse(this, sifrahSlot, ContextObject))
			{
				if (!sifrahToken2.DisabledThisTurn)
				{
					sifrahSlot.CurrentMove = num2;
					SlotSelected = SlotNumber;
					MoveRight(CheckCurrentMove: true);
					break;
				}
				Popup.ShowFail("Choosing " + sifrahToken2.Description + " is disabled for this turn.");
			}
		}
		return true;
	}

	public static void ShowHelp()
	{
		BookUI.ShowBookByID("Sifrah");
	}

	public virtual bool CheckIncompleteTurn(GameObject ContextObject)
	{
		return Popup.ShowYesNo("You haven't selected an option for every slot. Are you sure you want to complete the turn?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) == DialogResult.Yes;
	}

	public virtual bool CheckEarlyExit(GameObject ContextObject)
	{
		return Popup.ShowYesNo("You aren't finished! Are you sure you want to abort the process?", "Sounds/UI/ui_notification", AllowEscape: true, DialogResult.No) == DialogResult.Yes;
	}

	public virtual bool CheckOutOfOptions(GameObject ContextObject)
	{
		return true;
	}

	public virtual void Finish(GameObject ContextObject)
	{
	}

	public void Play(GameObject ContextObject = null)
	{
		if (Finished)
		{
			Finish(ContextObject);
			return;
		}
		HadNoPause = CombatJuiceManager.NoPause;
		if (!HadNoPause)
		{
			CombatJuiceManager.NoPause = true;
		}
		Loading.SetHideLoadStatus(hidden: true);
		ScreenBuffer scrapBuffer = ScreenBuffer.GetScrapBuffer2();
		scrapBuffer.Copy(TextConsole.CurrentBuffer);
		GameManager.Instance.PushGameView("Sifrah");
		Turn = 1;
		SlotSelected = 0;
		int num = -1;
		bool flag = false;
		Keyboard.ClearInput();
		while (!flag)
		{
			GameManager.Instance.ClearRegions();
			Render(ContextObject);
			Keys keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (keys == Keys.Right || keys == Keys.NumPad6)
			{
				MoveRight();
			}
			else if (keys == Keys.Left || keys == Keys.NumPad4)
			{
				MoveLeft();
			}
			else if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:SifrahEnter" && Slots != null && SlotSelected >= 0 && SlotSelected < Slots.Count)
			{
				num = SlotSelected;
			}
			else if ((keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:SifrahSpace") || (keys == Keys.Z && Options.PrereleaseInputManager))
			{
				if (TurnReady || !AnyUsableTokens(ContextObject) || CheckIncompleteTurn(ContextObject))
				{
					ProcessTurn(ContextObject);
					if (Finished)
					{
						flag = true;
						Render(ContextObject);
						do
						{
							keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
						}
						while (keys != Keys.Space && keys != Keys.Enter && keys != Keys.Escape);
					}
				}
			}
			else if (keys == Keys.Escape)
			{
				if (Finished || CheckEarlyExit(ContextObject))
				{
					flag = true;
				}
			}
			else if (keys >= Keys.D0 && keys <= Keys.D9 && Slots != null)
			{
				int num2 = (int)((keys == Keys.D0) ? Keys.Tab : (keys - 49));
				if (num2 >= 0 && num2 < Slots.Count)
				{
					num = num2;
				}
			}
			else if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event.StartsWith("Slot:"))
			{
				int num3 = Convert.ToInt32(Keyboard.CurrentMouseEvent.Event.Split(':')[1]);
				if (num3 >= 0 && num3 < Slots.Count)
				{
					num = num3;
				}
			}
			else
			{
				switch (keys)
				{
				case Keys.F1:
				case Keys.OemQuestion | Keys.Shift:
					ShowHelp();
					break;
				case Keys.I:
					if (GetInsight())
					{
						UseInsight(ContextObject);
					}
					break;
				}
			}
			if (num != -1)
			{
				if (AnyUsableTokens(ContextObject))
				{
					MakeMoveForSlot(num, ContextObject);
				}
				else if (CheckOutOfOptions(ContextObject))
				{
					flag = true;
					Finished = true;
					Render(ContextObject);
				}
				num = -1;
				Keyboard.ClearInput();
			}
		}
		Finish(ContextObject);
		Keyboard.ClearInput();
		GameManager.Instance.ClearRegions();
		GameManager.Instance.PopGameView();
		TextConsole.CurrentBuffer.Copy(scrapBuffer);
		Loading.SetHideLoadStatus(hidden: false);
		ProcessEffectChances();
		if (!HadNoPause)
		{
			CombatJuiceManager.NoPause = false;
		}
	}

	public static int PrioritySort(SifrahPrioritizable a, SifrahPrioritizable b)
	{
		int num = a.GetPriority().CompareTo(b.GetPriority());
		if (num != 0)
		{
			return -num;
		}
		int num2 = a.GetTiebreakerPriority().CompareTo(b.GetTiebreakerPriority());
		if (num2 != 0)
		{
			return -num2;
		}
		return 0;
	}

	protected void AssignPossibleTokens(List<SifrahPrioritizableToken> possibleTokens, List<SifrahToken> tokens, int need, int numTokens)
	{
		if (need >= possibleTokens.Count)
		{
			int i = 0;
			for (int count = possibleTokens.Count; i < count; i++)
			{
				tokens.Add(possibleTokens[i]);
			}
			return;
		}
		List<SifrahPrioritizableToken> list = new List<SifrahPrioritizableToken>(possibleTokens);
		list.Sort(PrioritySort);
		list.RemoveRange(need, list.Count - need);
		int j = 0;
		for (int count2 = possibleTokens.Count; j < count2; j++)
		{
			if (tokens.Count >= numTokens)
			{
				break;
			}
			if (list.Contains(possibleTokens[j]))
			{
				tokens.Add(possibleTokens[j]);
			}
		}
	}

	public abstract string GetSifrahCategory();

	public void UseInsight(GameObject ContextObject)
	{
		List<SifrahToken> list = null;
		bool flag = false;
		int i = 0;
		for (int count = Tokens.Count; i < count; i++)
		{
			SifrahToken sifrahToken = Tokens[i];
			if (sifrahToken.Eliminated)
			{
				flag = true;
				continue;
			}
			bool flag2 = false;
			int j = 0;
			for (int count2 = Slots.Count; j < count2; j++)
			{
				if (Slots[j].Token == i)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				if (list == null)
				{
					list = new List<SifrahToken>();
				}
				list.Add(sifrahToken);
				sifrahToken.EliminatedAt = Turn;
			}
		}
		if (list != null && list.Count > 0)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append("You determine these options to not be valid responses to any requirement:\n");
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				stringBuilder.Append('\n').Append(list[k].GetDescription(this, null, ContextObject));
			}
			Popup.Show(stringBuilder.ToString(), null, "Sounds/UI/ui_notification", CopyScrap: true, Capitalize: true, DimBackground: true, LogMessage: false);
			SetInsight(Value: false);
		}
		else if (flag)
		{
			Popup.ShowFail("All options which are not correct responses to any requirement have already been eliminated.");
		}
		else
		{
			Popup.ShowFail("All options are correct responses to some requirement.");
		}
	}

	public static void AwardInsight(string Category, string Description)
	{
		if (!GetInsight(Category))
		{
			if (The.Player?.CurrentCell != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("You have gained insight into ").Append(Description).Append(". In a future Sifrah task of this kind, you can use this insight to determine which of your ")
					.Append("game options are not correct for any requirement. This will expend your insight, unless there ")
					.Append("are no such options.");
				Popup.Show(stringBuilder.ToString());
			}
			SetInsight(Category, Value: true);
		}
	}

	public void AwardInsight(string Description)
	{
		AwardInsight(GetSifrahCategory(), Description);
	}

	public static string GetInsightKey(string Category)
	{
		return "Sifrah_Insight_" + Category;
	}

	public static bool GetInsight(string Category)
	{
		return The.Game.GetBooleanGameState(GetInsightKey(Category));
	}

	public bool GetInsight()
	{
		return GetInsight(GetSifrahCategory());
	}

	public static void SetInsight(string Category, bool Value)
	{
		The.Game.SetBooleanGameState(GetInsightKey(Category), Value);
	}

	public void SetInsight(bool Value)
	{
		SetInsight(GetSifrahCategory(), Value);
	}

	public abstract void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure);

	public GameOutcome DetermineOutcome(int Roll, int Success, int Failure, int PartialSuccess, int CriticalSuccess, int CriticalFailure)
	{
		if (Roll <= CriticalFailure)
		{
			return GameOutcome.CriticalFailure;
		}
		if (Roll <= CriticalFailure + Failure)
		{
			return GameOutcome.Failure;
		}
		if (Roll <= CriticalFailure + Failure + PartialSuccess)
		{
			return GameOutcome.PartialSuccess;
		}
		if (Roll <= CriticalFailure + Failure + PartialSuccess + Success)
		{
			return GameOutcome.Success;
		}
		return GameOutcome.CriticalSuccess;
	}

	public GameOutcome DetermineOutcome(int Success, int Failure, int PartialSuccess, int CriticalSuccess, int CriticalFailure)
	{
		return DetermineOutcome(Stat.Random(1, 100), Success, Failure, PartialSuccess, CriticalSuccess, CriticalFailure);
	}

	public GameOutcome DetermineOutcome()
	{
		CalculateOutcomeChances(out var Success, out var Failure, out var PartialSuccess, out var CriticalSuccess, out var CriticalFailure);
		return DetermineOutcome(Success, Failure, PartialSuccess, CriticalSuccess, CriticalFailure);
	}

	public GameOutcome DetermineOutcome(int Roll)
	{
		CalculateOutcomeChances(out var Success, out var Failure, out var PartialSuccess, out var CriticalSuccess, out var CriticalFailure);
		return DetermineOutcome(Roll, Success, Failure, PartialSuccess, CriticalSuccess, CriticalFailure);
	}

	public void PlayOutcomeSound(GameOutcome Outcome)
	{
		if (Options.Sound)
		{
			string text = null;
			float volume = 1f;
			switch (Outcome)
			{
			case GameOutcome.CriticalFailure:
				text = CriticalFailureSound;
				volume = CriticalFailureSoundVolume;
				break;
			case GameOutcome.Failure:
				text = FailureSound;
				volume = FailureSoundVolume;
				break;
			case GameOutcome.PartialSuccess:
				text = PartialSuccessSound;
				volume = PartialSuccessSoundVolume;
				break;
			case GameOutcome.Success:
				text = SuccessSound;
				volume = SuccessSoundVolume;
				break;
			case GameOutcome.CriticalSuccess:
				text = CriticalSuccessSound;
				volume = CriticalSuccessSoundVolume;
				break;
			}
			if (!text.IsNullOrEmpty())
			{
				SoundManager.PlaySound(text, 0f, volume);
			}
		}
	}

	protected static void RecordOutcome(string GameType, GameOutcome Outcome, int Slots, int Tokens, bool? AsMaster = null, bool? Mastered = null)
	{
		IncrementGameCount(GameType);
		IncrementGameCount(GameType, Outcome);
		int? slots = Slots;
		int? tokens = Tokens;
		IncrementGameCount(GameType, null, slots, tokens);
		IncrementGameCount(GameType, Outcome, Slots, Tokens);
		if (AsMaster.HasValue || Mastered.HasValue)
		{
			bool? asMaster = AsMaster;
			bool? mastered = Mastered;
			IncrementGameCount(GameType, null, null, null, asMaster, mastered);
			mastered = null;
			asMaster = Mastered;
			IncrementGameCount(GameType, null, null, null, mastered, asMaster);
			asMaster = AsMaster;
			IncrementGameCount(GameType, null, null, null, asMaster);
			GameOutcome? outcome = Outcome;
			mastered = AsMaster;
			asMaster = Mastered;
			IncrementGameCount(GameType, outcome, null, null, mastered, asMaster);
			GameOutcome? outcome2 = Outcome;
			asMaster = null;
			mastered = Mastered;
			IncrementGameCount(GameType, outcome2, null, null, asMaster, mastered);
			GameOutcome? outcome3 = Outcome;
			mastered = AsMaster;
			IncrementGameCount(GameType, outcome3, null, null, mastered);
			tokens = Slots;
			slots = Tokens;
			asMaster = AsMaster;
			mastered = Mastered;
			IncrementGameCount(GameType, null, tokens, slots, asMaster, mastered);
			slots = Slots;
			tokens = Tokens;
			mastered = null;
			asMaster = Mastered;
			IncrementGameCount(GameType, null, slots, tokens, mastered, asMaster);
			tokens = Slots;
			slots = Tokens;
			asMaster = AsMaster;
			IncrementGameCount(GameType, null, tokens, slots, asMaster);
			IncrementGameCount(GameType, Outcome, Slots, Tokens, AsMaster, Mastered);
			IncrementGameCount(GameType, Outcome, Slots, Tokens, null, Mastered);
			IncrementGameCount(GameType, Outcome, Slots, Tokens, AsMaster);
		}
	}

	protected static void RecordOutcome(SifrahGame Game, GameOutcome Outcome, int Slots, int Tokens, bool AsMaster = false, bool Mastered = false)
	{
		RecordOutcome(GetGameType(Game), Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	public static string GetGameType(Type Type)
	{
		return Type.Name;
	}

	public static string GetGameType(SifrahGame Game)
	{
		return GetGameType(Game.GetType());
	}

	public string GetGameType()
	{
		return GetGameType(this);
	}

	private static string GetGameCountKey(string GameType, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		KeySB.Clear().Append("Sifrah_").Append(GameType);
		if (Outcome.HasValue)
		{
			KeySB.Append('_').Append(Outcome.ToString());
		}
		else
		{
			KeySB.Append("_AnyOutcome");
		}
		if (Mastered.HasValue)
		{
			if (Mastered == true)
			{
				KeySB.Append("_Mastered");
			}
			else
			{
				KeySB.Append("_Unmastered");
			}
		}
		else
		{
			KeySB.Append("_BothMasteredAndUnmastered");
		}
		if (AsMaster.HasValue)
		{
			if (AsMaster == true)
			{
				KeySB.Append("_AsMaster");
			}
			else
			{
				KeySB.Append("_NotAsMaster");
			}
		}
		else
		{
			KeySB.Append("_BothAsMasterAndNot");
		}
		if (Slots.HasValue && Tokens.HasValue)
		{
			KeySB.Append("_Type_").Append(Slots).Append('_')
				.Append(Tokens);
		}
		else
		{
			KeySB.Append("_AnyType");
		}
		return KeySB.ToString();
	}

	private static string GetGameCountKey(Type Type, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return GetGameCountKey(GetGameType(Type), Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	private static string GetGameCountKey(SifrahGame Game, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return GetGameCountKey(GetGameType(Game), Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	public static int GetGameCount(string GameType, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return The.Game.GetIntGameState(GetGameCountKey(GameType, Outcome, Slots, Tokens, AsMaster, Mastered));
	}

	public static int GetGameCount(Type Type, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return GetGameCount(GetGameType(Type), Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	public static int GetGameCount(SifrahGame Game, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return GetGameCount(GetGameType(Game), Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	public int GetGameCount(GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		return GetGameCount(this, Outcome, Slots, Tokens, AsMaster, Mastered);
	}

	private static void IncrementGameCount(string GameType, GameOutcome? Outcome = null, int? Slots = null, int? Tokens = null, bool? AsMaster = null, bool? Mastered = null)
	{
		The.Game.ModIntGameState(GetGameCountKey(GameType, Outcome, Slots, Tokens, AsMaster, Mastered), 1);
	}

	public void AddEffectChance(string EffectName, int Chance, string Duration = null, string DisplayName = null, bool Stack = false, bool Force = false, Effect EffectInstance = null)
	{
		if (EffectChances == null)
		{
			EffectChances = new List<SifrahEffectChance>();
		}
		SifrahEffectChance item = new SifrahEffectChance(EffectName, Chance, Duration, DisplayName, Stack, Force, EffectInstance);
		EffectChances.Add(item);
	}

	public void ProcessEffectChances()
	{
		if (EffectChances == null)
		{
			return;
		}
		List<string> list = null;
		foreach (SifrahEffectChance effectChance in EffectChances)
		{
			if ((!effectChance.Stack && list != null && list.Contains(effectChance.EffectName)) || !effectChance.Chance.in100())
			{
				continue;
			}
			effectChance.Apply(The.Player);
			if (!effectChance.Stack)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(effectChance.EffectName);
			}
		}
	}
}

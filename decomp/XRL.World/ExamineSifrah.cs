using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class ExamineSifrah : TinkeringSifrah
{
	public int Complexity;

	public int Difficulty;

	public int Understanding;

	public int InspectRating;

	public bool Abort;

	public bool AsMaster;

	public bool Bypass;

	public bool InterfaceExitRequested;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("investigating gearworks", "Items/sw_gears2.bmp", "é", "&c", 'c'),
		new SifrahSlotConfiguration("tracing spark vines", "Items/sw_copper_wire.bmp", "í", "&W", 'G'),
		new SifrahSlotConfiguration("divining humour flows", "Items/sw_sparktick_plasma.bmp", "÷", "&Y", 'y'),
		new SifrahSlotConfiguration("investigating glow modules", "Items/sw_lens.bmp", "\u001f", "&B", 'b'),
		new SifrahSlotConfiguration("interpreting spirit messages", "Items/sw_computer.bmp", "Ñ", "&c", 'G'),
		new SifrahSlotConfiguration("meditating on the beyond", "Items/sw_wind_turbine_3.bmp", "ì", "&m", 'K')
	};

	public ExamineSifrah(GameObject ContextObject, int Complexity, int Difficulty, int Understanding, int InspectRating)
	{
		Description = "Examining " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
		this.Complexity = Complexity;
		this.Difficulty = Difficulty;
		this.Understanding = Understanding;
		this.InspectRating = InspectRating;
		int num = 3 + Complexity + Difficulty;
		int num2 = 4;
		if (Complexity >= 3)
		{
			num2++;
		}
		if (Complexity >= 7)
		{
			num2++;
			num += 4;
		}
		if (num2 > slotConfigs.Count)
		{
			num2 = slotConfigs.Count;
		}
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		int SecondaryBonus = 0;
		int num3 = GetTinkeringBonusEvent.GetFor(The.Player, ContextObject, "Examine", MaxTurns, 0, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable: true, ForSifrah: true);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		MaxTurns += num3;
		if (SecondaryBonus != 0)
		{
			InspectRating += SecondaryBonus;
			this.InspectRating = InspectRating;
		}
		List<SifrahToken> list = new List<SifrahToken>(num);
		if (list.Count < num && (Complexity < 3 || InspectRating + Understanding > 20 + Complexity * 3) && !The.Player.HasPart<Myopia>())
		{
			list.Add(new TinkeringSifrahTokenVisualInspection());
		}
		if (list.Count < num && (Complexity < 4 || InspectRating + Understanding > 10 + Complexity * 2))
		{
			list.Add(new TinkeringSifrahTokenPhysicalManipulation());
		}
		if (list.Count < num)
		{
			Scanning.Scan scan = Scanning.GetScanTypeFor(ContextObject);
			if (scan == Scanning.Scan.Structure)
			{
				scan = Scanning.Scan.Tech;
			}
			if (Scanning.HasScanningFor(The.Player, scan))
			{
				list.Add(new TinkeringSifrahTokenScanning(scan));
				MaxTurns++;
			}
		}
		if ((list.Count < num) & PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("read psychic impressions of past usage"));
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new TinkeringSifrahTokenTenfoldPathBin());
		}
		if (list.Count < num && The.Player.HasPart<Telekinesis>() && (Complexity < 5 || InspectRating + Understanding > 8 + Complexity * 2))
		{
			list.Add(new TinkeringSifrahTokenTelekinesis());
		}
		int num4 = num - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Complexity < 5 || InspectRating + Understanding > 5 + Complexity * 3)
			{
				list2.Add(new TinkeringSifrahTokenToolkit());
			}
			list2.Add(new TinkeringSifrahTokenComputePower(1 + Complexity * 2 + Difficulty * 3));
			list2.Add(new TinkeringSifrahTokenCharge(Math.Max(50 * (Complexity + Difficulty), 50)));
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Complexity)
				{
					break;
				}
				list2.Add(new TinkeringSifrahTokenBit(bitType));
			}
			list2.Add(new TinkeringSifrahTokenLiquid("oil"));
			list2.Add(new TinkeringSifrahTokenCopperWire());
			AssignPossibleTokens(list2, list, num4, num);
		}
		if (num > list.Count)
		{
			num = list.Count;
		}
		List<SifrahSlot> slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num2, num);
		Slots = slots;
		Tokens = list;
		if (!AnyUsableTokens(ContextObject))
		{
			Popup.ShowFail("You have no usable options to employ for examining " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of success. You can remedy this situation by improving your Intelligence and tinkering skills, or by obtaining items useful for tinkering.");
			Finished = true;
			Abort = true;
			return;
		}
		bool? asMaster = true;
		int? slots2 = Slots.Count;
		int? tokens = Tokens.Count;
		if (GetGameCount(null, slots2, tokens, asMaster) > 0)
		{
			AsMaster = true;
		}
		else
		{
			tokens = Slots.Count;
			slots2 = Tokens.Count;
			int gameCount = GetGameCount(null, tokens, slots2);
			int num5 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.Failure);
			int num6 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.CriticalFailure);
			int num7 = Tokens.Count + num5 / 2 + num6 + gameCount / 10;
			slots2 = Slots.Count;
			tokens = Tokens.Count;
			asMaster = true;
			int gameCount2 = GetGameCount(null, slots2, tokens, null, asMaster);
			asMaster = true;
			bool? asMaster2 = false;
			int gameCount3 = GetGameCount(null, null, null, asMaster2, asMaster);
			if (gameCount2 + gameCount3 / 100 >= num7)
			{
				AsMaster = true;
			}
		}
		if (!AsMaster)
		{
			return;
		}
		string sifrahExamineAuto = Options.SifrahExamineAuto;
		if (!(sifrahExamineAuto == "Always"))
		{
			if (sifrahExamineAuto == "Never")
			{
				return;
			}
			DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered examining artifacts of this complexity. Do you want to perform detailed examination anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically succeed at the examination.");
			if (dialogResult != DialogResult.Yes)
			{
				Finished = true;
				if (dialogResult == DialogResult.No)
				{
					Bypass = true;
				}
				else
				{
					Abort = true;
				}
			}
		}
		else
		{
			Finished = true;
			Bypass = true;
		}
	}

	public override bool CheckEarlyExit(GameObject ContextObject)
	{
		CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var CriticalFailure);
		if (Success <= 0 && CriticalSuccess <= 0 && Turn == 1)
		{
			Abort = true;
			return true;
		}
		string text = "Do you want to try to complete the examination in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the examination, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
			}
		}
		switch (Popup.ShowYesNoCancel(text))
		{
		case DialogResult.Yes:
			return true;
		case DialogResult.No:
			Abort = true;
			return true;
		default:
			return false;
		}
	}

	public override bool CheckOutOfOptions(GameObject ContextObject)
	{
		CalculateOutcomeChances(out var _, out var _, out var _, out var _, out var CriticalFailure);
		string text = "You have no more usable options. Do you want to try to complete the examination in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the examination, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
			}
		}
		if (Popup.ShowYesNo(text) != DialogResult.Yes)
		{
			Abort = true;
		}
		return true;
	}

	public override void Finish(GameObject ContextObject)
	{
		Examiner examiner = ContextObject.RequirePart<Examiner>();
		if (Bypass)
		{
			PlayOutcomeSound(GameOutcome.Success);
			examiner.ResultSuccess(The.Player);
			return;
		}
		if (PercentSolved == 100)
		{
			ContextObject.ModIntProperty("ItemNamingBonus", 1);
		}
		if (!Abort)
		{
			GameOutcome gameOutcome = DetermineOutcome();
			PlayOutcomeSound(gameOutcome);
			bool mastered = false;
			switch (gameOutcome)
			{
			case GameOutcome.CriticalFailure:
				examiner.ResultCriticalFailure(The.Player);
				break;
			case GameOutcome.Failure:
				examiner.ResultFailure(The.Player);
				break;
			case GameOutcome.PartialSuccess:
				examiner.ResultPartialSuccess(The.Player);
				break;
			case GameOutcome.Success:
			{
				if (PercentSolved >= 100)
				{
					mastered = true;
				}
				examiner.ResultSuccess(The.Player);
				int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty);
				if (PercentSolved < 100)
				{
					num2 = num2 * (100 - (100 - PercentSolved) * 3) / 100;
				}
				if (num2 > 0)
				{
					The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, ContextObject);
				}
				break;
			}
			case GameOutcome.CriticalSuccess:
			{
				if (PercentSolved >= 100)
				{
					mastered = true;
				}
				examiner.ResultExceptionalSuccess(The.Player);
				TinkeringSifrah.AwardInsight();
				int num = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 3;
				if (PercentSolved < 100)
				{
					num = num * (100 - (100 - PercentSolved) * 3) / 100;
				}
				if (num > 0)
				{
					The.Player.AwardXP(num, -1, 0, int.MaxValue, null, ContextObject);
				}
				ContextObject.ModIntProperty("ItemNamingBonus", (PercentSolved != 100) ? 1 : 2);
				break;
			}
			}
			SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count, AsMaster, mastered);
		}
		else if (Turn > 1)
		{
			CalculateOutcomeChances(out var _, out var _, out var _, out var _, out var CriticalFailure);
			int num3 = MaxTurns - Turn + 1;
			if ((CriticalFailure - num3).in100())
			{
				examiner.ResultCriticalFailure(The.Player);
				RequestInterfaceExit();
				AutoAct.Interrupt();
			}
		}
	}

	public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
	{
		double num = (double)(InspectRating + Understanding + PercentSolved) * 0.01;
		double num2 = num;
		if (num2 < 1.0)
		{
			int i = 1;
			for (int num3 = Complexity.DiminishingReturns(1.0); i < num3; i++)
			{
				num2 *= num;
			}
		}
		if (num2 < 0.0)
		{
			num2 = 0.0;
		}
		double num4 = 0.01;
		if (Turn > 1)
		{
			num4 += 0.02 * (double)(MaxTurns - Turn);
		}
		if (AsMaster)
		{
			num4 *= 3.0;
		}
		double num5 = num2 * num4;
		if (num2 > 1.0)
		{
			num2 = 1.0;
		}
		double num6 = 1.0 - num2;
		double num7 = num6 * 0.5;
		double num8 = num6 * 0.1;
		num2 -= num5;
		num6 -= num7;
		num6 -= num8;
		Success = (int)(num2 * 100.0);
		Failure = (int)(num6 * 100.0);
		PartialSuccess = (int)(num7 * 100.0);
		CriticalSuccess = (int)(num5 * 100.0);
		CriticalFailure = (int)(num8 * 100.0);
		while (Success + Failure + PartialSuccess + CriticalSuccess + CriticalFailure < 100)
		{
			if (AsMaster)
			{
				CriticalSuccess++;
			}
			else
			{
				Success++;
			}
		}
	}

	public void RequestInterfaceExit()
	{
		InterfaceExitRequested = true;
	}
}

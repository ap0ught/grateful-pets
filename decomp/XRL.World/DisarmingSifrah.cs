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
public class DisarmingSifrah : TinkeringSifrah
{
	public int Difficulty;

	public int Rating;

	public bool Abort;

	public bool AsMaster;

	public bool Bypass;

	public bool Auto;

	public bool InterfaceExitRequested;

	public string HandlerID;

	public string HandlerPartName;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("investigating gearworks", "Items/sw_gears2.bmp", "é", "&c", 'c'),
		new SifrahSlotConfiguration("tracing spark vines", "Items/sw_copper_wire.bmp", "í", "&W", 'G'),
		new SifrahSlotConfiguration("divining humour flows", "Items/sw_sparktick_plasma.bmp", "÷", "&Y", 'y'),
		new SifrahSlotConfiguration("investigating glow modules", "Items/sw_lens.bmp", "\u001f", "&B", 'b'),
		new SifrahSlotConfiguration("interpreting spirit messages", "Items/sw_computer.bmp", "Ñ", "&c", 'G'),
		new SifrahSlotConfiguration("meditating on the beyond", "Items/sw_wind_turbine_3.bmp", "ì", "&m", 'K')
	};

	public DisarmingSifrah(GameObject ContextObject, int Difficulty, int Rating, bool Auto)
	{
		Description = "Disarming " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
		this.Difficulty = Difficulty;
		this.Rating = Rating;
		this.Auto = Auto;
		int num = 3 + Difficulty;
		int num2 = 4;
		if (Difficulty >= 3)
		{
			num2++;
		}
		if (Difficulty >= 7)
		{
			num2++;
		}
		if (num2 > slotConfigs.Count)
		{
			num2 = slotConfigs.Count;
		}
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		int num3 = GetTinkeringBonusEvent.GetFor(The.Player, ContextObject, "Disarming", MaxTurns, 0, ref Interrupt, ref PsychometryApplied, Interruptable: true, ForSifrah: true);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		MaxTurns += num3;
		List<SifrahToken> list = new List<SifrahToken>(num);
		if (list.Count < num && (Difficulty < 3 || Rating > 20 + Difficulty * 3) && !The.Player.HasPart<Myopia>())
		{
			list.Add(new TinkeringSifrahTokenVisualInspection());
		}
		if (list.Count < num && (Difficulty < 4 || Rating > 10 + Difficulty * 2))
		{
			list.Add(new TinkeringSifrahTokenPhysicalManipulation());
		}
		if ((list.Count < num) & PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("read psychic impressions of arming process"));
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new TinkeringSifrahTokenTenfoldPathBin());
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
		if (list.Count < num && The.Player.HasPart<Telekinesis>())
		{
			list.Add(new TinkeringSifrahTokenTelekinesis());
		}
		int num4 = num - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Difficulty < 5 || Rating > 5 + Difficulty * 3)
			{
				list2.Add(new TinkeringSifrahTokenToolkit());
			}
			list2.Add(new TinkeringSifrahTokenComputePower(Difficulty * Difficulty));
			list2.Add(new TinkeringSifrahTokenCharge(500 * Difficulty));
			Tinkering_Mine part = ContextObject.GetPart<Tinkering_Mine>();
			if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(part?.Explosive))
			{
				list2.Add(new TinkeringSifrahTokenCreationKnowledge(part.Explosive));
			}
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Difficulty)
				{
					break;
				}
				list2.Add(new TinkeringSifrahTokenBit(bitType));
			}
			list2.Add(new TinkeringSifrahTokenLiquid("oil"));
			list2.Add(new TinkeringSifrahTokenLiquid("gel"));
			list2.Add(new TinkeringSifrahTokenLiquid("acid"));
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
			Popup.ShowFail("You have no usable options to employ for disarming " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of success. You can remedy this situation by improving your Intelligence and tinkering skills, or by obtaining items useful for tinkering.");
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
			int num7 = Tokens.Count + num5 / 2 + num6 + gameCount / 15;
			slots2 = Slots.Count;
			tokens = Tokens.Count;
			asMaster = true;
			int gameCount2 = GetGameCount(null, slots2, tokens, null, asMaster);
			asMaster = true;
			bool? asMaster2 = false;
			int gameCount3 = GetGameCount(null, null, null, asMaster2, asMaster);
			if (gameCount2 + gameCount3 / 75 >= num7)
			{
				AsMaster = true;
			}
		}
		if (!AsMaster)
		{
			return;
		}
		string sifrahDisarmingAuto = Options.SifrahDisarmingAuto;
		if (!(sifrahDisarmingAuto == "Always"))
		{
			if (sifrahDisarmingAuto == "Never")
			{
				return;
			}
			DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered disarming operations of this complexity. Do you want to perform detailed disarming anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically succeed.");
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

	public static bool IsMastered(GameObject ContextObject, int Difficulty, int Rating)
	{
		int num = 3 + Difficulty;
		int num2 = 4;
		if (Difficulty >= 3)
		{
			num2++;
		}
		if (Difficulty >= 7)
		{
			num2++;
		}
		if (num2 > slotConfigs.Count)
		{
			num2 = slotConfigs.Count;
		}
		int num3 = 0;
		if ((Difficulty < 3 || Rating > 20 + Difficulty * 3) && !The.Player.HasPart<Myopia>())
		{
			num3++;
		}
		if (Difficulty < 4 || Rating > 10 + Difficulty * 2)
		{
			num3++;
		}
		if (The.Player.HasPart<Psychometry>())
		{
			num3++;
		}
		if (The.Player.HasSkill("TenfoldPath_Bin"))
		{
			num3++;
		}
		Scanning.Scan scan = Scanning.GetScanTypeFor(ContextObject);
		if (scan == Scanning.Scan.Structure)
		{
			scan = Scanning.Scan.Tech;
		}
		if (Scanning.HasScanningFor(The.Player, scan))
		{
			num3++;
		}
		if (The.Player.HasPart<Telekinesis>())
		{
			num3++;
		}
		if (Difficulty < 5 || Rating > 5 + Difficulty * 3)
		{
			num3++;
		}
		num3++;
		num3++;
		if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(ContextObject.GetPart<Tinkering_Mine>()?.Explosive))
		{
			num3++;
		}
		using (List<BitType>.Enumerator enumerator = BitType.BitTypes.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Level <= Difficulty)
			{
				num3++;
			}
		}
		num3++;
		num3++;
		num3++;
		num3++;
		if (num > num3)
		{
			num = num3;
		}
		string gameType = SifrahGame.GetGameType(typeof(DisarmingSifrah));
		bool? asMaster = true;
		int? slots = num2;
		int? tokens = num;
		if (SifrahGame.GetGameCount(gameType, null, slots, tokens, asMaster) > 0)
		{
			return true;
		}
		tokens = num2;
		slots = num;
		int gameCount = SifrahGame.GetGameCount(gameType, null, tokens, slots);
		int num4 = SifrahGame.GetGameCount(gameType, Slots: num2, Tokens: num, Outcome: GameOutcome.Failure);
		int num5 = SifrahGame.GetGameCount(gameType, Slots: num2, Tokens: num, Outcome: GameOutcome.CriticalFailure);
		int num6 = num + num4 / 2 + num5 + gameCount / 15;
		slots = num2;
		tokens = num;
		asMaster = true;
		int gameCount2 = SifrahGame.GetGameCount(gameType, null, slots, tokens, null, asMaster);
		asMaster = true;
		bool? asMaster2 = false;
		int gameCount3 = SifrahGame.GetGameCount(gameType, null, null, null, asMaster2, asMaster);
		return gameCount2 + gameCount3 / 75 >= num6;
	}

	public override bool CheckEarlyExit(GameObject ContextObject)
	{
		CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var CriticalFailure);
		if (Success <= 0 && CriticalSuccess <= 0 && Turn == 1)
		{
			Abort = true;
			return true;
		}
		string text = "Do you want to try to complete the disarming attempt in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the disarming attempt, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
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
		string text = "You have no more usable options. Do you want to try to complete the disarming attempt in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the disarming attempt, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
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
		GameObject gameObject = GameObject.FindByID(HandlerID);
		if (gameObject == null || !(gameObject.GetPart(HandlerPartName) is IDisarmingSifrahHandler disarmingSifrahHandler))
		{
			return;
		}
		if (Bypass)
		{
			PlayOutcomeSound(GameOutcome.Success);
			disarmingSifrahHandler.DisarmingResultSuccess(The.Player, ContextObject, Auto);
		}
		else if (!Abort)
		{
			GameOutcome gameOutcome = DetermineOutcome();
			PlayOutcomeSound(gameOutcome);
			bool mastered = false;
			switch (gameOutcome)
			{
			case GameOutcome.CriticalFailure:
				disarmingSifrahHandler.DisarmingResultCriticalFailure(The.Player, ContextObject, Auto);
				RequestInterfaceExit();
				break;
			case GameOutcome.Failure:
				disarmingSifrahHandler.DisarmingResultFailure(The.Player, ContextObject, Auto);
				RequestInterfaceExit();
				break;
			case GameOutcome.PartialSuccess:
				disarmingSifrahHandler.DisarmingResultPartialSuccess(The.Player, ContextObject, Auto);
				break;
			case GameOutcome.Success:
			{
				if (PercentSolved >= 100)
				{
					mastered = true;
				}
				disarmingSifrahHandler.DisarmingResultSuccess(The.Player, ContextObject, Auto);
				int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
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
				disarmingSifrahHandler.DisarmingResultExceptionalSuccess(The.Player, ContextObject, Auto);
				TinkeringSifrah.AwardInsight();
				int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty * 3;
				if (PercentSolved < 100)
				{
					num = num * (100 - (100 - PercentSolved) * 3) / 100;
				}
				if (num > 0)
				{
					The.Player.AwardXP(num, -1, 0, int.MaxValue, null, ContextObject);
				}
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
				disarmingSifrahHandler.DisarmingResultCriticalFailure(The.Player, ContextObject, Auto);
				RequestInterfaceExit();
				AutoAct.Interrupt();
			}
		}
	}

	public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
	{
		double num = (double)(Rating + PercentSolved) * 0.01;
		double num2 = num;
		if (num2 < 1.0)
		{
			int i = 1;
			for (int num3 = Difficulty.DiminishingReturns(1.0); i < num3; i++)
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
			num4 += 0.01 * (double)(MaxTurns - Turn);
		}
		if (AsMaster)
		{
			num4 *= 5.0;
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

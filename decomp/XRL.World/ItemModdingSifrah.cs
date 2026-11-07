using System;
using System.Collections.Generic;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class ItemModdingSifrah : TinkeringSifrah
{
	public int Complexity;

	public int Difficulty;

	public int Rating;

	public int Performance;

	public bool Abort;

	public bool ApplyMod;

	public bool InterfaceExitRequested;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("upgrading gearworks", "Items/sw_gears2.bmp", "é", "&c", 'c'),
		new SifrahSlotConfiguration("retooling spark vines", "Items/sw_copper_wire.bmp", "í", "&W", 'G'),
		new SifrahSlotConfiguration("optimizing humour flows", "Items/sw_sparktick_plasma.bmp", "÷", "&Y", 'y'),
		new SifrahSlotConfiguration("enhancing glow modules", "Items/sw_lens.bmp", "\u001f", "&B", 'b'),
		new SifrahSlotConfiguration("befriending spirits", "Items/sw_computer.bmp", "Ñ", "&c", 'G'),
		new SifrahSlotConfiguration("invoking the beyond", "Items/sw_wind_turbine_3.bmp", "ì", "&m", 'K')
	};

	public ItemModdingSifrah(GameObject ContextObject, int Complexity, int Difficulty, int Rating)
	{
		Description = "Modding " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
		this.Complexity = Complexity;
		this.Difficulty = Difficulty;
		this.Rating = Rating;
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
		int num3 = GetTinkeringBonusEvent.GetFor(The.Player, ContextObject, "ItemModding", MaxTurns, 0, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable: true, ForSifrah: true);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		MaxTurns += num3;
		if (SecondaryBonus != 0)
		{
			Rating += SecondaryBonus;
			this.Rating = Rating;
		}
		List<SifrahToken> list = new List<SifrahToken>(num);
		if (list.Count < num && Rating > 10 + Complexity * 3 + Difficulty * 2)
		{
			list.Add(new TinkeringSifrahTokenPhysicalManipulation());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Hok"))
		{
			list.Add(new TinkeringSifrahTokenTenfoldPathHok());
		}
		if (list.Count < num && The.Player.HasPart<Telekinesis>() && (Complexity < 3 || Rating > Difficulty + Complexity * 2))
		{
			list.Add(new TinkeringSifrahTokenTelekinesis());
		}
		int num4 = num - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Rating > 5 + Complexity * 3 + Difficulty)
			{
				list2.Add(new TinkeringSifrahTokenAdvancedToolkit());
			}
			if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(ContextObject))
			{
				list2.Add(new TinkeringSifrahTokenCreationKnowledge(ContextObject));
			}
			list2.Add(new TinkeringSifrahTokenComputePower((Complexity + Difficulty) * (Complexity + Difficulty)));
			list2.Add(new TinkeringSifrahTokenCharge(Math.Min(1000 * (Complexity + Difficulty), 10000)));
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Complexity + Difficulty)
				{
					break;
				}
				list2.Add(new TinkeringSifrahTokenBit(bitType));
			}
			if (Rating > 5 + Complexity * 3 + Difficulty)
			{
				list2.Add(new TinkeringSifrahTokenLiquid("oil"));
			}
			if (Rating > 5 + Complexity * 4 + Difficulty * 2)
			{
				list2.Add(new TinkeringSifrahTokenLiquid("gel"));
			}
			if (Rating > 5 + Complexity * 5 + Difficulty * 3)
			{
				list2.Add(new TinkeringSifrahTokenLiquid("acid"));
			}
			if (Rating > 5 + Complexity + Difficulty / 2)
			{
				list2.Add(new TinkeringSifrahTokenLiquid("lava"));
			}
			list2.Add(new TinkeringSifrahTokenLiquid("brainbrine"));
			list2.Add(new TinkeringSifrahTokenLiquid("neutronflux"));
			list2.Add(new TinkeringSifrahTokenLiquid("sunslag"));
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
			Popup.ShowFail("You have no usable options to employ for modding " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of success. You can remedy this situation by improving your Intelligence and tinkering skills, or by obtaining items useful for tinkering.");
			Finished = true;
			Abort = true;
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
		string text = "Do you want to try to complete the mod in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the mod, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
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
		string text = "You have no more usable options. Do you want to try to complete the mod in its current state?";
		if (Turn > 1)
		{
			int num = MaxTurns - Turn + 1;
			int num2 = CriticalFailure - num;
			if (num2 > 0)
			{
				text = text + " Answering 'No' will abort the mod, but will still have " + (Grammar.IndefiniteArticleShouldBeAn(num2) ? "an" : "a") + " {{C|" + num2 + "%}} chance of causing a critical failure.";
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
		if (PercentSolved == 100)
		{
			ContextObject.ModIntProperty("ItemNamingBonus", 1);
		}
		if (!Abort)
		{
			GameOutcome gameOutcome = DetermineOutcome();
			PlayOutcomeSound(gameOutcome);
			switch (gameOutcome)
			{
			case GameOutcome.CriticalFailure:
				ResultCriticalFailure(ContextObject);
				break;
			case GameOutcome.Failure:
				ResultFailure(ContextObject);
				break;
			case GameOutcome.PartialSuccess:
				ResultPartialSuccess(ContextObject);
				break;
			case GameOutcome.Success:
			{
				ResultSuccess(ContextObject);
				int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 2;
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
				ResultCriticalSuccess(ContextObject);
				TinkeringSifrah.AwardInsight();
				int num = (Tokens.Count * Tokens.Count - Slots.Count) * (Complexity + Difficulty) * 10;
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
			SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count);
		}
		else if (Turn > 1)
		{
			CalculateOutcomeChances(out var _, out var _, out var _, out var _, out var CriticalFailure);
			int num3 = MaxTurns - Turn + 1;
			if ((CriticalFailure - num3).in100())
			{
				ResultCriticalFailure(ContextObject);
				RequestInterfaceExit();
				AutoAct.Interrupt();
			}
		}
	}

	private void ResultCriticalFailure(GameObject ContextObject)
	{
		if (The.Player.HasPart<Dystechnia>())
		{
			Dystechnia.CauseExplosion(ContextObject, The.Player);
			RequestInterfaceExit();
			return;
		}
		Popup.Show("Your work applied the mod, but catastrophically poorly.");
		ContextObject.ApplyEffect(new Broken());
		Performance = -Stat.Random(1, 4);
		ApplyMod = true;
	}

	private void ResultFailure(GameObject ContextObject)
	{
		Popup.Show("You abjectly failed to mod " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
		Performance = 0;
	}

	private void ResultPartialSuccess(GameObject ContextObject)
	{
		Popup.Show("Your work modding " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " was passable.");
		Performance = 0;
		ApplyMod = true;
	}

	private void ResultSuccess(GameObject ContextObject)
	{
		Popup.Show("Your work modding " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " was solid and craftsmanlike.");
		Performance = ((!25.in100()) ? 1 : 2);
		ApplyMod = true;
	}

	private void ResultCriticalSuccess(GameObject ContextObject)
	{
		Popup.Show("Your work modding " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " was outstanding.");
		Performance = Stat.Random(3, 5);
		ApplyMod = true;
	}

	public override void CalculateOutcomeChances(out int Success, out int Failure, out int PartialSuccess, out int CriticalSuccess, out int CriticalFailure)
	{
		double num = (double)(Rating + PercentSolved) * 0.01;
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
		double num4 = 0.02;
		if (Turn > 1)
		{
			num4 += 0.02 * (double)(MaxTurns - Turn);
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
			Success++;
		}
	}

	public void RequestInterfaceExit()
	{
		InterfaceExitRequested = true;
	}
}

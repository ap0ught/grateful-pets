using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class ItemNamingSifrah : RitualSifrah
{
	public int Difficulty;

	public int Rating;

	public int BasicBestowals;

	public bool ElementBestowal;

	public bool Abort;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("opening the circle", "Items/sw_cyber_reactive_cranial_plating.bmp", "\u000f", "&B", 'y'),
		new SifrahSlotConfiguration("cleansing the circle", "Items/sw_cyber_medassist_module.bmp", "\u0015", "&y", 'K', 4),
		new SifrahSlotConfiguration("calling the quarters", "Items/sw_cyber_anchor_spikes.bmp", "\n", "&Y", 'W', 5),
		new SifrahSlotConfiguration("making the petition", "Items/sw_mannequin.bmp", "\u0014", "&y", 'W', 2),
		new SifrahSlotConfiguration("giving thanks", "Items/ms_face_heart.png", "§", "&y", 'M', 3),
		new SifrahSlotConfiguration("closing the circle", "Items/sw_orb.bmp", "\a", "&B", 'b', 1)
	};

	public ItemNamingSifrah(GameObject ContextObject, int Rating, int Difficulty)
	{
		Description = "Ritually naming " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		GetRitualSifrahSetupEvent.GetFor(The.Player, ContextObject, "ItemNaming", Interruptable: false, ref Difficulty, ref Rating, ref MaxTurns, ref Interrupt, ref PsychometryApplied);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		this.Rating = Rating;
		this.Difficulty = Difficulty + ContextObject.GetIntProperty("ItemNamingSifrahDifficultyModifier");
		int num = Math.Max(Difficulty * 4 / 3, 4);
		int num2 = 4;
		if (Difficulty >= 5)
		{
			num2++;
		}
		if (Difficulty >= 8)
		{
			num2++;
		}
		if (num2 > slotConfigs.Count)
		{
			num2 = slotConfigs.Count;
		}
		List<SifrahToken> list = new List<SifrahToken>(num);
		if (Difficulty < 1)
		{
			Difficulty = 1;
		}
		if (Rating < 1)
		{
			Rating = 1;
		}
		if (MaxTurns < 1)
		{
			MaxTurns = 1;
		}
		if (list.Count < num && SocialSifrahTokenThePowerOfLove.IsAvailable())
		{
			list.Add(new RitualSifrahTokenThePowerOfLove());
		}
		if (list.Count < num && Rating >= Difficulty)
		{
			list.Add(new RitualSifrahTokenInvokeAncientCompacts());
		}
		if (list.Count < num && The.Player.Stat("Level") >= Difficulty * 5)
		{
			list.Add(new RitualSifrahTokenRecountAccomplishments());
		}
		if (list.Count < num && !The.Player.HasEquippedItem("Cyclopean Prism"))
		{
			list.Add(new RitualSifrahTokenPrayHumbly());
		}
		if (list.Count < num && The.Player.HasSkill("Customs_Sharer"))
		{
			list.Add(new RitualSifrahTokenSingHymn());
		}
		if ((list.Count < num) & PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("recount psychic impressions of item's history"));
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Sed"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathSed());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Hok"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathHok());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathBin());
		}
		int num3 = num - list.Count;
		if (num3 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Rating >= Difficulty / 2 && The.Player.Respires)
			{
				list2.Add(new RitualSifrahTokenHookah());
			}
			if (Rating >= Difficulty + 1)
			{
				list2.Add(new RitualSifrahTokenLiquid("water"));
			}
			if (Rating >= Difficulty)
			{
				list2.Add(new RitualSifrahTokenLiquid("blood"));
			}
			if (Rating >= Difficulty - 1)
			{
				list2.Add(new RitualSifrahTokenLiquid("honey"));
			}
			list2.Add(new RitualSifrahTokenReadFromTheCanticlesChromaic());
			if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(ContextObject))
			{
				list2.Add(new RitualSifrahTokenCreationKnowledge(ContextObject));
			}
			list2.Add(new RitualSifrahTokenLiquid("brainbrine"));
			list2.Add(new RitualSifrahTokenLiquid("neutronflux"));
			list2.Add(new RitualSifrahTokenLiquid("sunslag"));
			foreach (BitType bitType in BitType.BitTypes)
			{
				if (bitType.Level > Difficulty)
				{
					break;
				}
				list2.Add(new RitualSifrahTokenBit(bitType));
			}
			list2.Add(new RitualSifrahTokenCharge(Math.Min(Difficulty * 1000, 10000)));
			RitualSifrahTokenGift appropriate = RitualSifrahTokenGift.GetAppropriate(ContextObject);
			if (appropriate != null)
			{
				list2.Add(appropriate);
			}
			RitualSifrahTokenFood appropriate2 = RitualSifrahTokenFood.GetAppropriate(ContextObject);
			if (appropriate2 != null)
			{
				list2.Add(appropriate2);
			}
			list2.Add(new RitualSifrahTokenScourging());
			List<Worshippable> worshippables = Factions.GetWorshippables();
			foreach (Worshippable item in worshippables)
			{
				if (item.GetRelevance(ContextObject, Rating) >= Difficulty * 2)
				{
					list2.Add(new RitualSifrahTokenInvokeHigherBeing(item, worshippables));
				}
			}
			if (!The.Player.HasEffect<Shamed>() && The.Player.CanApplyEffect("Shamed"))
			{
				int num4 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num4 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShamed(num4));
				}
			}
			if (!The.Player.HasEffect<Dazed>() && !The.Player.HasEffect<Stun>() && The.Player.CanApplyEffect("Dazed"))
			{
				int num5 = Math.Max(50 + (Difficulty - Rating) * 5, 5);
				if (num5 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDazed(num5));
				}
			}
			if (!The.Player.HasEffect<Shaken>() && The.Player.CanApplyEffect("Shaken"))
			{
				int num6 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShaken(num6));
				}
			}
			if (Rating >= 4 && !The.Player.HasEffect<Exhausted>() && The.Player.CanApplyEffect("Exhausted"))
			{
				int num7 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num7 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectExhausted(num7));
				}
			}
			if (Rating >= 5 && !The.Player.HasEffect<Terrified>() && The.Player.CanApplyEffect("Terrified", 0, "CanApplyFear"))
			{
				int num8 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num8 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectTerrified(num8));
				}
			}
			if (Rating >= 6 && !The.Player.HasEffect<Asleep>() && The.Player.CanApplyEffect("Asleep", 0, "CanApplySleep"))
			{
				int num9 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num9 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectAsleep(num9));
				}
			}
			if (Rating >= 7 && !The.Player.HasEffect<Disoriented>() && The.Player.CanApplyEffect("Disoriented"))
			{
				int num10 = Math.Max(25 + (Difficulty - Rating) * 10, 5);
				if (num10 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDisoriented(num10));
				}
			}
			if (Rating >= 4 && !The.Player.HasEffect<Confused>() && The.Player.CanApplyEffect("Confused"))
			{
				int num11 = Math.Max(20 + (Difficulty - Rating) * 10, 5);
				if (num11 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectConfused(num11));
				}
			}
			if (Rating >= 2 && !The.Player.HasEffect<Bleeding>() && The.Player.CanApplyEffect("Bleeding"))
			{
				int num12 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num12 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectBleeding(num12));
				}
			}
			if (Rating >= 2 && !The.Player.HasEffect<Poisoned>() && The.Player.CanApplyEffect("Poisoned", 0, "CanApplyPoison"))
			{
				int num13 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num13 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectPoisoned(num13));
				}
			}
			if (Rating >= 3 && !The.Player.HasEffect<Ill>() && !The.Player.HasEffect<Poisoned>() && The.Player.CanApplyEffect("Ill"))
			{
				int num14 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num14 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectIll(num14));
				}
			}
			if (Rating >= 5 && !The.Player.HasEffect<CardiacArrest>() && The.Player.CanApplyEffect("CardiacArrest"))
			{
				int num15 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num15 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectCardiacArrest(num15));
				}
			}
			if (Rating >= 12 && !The.Player.HasEffect<ShatterMentalArmor>() && The.Player.CanApplyEffect("ShatterMentalArmor"))
			{
				int num16 = Math.Max(40 + (Difficulty - Rating) * 5, 5);
				if (num16 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShatterMentalArmor(num16));
				}
			}
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Ego"));
			AssignPossibleTokens(list2, list, num3, num);
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
			Popup.ShowFail("You have no usable options to employ for ritually naming " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of performing well. You can remedy this situation by improving your Ego, Willpower, and esoteric skills, or by obtaining items useful in ritual.");
			Finished = true;
			Abort = true;
		}
	}

	public override bool CheckEarlyExit(GameObject ContextObject)
	{
		if (Turn == 1)
		{
			CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var _);
			if (Success <= 0 && CriticalSuccess <= 0)
			{
				Abort = true;
				return true;
			}
			switch (Popup.ShowYesNoCancel("Do you want to finish the naming ritual as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish the naming ritual as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
	}

	public override bool CheckOutOfOptions(GameObject ContextObject)
	{
		if (Turn > 1)
		{
			Popup.ShowFail("You have no more usable options, so your performance so far will determine the outcome.");
		}
		return true;
	}

	public override void Finish(GameObject ContextObject)
	{
		if (Abort)
		{
			return;
		}
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
			int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
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
			ResultExceptionalSuccess(ContextObject);
			RitualSifrah.AwardInsight();
			int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
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
		SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count);
	}

	public virtual void ResultCriticalFailure(GameObject ContextObject)
	{
		Popup.Show("Your abysmal ritual performance deeply shames you.");
		The.Player.ApplyEffect(new Shamed(Stat.Random(100, 500)));
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Popup.Show("Your performance of the naming ritual was adequate, if barely.");
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the naming ritual was passable.");
		BasicBestowals++;
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the naming ritual was solemn and dignified.");
		BasicBestowals++;
		ElementBestowal = true;
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the naming ritual was sublime and inspiring.");
		BasicBestowals += Stat.Random(2, 4);
		ElementBestowal = true;
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
		double num4 = 0.03 + (double)Powerup * 0.01;
		if (Turn > 1)
		{
			num4 += 0.01 * (double)(MaxTurns - Turn);
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
}

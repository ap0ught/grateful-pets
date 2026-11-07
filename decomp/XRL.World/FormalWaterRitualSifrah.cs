using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class FormalWaterRitualSifrah : RitualSifrah
{
	public int Difficulty;

	public int Rating;

	public int Performance;

	public bool Abort;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("opening the ceremony", "Items/sw_cyber_reactive_cranial_plating.bmp", "\u000f", "&B", 'y', 1),
		new SifrahSlotConfiguration("cleansing the space", "Items/sw_skin_glitter.bmp", "\u0015", "&K", 'y', 4),
		new SifrahSlotConfiguration("preparing the sacrament", "Items/sw_shawl.bmp", "\n", "&Y", 'W', 3),
		new SifrahSlotConfiguration("consummating the rite", "Items/sw_urn_1.bmp", "é", "&Y", 'B'),
		new SifrahSlotConfiguration("honoring the space", "Items/ms_face_heart.png", "§", "&y", 'M', 5),
		new SifrahSlotConfiguration("closing the ceremony", "Items/sw_orb.bmp", "\a", "&B", 'b', 2)
	};

	public FormalWaterRitualSifrah(GameObject ContextObject)
	{
		Description = "Performing the formal water ritual with " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true);
		Rating = The.Player.StatMod("Ego");
		Difficulty = ContextObject.GetTier() + ContextObject.GetIntProperty("FormalWaterRitualSifrahDifficultyModifier");
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		GetRitualSifrahSetupEvent.GetFor(The.Player, ContextObject, "FormalWaterRitual", Interruptable: false, ref Difficulty, ref Rating, ref MaxTurns, ref Interrupt, ref PsychometryApplied);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		int num = Math.Max(Difficulty * 6 / 5, 4);
		int num2 = 4;
		if (Difficulty >= 4)
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
		RitualSifrahTokenLiquid ritualSifrahTokenLiquid = null;
		if (list.Count < num)
		{
			ritualSifrahTokenLiquid = new RitualSifrahTokenLiquid(ContextObject, WaterRitual: true);
			list.Add(ritualSifrahTokenLiquid);
		}
		if (list.Count < num && SocialSifrahTokenThePowerOfLove.IsAvailable())
		{
			list.Add(new RitualSifrahTokenThePowerOfLove());
		}
		if (list.Count < num && Rating >= Difficulty)
		{
			list.Add(new RitualSifrahTokenInvokeAncientCompacts());
		}
		if (list.Count < num && The.Player.HasSkill("Customs_Sharer"))
		{
			list.Add(new RitualSifrahTokenSingAHistoricalEpic());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Sed"))
		{
			list.Add(new RitualSifrahTokenTenfoldPathSed());
		}
		int num3 = num - list.Count;
		if (num3 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Rating >= Difficulty / 2 && ContextObject.Respires)
			{
				list2.Add(new SocialSifrahTokenHookah());
			}
			RitualSifrahTokenLiquid ritualSifrahTokenLiquid2 = new RitualSifrahTokenLiquid(ContextObject);
			if (ritualSifrahTokenLiquid == null || ritualSifrahTokenLiquid.LiquidID != ritualSifrahTokenLiquid2.LiquidID)
			{
				list2.Add(ritualSifrahTokenLiquid2);
			}
			if (Rating >= Difficulty / 2 && (Rating >= Difficulty * 2 || ContextObject.IsMemberOfFaction("Mechanimists") || ContextObject.IsMemberOfFaction("MechaSub") || Factions.GetFeelingFactionToObject("Mechanimists", ContextObject) >= 0))
			{
				list.Add(new SocialSifrahTokenReadFromTheCanticlesChromaic());
			}
			if (ContextObject.HasPart<Robot>())
			{
				foreach (BitType bitType in BitType.BitTypes)
				{
					if (bitType.Level > Difficulty)
					{
						break;
					}
					list2.Add(new SocialSifrahTokenBit(bitType));
				}
				list2.Add(new SocialSifrahTokenCharge(Difficulty * 1000));
			}
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
			List<Worshippable> worshippables = Factions.GetWorshippables();
			foreach (Worshippable item in worshippables)
			{
				if (item.GetRelevance(ContextObject, Rating) >= Difficulty * 2)
				{
					list2.Add(new RitualSifrahTokenInvokeHigherBeing(item, worshippables));
				}
			}
			if (!The.Player.HasEffect<Lovesick>() && The.Player.CanApplyEffect("Lovesick") && The.Player.IsOrganic)
			{
				int num4 = Math.Max(10 + (Difficulty - Rating) * 5, 5);
				if (num4 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectLovesick(num4));
				}
			}
			if (!The.Player.HasEffect<Shamed>() && The.Player.CanApplyEffect("Shamed"))
			{
				int num5 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num5 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShamed(num5));
				}
			}
			if (!The.Player.HasEffect<Dazed>() && !The.Player.HasEffect<Stun>() && The.Player.CanApplyEffect("Dazed"))
			{
				int num6 = Math.Max(50 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDazed(num6));
				}
			}
			if (!The.Player.HasEffect<Shaken>() && The.Player.CanApplyEffect("Shaken"))
			{
				int num7 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num7 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShaken(num7));
				}
			}
			if (Rating >= 2 && !The.Player.HasEffect<Exhausted>() && The.Player.CanApplyEffect("Exhausted"))
			{
				int num8 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num8 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectExhausted(num8));
				}
			}
			if (Rating >= 4 && !The.Player.HasEffect<Asleep>() && The.Player.CanApplyEffect("Asleep", 0, "CanApplySleep"))
			{
				int num9 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num9 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectAsleep(num9));
				}
			}
			if (Rating >= 8 && !The.Player.HasEffect<ShatterMentalArmor>() && The.Player.CanApplyEffect("ShatterMentalArmor"))
			{
				int num10 = Math.Max(40 + (Difficulty - Rating) * 5, 5);
				if (num10 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectShatterMentalArmor(num10));
				}
			}
			SocialSifrah.AddTokenTokens(list2, ContextObject);
			list2.Add(new SocialSifrahTokenSecret(ContextObject));
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
			Popup.ShowFail("You have no usable options to employ for performing the formal water ritual with " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of doing so well. You can remedy this situation by improving your Ego or by obtaining items useful in such a ritual.");
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
			switch (Popup.ShowYesNoCancel("Do you want to finish the formal water ritual as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish the formal water ritual as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
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
		Performance = 10;
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Popup.Show("Your performance of the formal water ritual was adequate, if barely.");
		Performance = 50;
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the formal water ritual was passable.");
		Performance = 100;
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the formal water ritual was solemn and dignified.");
		Performance = 150;
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
		Popup.Show("Your performance of the formal water ritual was sublime and inspiring.");
		Performance = 300;
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

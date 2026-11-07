using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Skills;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class RealityDistortionSifrah : PsionicSifrah
{
	public int Difficulty;

	public int Rating;

	public bool Abort;

	public bool AsMaster;

	public bool Bypass;

	public bool InterfaceExitRequested;

	public GameOutcome Outcome;

	public int Performance;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("clearing the mind", "Items/sw_cherubic.bmp", "\t", "&B", 'K', 3),
		new SifrahSlotConfiguration("forming the intention", "Items/sw_cyber_medassist_module.bmp", "\u000f", "&y", 'K'),
		new SifrahSlotConfiguration("focusing the will", "Items/sw_cyber_anchor_spikes.bmp", "\a", "&W", 'B', 2),
		new SifrahSlotConfiguration("charging the intention", "Items/sw_power_cut_large.png", "\u001e", "&M", 'R', 5),
		new SifrahSlotConfiguration("directing the intention", "Items/sw_esper.bmp", "\u001a", "&W", 'M', 4),
		new SifrahSlotConfiguration("letting go of the intention", "Items/sw_adrenalcontrol.bmp", "ê", "&b", 'm', 1)
	};

	public RealityDistortionSifrah(GameObject ContextObject, string Subtype, string Description, int Rating, int Difficulty)
	{
		base.Description = Description;
		MaxTurns = 3;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		GetPsionicSifrahSetupEvent.GetFor(The.Player, ContextObject, "RealityDistortion", Subtype, Interruptable: false, ref Difficulty, ref Rating, ref MaxTurns, ref Interrupt, ref PsychometryApplied);
		if (Interrupt)
		{
			Finished = true;
			Abort = true;
		}
		this.Rating = Rating;
		this.Difficulty = Difficulty;
		int num = Math.Max(Difficulty, 4);
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
			list.Add(new PsionicSifrahTokenThePowerOfLove());
		}
		if (list.Count < num)
		{
			int num3 = 0;
			foreach (KeyValuePair<string, PowerEntry> item in SkillFactory.Factory.PowersByClass)
			{
				if (item.Key.StartsWith("Discipline_") && The.Player.HasSkill(item.Value.Class))
				{
					num3++;
				}
			}
			if (num3 > 0 && num3 >= Difficulty / 2)
			{
				list.Add(new PsionicSifrahTokenDiscipline());
			}
		}
		if (list.Count < num && The.Player.Stat("Ego") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenExertWill());
		}
		if (list.Count < num && The.Player.Stat("Intelligence") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenApplyIntellect());
		}
		if (list.Count < num && The.Player.Stat("Willpower") >= 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenCalmMind());
		}
		if (list.Count < num && !The.Player.HasEquippedItem("Cyclopean Prism") && The.Player.Stat("Ego") < 10 + Difficulty * 3)
		{
			list.Add(new PsionicSifrahTokenPrayHumbly());
		}
		if (list.Count < num && The.Player.HasSkill("Customs_Sharer") && Rating > Difficulty)
		{
			list.Add(new PsionicSifrahTokenApplyAncientLore());
		}
		if ((list.Count < num) & PsychometryApplied)
		{
			list.Add(new TinkeringSifrahTokenPsychometry("leverage psychic impressions of item's history"));
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Khu") && Rating > Difficulty * 5 && The.Player.Stat("Willpoer") > 10 + Difficulty * 4)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathKhu());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Yis") && Rating > Difficulty * 4 && The.Player.Stat("Willpower") > 10 + Difficulty * 3)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathYis());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Hod") && Rating > Difficulty * 2 && The.Player.Stat("Intelligence") > 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathHod());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Tza") && Rating > Difficulty * 2 && The.Player.Stat("Ego") > 10 + Difficulty * 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathTza());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Ret") && Rating > Difficulty * 3 / 2 && The.Player.Stat("Willpower") > 10 + Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathRet());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Vur") && Rating > Difficulty * 3 / 2 && The.Player.Stat("Intelligence") > 10 + Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathVur());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Sed") && Rating > Difficulty * 3 / 2 && The.Player.Stat("Ego") > 10 + Difficulty * 3 / 2)
		{
			list.Add(new PsionicSifrahTokenTenfoldPathSed());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Bin"))
		{
			list.Add(new PsionicSifrahTokenTenfoldPathBin());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Hok"))
		{
			list.Add(new PsionicSifrahTokenTenfoldPathHok());
		}
		if (list.Count < num && The.Player.HasSkill("TenfoldPath_Ket"))
		{
			list.Add(new PsionicSifrahTokenTenfoldPathKet());
		}
		int num4 = num - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (TinkeringSifrahTokenCreationKnowledge.IsPotentiallyAvailableFor(ContextObject))
			{
				list2.Add(new PsionicSifrahTokenCreationKnowledge(ContextObject));
			}
			List<Worshippable> worshippables = Factions.GetWorshippables();
			foreach (Worshippable item2 in worshippables)
			{
				if (item2.GetCounterRelevance(ContextObject, Rating) >= Difficulty * 2)
				{
					list2.Add(new RitualSifrahTokenInvokeHigherBeing(item2, worshippables));
				}
			}
			if (ContextObject.IsCreature && !The.Player.HasEffect<Lovesick>() && The.Player.CanApplyEffect("Lovesick") && The.Player.IsOrganic)
			{
				int num5 = Math.Max(10 + (Difficulty - Rating) * 5, 5);
				if (num5 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectLovesick(num5));
				}
			}
			if (!The.Player.HasEffect<Shamed>() && The.Player.CanApplyEffect("Shamed"))
			{
				int num6 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShamed(num6));
				}
			}
			if (!The.Player.HasEffect<Dazed>() && !The.Player.HasEffect<Stun>() && The.Player.CanApplyEffect("Dazed"))
			{
				int num7 = Math.Max(50 + (Difficulty - Rating) * 5, 5);
				if (num7 <= 120)
				{
					list2.Add(new RitualSifrahTokenEffectDazed(num7));
				}
			}
			if (!The.Player.HasEffect<Shaken>() && The.Player.CanApplyEffect("Shaken"))
			{
				int num8 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num8 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShaken(num8));
				}
			}
			if (!The.Player.HasEffect<Exhausted>() && The.Player.CanApplyEffect("Exhausted"))
			{
				int num9 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num9 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectExhausted(num9));
				}
			}
			if (!The.Player.HasEffect<Terrified>() && The.Player.CanApplyEffect("Terrified", 0, "CanApplyFear"))
			{
				int num10 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num10 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectTerrified(num10));
				}
			}
			if (!The.Player.HasEffect<Asleep>() && The.Player.CanApplyEffect("Asleep", 0, "CanApplySleep"))
			{
				int num11 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num11 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectAsleep(num11));
				}
			}
			if (!The.Player.HasEffect<Disoriented>() && The.Player.CanApplyEffect("Disoriented"))
			{
				int num12 = Math.Max(25 + (Difficulty - Rating) * 10, 5);
				if (num12 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectDisoriented(num12));
				}
			}
			if (!The.Player.HasEffect<Confused>() && The.Player.CanApplyEffect("Confused"))
			{
				int num13 = Math.Max(20 + (Difficulty - Rating) * 10, 5);
				if (num13 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectConfused(num13));
				}
			}
			if (!The.Player.HasEffect<Nosebleed>() && The.Player.CanApplyEffect("Nosebleed") && The.Player.CanApplyEffect("Bleeding"))
			{
				int num14 = Math.Max(20 + (Difficulty - Rating) * 5, 5);
				if (num14 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectNosebleed(num14));
				}
			}
			if (!The.Player.HasEffect<CardiacArrest>() && The.Player.CanApplyEffect("CardiacArrest"))
			{
				int num15 = Math.Max(10 + (Difficulty - Rating) / 2 * 5, 5);
				if (num15 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectCardiacArrest(num15));
				}
			}
			if (!The.Player.HasEffect<ShatterMentalArmor>() && The.Player.CanApplyEffect("ShatterMentalArmor"))
			{
				int num16 = Math.Max(40 + (Difficulty - Rating) / 2 * 5, 5);
				if (num16 <= 120)
				{
					list2.Add(new PsionicSifrahTokenEffectShatterMentalArmor(num16));
				}
			}
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Ego"));
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Intelligence"));
			list2.Add(new RitualSifrahTokenAttributeSacrifice("Willpower"));
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
			Popup.ShowFail("You have no usable options to employ for " + Description + ", giving you no chance of performing well. You can remedy this situation by improving your Ego, Willpower, Intelligence, and esoteric skills.");
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
			int num17 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.Failure);
			int num18 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.CriticalFailure);
			int num19 = Tokens.Count + num17 / 2 + num18 + gameCount / 10;
			slots2 = Slots.Count;
			tokens = Tokens.Count;
			asMaster = true;
			int gameCount2 = GetGameCount(null, slots2, tokens, null, asMaster);
			asMaster = true;
			bool? asMaster2 = false;
			int gameCount3 = GetGameCount(null, null, null, asMaster2, asMaster);
			if (gameCount2 + gameCount3 / 100 >= num19)
			{
				AsMaster = true;
			}
		}
		if (!AsMaster)
		{
			return;
		}
		string sifrahRealityDistortionAuto = Options.SifrahRealityDistortionAuto;
		if (!(sifrahRealityDistortionAuto == "Always"))
		{
			if (sifrahRealityDistortionAuto == "Never")
			{
				return;
			}
			DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered reality distortion at this level of difficulty. Do you want to guide the process in detail anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically receive the results of strong but unexceptional performance.");
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
		if (Turn == 1)
		{
			CalculateOutcomeChances(out var Success, out var _, out var _, out var CriticalSuccess, out var _);
			if (Success <= 0 && CriticalSuccess <= 0)
			{
				Abort = true;
				return true;
			}
			switch (Popup.ShowYesNoCancel("Do you want to finish the attempt as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish the attempt as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
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
		if (Bypass)
		{
			PlayOutcomeSound(GameOutcome.Success);
			ResultSuccess(ContextObject);
		}
		else
		{
			if (Abort)
			{
				return;
			}
			Outcome = DetermineOutcome();
			PlayOutcomeSound(Outcome);
			bool mastered = false;
			if (Outcome == GameOutcome.CriticalFailure)
			{
				ResultCriticalFailure(ContextObject);
			}
			else if (Outcome == GameOutcome.Failure)
			{
				ResultFailure(ContextObject);
			}
			else if (Outcome == GameOutcome.PartialSuccess)
			{
				ResultPartialSuccess(ContextObject);
			}
			else if (Outcome == GameOutcome.Success)
			{
				if (PercentSolved >= 100)
				{
					mastered = true;
				}
				ResultSuccess(ContextObject);
				int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
				if (PercentSolved < 100)
				{
					num = num * (100 - (100 - PercentSolved) * 3) / 100;
				}
				if (num > 0)
				{
					The.Player.AwardXP(num, -1, 0, int.MaxValue, null, ContextObject);
				}
			}
			else if (Outcome == GameOutcome.CriticalSuccess)
			{
				if (PercentSolved >= 100)
				{
					mastered = true;
				}
				ResultExceptionalSuccess(ContextObject);
				PsionicSifrah.AwardInsight();
				int num2 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
				if (PercentSolved < 100)
				{
					num2 = num2 * (100 - (100 - PercentSolved) * 3) / 100;
				}
				if (num2 > 0)
				{
					The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, ContextObject);
				}
			}
			SifrahGame.RecordOutcome(this, Outcome, Slots.Count, Tokens.Count, AsMaster, mastered);
		}
	}

	public virtual void ResultCriticalFailure(GameObject ContextObject)
	{
		Performance = -100;
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Performance = 0;
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Performance = 50;
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		Performance = 100;
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
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
		double num4 = 0.02 + (double)Powerup * 0.01;
		if (Turn > 1)
		{
			num4 += 0.01 * (double)(MaxTurns - Turn);
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
				PartialSuccess++;
			}
		}
	}

	public void RequestInterfaceExit()
	{
		InterfaceExitRequested = true;
	}
}

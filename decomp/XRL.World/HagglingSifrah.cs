using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class HagglingSifrah : SocialSifrah
{
	public int Difficulty;

	public int Rating;

	public int Performance;

	public bool Abort;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("establishing social relations", "Items/sw_leather_gloves.bmp", "\u0015", "&y", 'W'),
		new SifrahSlotConfiguration("setting expectations", "Items/sw_table_ornate_1.bmp", "Ò", "&W", 'r', 2),
		new SifrahSlotConfiguration("justifying desired outcomes", "Items/sw_flower_cut_4.bmp", "\u009c", "&g", 'M', 4),
		new SifrahSlotConfiguration("establishing willingness to disengage", "Items/sw_buckler2.bmp", "\u001f", "&W", 'r', 3),
		new SifrahSlotConfiguration("preserving mutual dignity", "Items/sw_bust1.bmp", "è", "&Y", 'w', 5),
		new SifrahSlotConfiguration("closing the deal", "Items/sw_bar.bmp", "$", "&W", 'w', 1)
	};

	public HagglingSifrah(GameObject ContextObject)
	{
		Description = "Haggling with " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
		Difficulty = ContextObject.GetTier();
		string primaryFaction = ContextObject.GetPrimaryFaction();
		int num = ((!string.IsNullOrEmpty(primaryFaction)) ? The.Game.PlayerReputation.GetLevel(primaryFaction) : 0);
		Rating = GetTradePerformanceEvent.GetRatingFor(The.Player, ContextObject);
		MaxTurns = 3;
		if (!GetSocialSifrahSetupEvent.GetFor(The.Player, ContextObject, "Haggling", ref Difficulty, ref Rating, ref MaxTurns))
		{
			Finished = true;
			Abort = true;
		}
		int num2 = 3 + Difficulty;
		int num3 = 4;
		if (Difficulty >= 3)
		{
			num3++;
		}
		if (Difficulty >= 7)
		{
			num3++;
			num2 += 4;
		}
		switch (num)
		{
		case -1:
			num2++;
			break;
		case -2:
			num2 += 2;
			num3++;
			MaxTurns--;
			break;
		}
		if (num3 > slotConfigs.Count)
		{
			num3 = slotConfigs.Count;
		}
		List<SifrahToken> list = new List<SifrahToken>(num2);
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
		if (list.Count < num2 && SocialSifrahTokenThePowerOfLove.IsAvailable())
		{
			list.Add(new SocialSifrahTokenThePowerOfLove());
		}
		if (list.Count < num2)
		{
			Scanning.Scan scanTypeFor = Scanning.GetScanTypeFor(ContextObject);
			if (Scanning.HasScanningFor(The.Player, scanTypeFor))
			{
				list.Add(new SocialSifrahTokenScanning(scanTypeFor));
			}
		}
		if (list.Count < num2 && Rating >= Difficulty + 3)
		{
			list.Add(new SocialSifrahTokenSociableChat());
		}
		if (list.Count < num2 && Rating >= Difficulty + 2)
		{
			list.Add(new SocialSifrahTokenListenSympathetically());
		}
		if (list.Count < num2 && Rating >= Difficulty + 1)
		{
			list.Add(new SocialSifrahTokenCrackAJoke());
		}
		if (list.Count < num2 && Rating >= Difficulty)
		{
			list.Add(new SocialSifrahTokenPayACompliment());
		}
		switch (num)
		{
		case 2:
			MaxTurns++;
			if (list.Count < num2)
			{
				list.Add(new SocialSifrahTokenLeverageBeingLoved(primaryFaction));
			}
			break;
		case 1:
			if (list.Count < num2)
			{
				list.Add(new SocialSifrahTokenLeverageBeingFavored(primaryFaction));
			}
			break;
		}
		if (list.Count < num2 && The.Player.IsTrueKin() && ContextObject.HasPart<Robot>())
		{
			list.Add(new SocialSifrahTokenLeverageBeingTrueKin());
		}
		if (list.Count < num2 && Rating >= Difficulty - 1)
		{
			list.Add(new SocialSifrahTokenFlatterInsincerely());
		}
		if (list.Count < num2 && Rating >= Difficulty - 2)
		{
			list.Add(new SocialSifrahTokenSpinATaleOfWoe());
		}
		if (list.Count < num2 && ContextObject.Con(The.Player) <= -10)
		{
			list.Add(new SocialSifrahTokenPostureIntimidatingly());
		}
		if (list.Count < num2 && The.Player.CanMakeTelepathicContactWith(ContextObject))
		{
			list.Add(new SocialSifrahTokenTelepathy());
		}
		if (list.Count < num2 && The.Player.CanMakeEmpathicContactWith(ContextObject))
		{
			list.Add(new SocialSifrahTokenEmpathy());
		}
		if (list.Count < num2 && The.Player.HasSkill("TenfoldPath_Sed"))
		{
			list.Add(new SocialSifrahTokenTenfoldPathSed());
		}
		int num4 = num2 - list.Count;
		if (num4 > 0)
		{
			List<SifrahPrioritizableToken> list2 = new List<SifrahPrioritizableToken>();
			if (Rating >= Difficulty / 2 && ContextObject.Respires)
			{
				list2.Add(new SocialSifrahTokenHookah());
			}
			list2.Add(new SocialSifrahTokenLiquid(ContextObject));
			if (SocialSifrahTokenApplySocialCoprocessor.IsPotentiallyAvailable())
			{
				list2.Add(new SocialSifrahTokenApplySocialCoprocessor());
			}
			if (ContextObject.HasPart<Robot>())
			{
				int tier = ContextObject.GetTier();
				foreach (BitType bitType in BitType.BitTypes)
				{
					if (bitType.Level > tier)
					{
						break;
					}
					list2.Add(new SocialSifrahTokenBit(bitType));
				}
				list2.Add(new SocialSifrahTokenCharge(tier * 1000));
			}
			if (!The.Player.HasEffect<Lovesick>() && The.Player.CanApplyEffect("Lovesick") && The.Player.IsOrganic)
			{
				int num5 = Math.Max(10 + (Difficulty - Rating) * 5, 5);
				if (num5 <= 120)
				{
					list2.Add(new SocialSifrahTokenEffectLovesick(num5));
				}
			}
			if (The.Player.CanApplyEffect("Shamed"))
			{
				int num6 = Math.Max(30 + (Difficulty - Rating) * 5, 5);
				if (num6 <= 120)
				{
					list2.Add(new SocialSifrahTokenEffectShamed(num6));
				}
			}
			SocialSifrahTokenGift appropriate = SocialSifrahTokenGift.GetAppropriate(ContextObject);
			if (appropriate != null)
			{
				list2.Add(appropriate);
			}
			SocialSifrah.AddTokenTokens(list2, ContextObject);
			list2.Add(new SocialSifrahTokenSecret(ContextObject));
			AssignPossibleTokens(list2, list, num4, num2);
		}
		if (num2 > list.Count)
		{
			num2 = list.Count;
		}
		List<SifrahSlot> slots = SifrahSlot.GenerateListFromConfigurations(slotConfigs, num3, num2);
		Slots = slots;
		Tokens = list;
		if (!AnyUsableTokens(ContextObject))
		{
			Popup.ShowFail("You have no usable options to employ for haggling with " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of success. You can remedy this situation by improving your Ego and social skills, or by obtaining items useful in social situations.");
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
			switch (Popup.ShowYesNoCancel("Do you want to finish haggling as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish haggling as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
	}

	public override bool CheckOutOfOptions(GameObject ContextObject)
	{
		if (Turn > 1)
		{
			Popup.ShowFail("You have no more usable options, so your haggling so far will determine the outcome.");
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
			ResultCriticalFailure();
			break;
		case GameOutcome.Failure:
			ResultFailure();
			break;
		case GameOutcome.PartialSuccess:
			ResultPartialSuccess();
			break;
		case GameOutcome.Success:
		{
			ResultSuccess();
			int num3 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
			if (PercentSolved < 100)
			{
				num3 = num3 * (100 - (100 - PercentSolved) * 3) / 100;
			}
			int num4 = num3 - ContextObject.GetIntProperty("HagglingXPAwarded");
			if (num4 > 0)
			{
				ContextObject.SetIntProperty("HagglingXPAwarded", num3);
				The.Player.AwardXP(num4, -1, 0, int.MaxValue, null, ContextObject);
			}
			break;
		}
		case GameOutcome.CriticalSuccess:
		{
			ResultExceptionalSuccess();
			SocialSifrah.AwardInsight();
			int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
			if (PercentSolved < 100)
			{
				num = num * (100 - (100 - PercentSolved) * 3) / 100;
			}
			int num2 = num - ContextObject.GetIntProperty("HagglingXPAwarded");
			if (num2 > 0)
			{
				ContextObject.SetIntProperty("HagglingXPAwarded", num);
				The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, ContextObject);
			}
			break;
		}
		}
		SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count);
	}

	public virtual void ResultCriticalFailure()
	{
		Performance = -Stat.Random(80, 90);
		Description = "Your haggling was an abysmal failure.";
	}

	public virtual void ResultFailure()
	{
		Performance = -Stat.Random(40, 60);
		Description = "Your haggling went poorly.";
	}

	public virtual void ResultPartialSuccess()
	{
		Performance = Stat.Random(0, 10);
		Description = "Your haggling was mediocre.";
	}

	public virtual void ResultSuccess()
	{
		Performance = Stat.Random(20, 40);
		Description = "Your haggling went well.";
	}

	public virtual void ResultExceptionalSuccess()
	{
		Performance = Stat.Random(60, 80);
		Description = "Your haggling was spectacular.";
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

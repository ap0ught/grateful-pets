using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.Capabilities;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Tinkering;

namespace XRL.World;

/// This class is not used in the base game.
[Serializable]
public class ProselytizationSifrah : SocialSifrah
{
	public int Difficulty;

	public int Rating;

	public bool Abort;

	public bool AsMaster;

	public bool Bypass;

	private static readonly List<SifrahSlotConfiguration> slotConfigs = new List<SifrahSlotConfiguration>
	{
		new SifrahSlotConfiguration("opening a conversation", "Items/sw_leather_gloves.bmp", "\u0015", "&y", 'W'),
		new SifrahSlotConfiguration("identifying motivations", "Items/sw_heightenedhearing.bmp", "?", "&Y", 'W', 5),
		new SifrahSlotConfiguration("constructing a shared vision", "Mutations/night_vision_mutation.bmp", "é", "&B", 'M', 3),
		new SifrahSlotConfiguration("overcoming reservations", "Items/sw_microscope.bmp", ">", "&c", 'R', 2),
		new SifrahSlotConfiguration("justifying social realignment", "Items/sw_jackhammer.bmp", "ö", "&c", 'K', 4),
		new SifrahSlotConfiguration("eliciting the leap of faith", "Items/sw_longblade_3.bmp", "\u001a", "&Y", 'C', 1)
	};

	public ProselytizationSifrah(GameObject ContextObject, int Rating, int Difficulty)
	{
		Description = "Proselytizing " + ContextObject.an(int.MaxValue, null, null, AsIfKnown: false, Single: true, NoConfusion: false, NoColor: false, Stripped: false, WithoutTitles: false, Short: false);
		string primaryFaction = ContextObject.GetPrimaryFaction();
		int num = ((!string.IsNullOrEmpty(primaryFaction)) ? The.Game.PlayerReputation.GetLevel(primaryFaction) : 0);
		MaxTurns = 3;
		if (!GetSocialSifrahSetupEvent.GetFor(The.Player, ContextObject, "Proselytization", ref Difficulty, ref Rating, ref MaxTurns))
		{
			Finished = true;
			Abort = true;
		}
		this.Rating = Rating;
		this.Difficulty = Difficulty + ContextObject.GetIntProperty("ProselytizationSifrahDifficultyModifier");
		int num2 = Math.Max(Difficulty / 5, 4);
		int num3 = 4;
		if (Difficulty >= 20)
		{
			num3++;
		}
		if (Difficulty >= 40)
		{
			num3++;
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
		if (list.Count < num2 && Rating >= Difficulty + 3)
		{
			list.Add(new SocialSifrahTokenSociableChat());
		}
		if (list.Count < num2 && Rating >= Difficulty + 2)
		{
			list.Add(new SocialSifrahTokenListenSympathetically());
		}
		if (list.Count < num2 && Rating >= Difficulty + 4)
		{
			list.Add(new SocialSifrahTokenDebateRationally());
		}
		if (list.Count < num2 && Rating >= Difficulty + 1)
		{
			list.Add(new SocialSifrahTokenFlirtSuggestively());
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
			list.Add(new SocialSifrahTokenInvokeAncientCompacts());
		}
		if (list.Count < num2 && The.Player.HasSkill("Tinkering_Repair") && ContextObject.HasPart<Robot>())
		{
			list.Add(new SocialSifrahTokenOfferMaintenanceServices());
		}
		if (list.Count < num2 && Rating >= Difficulty - 1)
		{
			list.Add(new SocialSifrahTokenFlatterInsincerely());
		}
		if (list.Count < num2 && Rating >= Difficulty - 2)
		{
			list.Add(new SocialSifrahTokenTellAnInspiringTale());
		}
		if (list.Count < num2 && Rating >= Difficulty - 3)
		{
			list.Add(new SocialSifrahTokenRailAgainstInjustice());
		}
		if (list.Count < num2 && The.Player.Stat("Level") + Rating > ContextObject.Stat("Level") + Difficulty)
		{
			list.Add(new SocialSifrahTokenBoastOfAccomplishments());
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
		if (list.Count < num2)
		{
			Scanning.Scan scanTypeFor = Scanning.GetScanTypeFor(ContextObject);
			if (Scanning.HasScanningFor(The.Player, scanTypeFor))
			{
				list.Add(new SocialSifrahTokenScanning(scanTypeFor));
			}
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
			if (Rating >= Difficulty / 2 && (ContextObject.IsMemberOfFaction("Mechanimists") || ContextObject.IsMemberOfFaction("MechaSub") || Factions.GetFeelingFactionToObject("Mechanimists", ContextObject) >= 0))
			{
				list.Add(new SocialSifrahTokenReadFromTheCanticlesChromaic());
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
			Popup.ShowFail("You have no usable options to employ for proselytizing " + ContextObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", giving you no chance of success. You can remedy this situation by improving your Ego and social skills, or by obtaining items useful in social situations.");
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
			int num7 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.Failure);
			int num8 = GetGameCount(Slots: Slots.Count, Tokens: Tokens.Count, Outcome: GameOutcome.CriticalFailure);
			int num9 = Tokens.Count + num7 / 2 + num8 + gameCount / 10;
			slots2 = Slots.Count;
			tokens = Tokens.Count;
			asMaster = true;
			int gameCount2 = GetGameCount(null, slots2, tokens, null, asMaster);
			asMaster = true;
			bool? asMaster2 = false;
			int gameCount3 = GetGameCount(null, null, null, asMaster2, asMaster);
			asMaster2 = true;
			asMaster = true;
			int gameCount4 = GetGameCount(null, null, null, asMaster, asMaster2);
			if (gameCount2 + gameCount3 / 50 + gameCount4 / 75 >= num9)
			{
				AsMaster = true;
			}
		}
		if (!AsMaster)
		{
			return;
		}
		string sifrahRecruitmentAuto = Options.SifrahRecruitmentAuto;
		if (!(sifrahRecruitmentAuto == "Always"))
		{
			if (sifrahRecruitmentAuto == "Never")
			{
				return;
			}
			DialogResult dialogResult = Popup.ShowYesNoCancel("You have mastered proselytization at this level of discourse. Do you want to perform detailed proselytization anyway, with an enhanced chance of exceptional success? If you answer 'No', you will automatically succeed at proselytization if that is possible.");
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
			switch (Popup.ShowYesNoCancel("Do you want to finish the proselytization attempt as matters stand?"))
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
		return Popup.ShowYesNo("Exiting now will finish the proselytization attempt as matters stand. Are you sure you want to exit?") == DialogResult.Yes;
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
		if (Bypass)
		{
			PlayOutcomeSound(GameOutcome.Success);
			ResultSuccess(ContextObject);
			return;
		}
		GameOutcome gameOutcome = DetermineOutcome();
		PlayOutcomeSound(gameOutcome);
		bool mastered = false;
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
			if (PercentSolved >= 100)
			{
				mastered = true;
			}
			ResultSuccess(ContextObject);
			int num3 = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty / 3;
			if (PercentSolved < 100)
			{
				num3 = num3 * (100 - (100 - PercentSolved) * 3) / 100;
			}
			int num4 = num3 - ContextObject.GetIntProperty("ProselytizationXPAwarded");
			if (num4 > 0)
			{
				ContextObject.SetIntProperty("ProselytizationXPAwarded", num3);
				The.Player.AwardXP(num4, -1, 0, int.MaxValue, null, ContextObject);
			}
			break;
		}
		case GameOutcome.CriticalSuccess:
		{
			if (PercentSolved >= 100)
			{
				mastered = true;
			}
			ResultExceptionalSuccess(ContextObject);
			SocialSifrah.AwardInsight();
			int num = (Tokens.Count * Tokens.Count - Slots.Count) * Difficulty;
			if (PercentSolved < 100)
			{
				num = num * (100 - (100 - PercentSolved) * 3) / 100;
			}
			int num2 = num - ContextObject.GetIntProperty("ProselytizationXPAwarded");
			if (num2 > 0)
			{
				ContextObject.SetIntProperty("ProselytizationXPAwarded", num);
				The.Player.AwardXP(num2, -1, 0, int.MaxValue, null, ContextObject);
			}
			break;
		}
		}
		SifrahGame.RecordOutcome(this, gameOutcome, Slots.Count, Tokens.Count, AsMaster, mastered);
	}

	public virtual void ResultCriticalFailure(GameObject ContextObject)
	{
		Popup.Show(ContextObject.Does("are") + " offended by your impertinence.");
		ContextObject.AddOpinion<OpinionPoorReasoning>(The.Player);
	}

	public virtual void ResultFailure(GameObject ContextObject)
	{
		Popup.Show(ContextObject.Does("are") + " unconvinced by your pleas.");
	}

	public virtual void ResultPartialSuccess(GameObject ContextObject)
	{
		Popup.Show(ContextObject.Does("are") + " unconvinced by your pleas, but interested in hearing more.");
		ContextObject.ModIntProperty("ProselytizationSifrahDifficultyModifier", -Stat.Random(1, 5));
	}

	public virtual void ResultSuccess(GameObject ContextObject)
	{
		if (!ContextObject.ApplyEffect(new Proselytized(The.Player)))
		{
			Popup.Show(ContextObject.Does("are") + " sympathetic, but unable to join you.");
		}
	}

	public virtual void ResultExceptionalSuccess(GameObject ContextObject)
	{
		ContextObject.LikeBetter(The.Player, 20);
		if (ContextObject.ApplyEffect(new Proselytized(The.Player)))
		{
			ContextObject.AwardXP(ContextObject.Stat("Level") * 100, -1, 0, int.MaxValue, null, ContextObject, null, The.Player);
		}
		else
		{
			Popup.Show(ContextObject.Does("are") + " sympathetic, but unable to join you.");
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
		double num4 = 0.01 + (double)Powerup * 0.01;
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
				Success++;
			}
		}
	}
}

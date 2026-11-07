using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qud.API;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class Leveler : IPart
{
	[NonSerialized]
	public static bool PlayerLedPrompt;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AwardedXPEvent.ID)
		{
			return ID == PooledEvent<StatChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(AwardedXPEvent E)
	{
		int newValue = E.Actor.Stat("XP");
		while (valuePassed(E.AmountBefore, newValue, GetXPForLevel(ParentObject.Stat("Level") + 1)) && LevelUp(E.Kill, E.InfluencedBy, E.ZoneID))
		{
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Toughness")
		{
			int scoreModifier = Stat.GetScoreModifier(E.OldValue);
			int scoreModifier2 = Stat.GetScoreModifier(E.NewValue);
			int num = E.NewValue - E.OldValue + (scoreModifier2 - scoreModifier) * (ParentObject.Stat("Level") - 1);
			if (num != 0)
			{
				Statistic stat = ParentObject.GetStat("Hitpoints");
				if (stat != null)
				{
					int baseValue = stat.BaseValue;
					if (num > 0)
					{
						stat.BaseValue += num;
					}
					if (stat.Penalty != 0)
					{
						int num2 = baseValue + num;
						double num3 = (double)stat.Penalty / (double)baseValue;
						int penalty = (int)Math.Min(Math.Round((double)num2 * num3, MidpointRounding.AwayFromZero), num2 - 1);
						stat.Penalty = penalty;
					}
					if (num < 0)
					{
						stat.BaseValue += num;
					}
				}
			}
		}
		else if (E.Name == "Intelligence" && E.Type == "BaseValue")
		{
			int intProperty = ParentObject.GetIntProperty("PeakIntelligence", E.OldValue);
			if (E.NewValue > intProperty && ParentObject.HasStat("SP"))
			{
				int num4 = (E.NewValue - intProperty) * 4 * (ParentObject.Stat("Level") - 1);
				if (num4 > 0)
				{
					ParentObject.GetStat("SP").BaseValue += num4;
				}
				if (E.OldValue != 0 && E.NewValue > intProperty)
				{
					ParentObject.SetIntProperty("PeakIntelligence", E.NewValue);
				}
			}
		}
		return base.HandleEvent(E);
	}

	private bool valuePassed(int oldValue, int newValue, int threshold)
	{
		if (oldValue < threshold)
		{
			return newValue >= threshold;
		}
		return false;
	}

	public static int GetXPForLevel(int L)
	{
		if (L <= 1)
		{
			return 0;
		}
		return (int)(Math.Floor(Math.Pow(L, 3.0) * 15.0) + 100.0);
	}

	public void GetEntryDice(out string BaseHPGain, out string BaseSPGain, out string BaseMPGain)
	{
		GenotypeEntry genotypeEntry = GenotypeFactory.RequireGenotypeEntry(ParentObject.GetGenotype());
		SubtypeEntry subtypeEntry = SubtypeFactory.GetSubtypeEntry(ParentObject.GetSubtype());
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseHPGain))
		{
			BaseHPGain = subtypeEntry.BaseHPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseHPGain))
		{
			BaseHPGain = genotypeEntry.BaseHPGain;
		}
		else
		{
			BaseHPGain = "1-4";
		}
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseSPGain))
		{
			BaseSPGain = subtypeEntry.BaseSPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseSPGain))
		{
			BaseSPGain = genotypeEntry.BaseSPGain;
		}
		else
		{
			BaseSPGain = "50";
		}
		if (!string.IsNullOrEmpty(subtypeEntry?.BaseMPGain))
		{
			BaseMPGain = subtypeEntry.BaseMPGain;
		}
		else if (!string.IsNullOrEmpty(genotypeEntry.BaseMPGain))
		{
			BaseMPGain = genotypeEntry.BaseMPGain;
		}
		else
		{
			BaseMPGain = "1";
		}
	}

	public int RollHP(string BaseHPGain)
	{
		return Math.Max(Stat.RollLevelupChoice(BaseHPGain) + ParentObject.StatMod("Toughness"), 1);
	}

	public int RollSP(string BaseSPGain)
	{
		int num = Stat.RollLevelupChoice(BaseSPGain);
		num += (ParentObject.BaseStat("Intelligence") - 10) * 4;
		return GetLevelUpSkillPointsEvent.GetFor(ParentObject, num);
	}

	public int RollMP(string BaseMPGain)
	{
		return Stat.RollLevelupChoice(BaseMPGain);
	}

	public void AppendStatMessage(StringBuilder sb, string Type, int Amount, bool Pluralize = true)
	{
		if (sb != null)
		{
			int num = Math.Abs(Amount);
			sb.CompoundItVerb(ParentObject, (Amount > 0) ? "gain" : "lose", '\n').Append(" {{rules|").Append(num)
				.Append("}} ")
				.Append(Type);
			if (Pluralize && num != 1)
			{
				sb.Append('s');
			}
		}
	}

	public void AddHitpoints(StringBuilder sb, int HPGain)
	{
		if (HPGain != 0)
		{
			ParentObject.GetStat("Hitpoints").BaseValue += HPGain;
			AppendStatMessage(sb, "hitpoint", HPGain);
		}
	}

	public void AddSkillPoints(StringBuilder sb, int SPGain)
	{
		if (SPGain != 0)
		{
			ParentObject.GetStat("SP").BaseValue += SPGain;
			AppendStatMessage(sb, "Skill Point", SPGain);
		}
	}

	public void AddMutationPoints(StringBuilder sb, int MPGain)
	{
		if (MPGain != 0)
		{
			ParentObject.GainMP(MPGain);
			AppendStatMessage(sb, "Mutation Point", MPGain);
		}
	}

	public void AddAttributePoints(StringBuilder sb, int APGain)
	{
		if (APGain != 0)
		{
			ParentObject.GetStat("AP").BaseValue += APGain;
			AppendStatMessage(sb, "Attribute Point", APGain);
		}
	}

	public void AddAttributeBonus(StringBuilder sb, int ABGain)
	{
		if (ABGain != 0)
		{
			ParentObject.GetStat("Strength").BaseValue += ABGain;
			ParentObject.GetStat("Intelligence").BaseValue += ABGain;
			ParentObject.GetStat("Willpower").BaseValue += ABGain;
			ParentObject.GetStat("Agility").BaseValue += ABGain;
			ParentObject.GetStat("Toughness").BaseValue += ABGain;
			ParentObject.GetStat("Ego").BaseValue += ABGain;
			AppendStatMessage(sb, "to each attribute", ABGain, Pluralize: false);
		}
	}

	public bool UseDetailedPrompt()
	{
		if (!ParentObject.IsPlayer())
		{
			if (PlayerLedPrompt)
			{
				return ParentObject.IsPlayerLed();
			}
			return false;
		}
		return true;
	}

	public bool LevelUp(GameObject Kill = null, GameObject InfluencedBy = null, string ZoneID = null, IEvent ParentEvent = null)
	{
		int num = ParentObject.Stat("Level") + 1;
		bool flag = ParentObject.IsPlayer();
		using BeforeLevelGainedEvent beforeLevelGainedEvent = BeforeLevelGainedEvent.FromPool(ParentObject, Kill, InfluencedBy, num, UseDetailedPrompt());
		if (beforeLevelGainedEvent.Detail)
		{
			beforeLevelGainedEvent.Message.Append(flag ? "You" : ParentObject.T(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true)).Append(ParentObject.Has).Append(" gained a level!")
				.CompoundItVerb(ParentObject, "are")
				.Append(" now level {{C|")
				.Append(num)
				.Append("}}!");
		}
		if (!beforeLevelGainedEvent.Check())
		{
			return false;
		}
		ParentObject.GetStat("Level").BaseValue++;
		GetEntryDice(out var BaseHPGain, out var BaseSPGain, out var BaseMPGain);
		GetLevelUpDiceEvent.GetFor(ParentObject, num, ref BaseHPGain, ref BaseSPGain, ref BaseMPGain);
		bool flag2 = ParentObject.IsMutant();
		int HitPoints = RollHP(BaseHPGain);
		int SkillPoints = RollSP(BaseSPGain);
		int MutationPoints = (flag2 ? RollMP(BaseMPGain) : 0);
		int AttributePoints = ((num % 3 == 0 && num % 6 != 0) ? 1 : 0);
		int AttributeBonus = ((num % 3 == 0 && num % 6 == 0) ? 1 : 0);
		int RapidAdvancement = ((flag2 && (num + 5) % 10 == 0 && !ParentObject.IsEsper()) ? 3 : 0);
		GetLevelUpPointsEvent.GetFor(ParentObject, num, ref HitPoints, ref SkillPoints, ref MutationPoints, ref AttributePoints, ref AttributeBonus, ref RapidAdvancement);
		StringBuilder sb = (beforeLevelGainedEvent.Detail ? beforeLevelGainedEvent.Message : null);
		AddHitpoints(sb, HitPoints);
		AddSkillPoints(sb, SkillPoints);
		AddMutationPoints(sb, MutationPoints);
		AddAttributePoints(sb, AttributePoints);
		AddAttributeBonus(sb, AttributeBonus);
		if (flag)
		{
			IComponent<GameObject>.PlayUISound("sfx_characterMod_level_gain");
		}
		else
		{
			PlayWorldSound("sfx_npc_level gain");
		}
		RenderParticlesAt(ParentObject.CurrentCell);
		if (beforeLevelGainedEvent.Detail && !beforeLevelGainedEvent.Message.IsNullOrEmpty())
		{
			Popup.Show(beforeLevelGainedEvent.Message.ToString());
		}
		else
		{
			DidX("gain", "a level", "!", null, null, ParentObject);
		}
		Leveler.RapidAdvancement(RapidAdvancement, ParentObject);
		SifrahInsights();
		ItemNaming.Opportunity(ParentObject, Kill, InfluencedBy, ZoneID, "Level", 6, 0, 0, 3);
		AfterLevelGainedEvent.Send(beforeLevelGainedEvent);
		ParentEvent?.ProcessChildEvent(beforeLevelGainedEvent);
		return true;
	}

	public void RenderParticlesAt(Cell C)
	{
		if (C != null && C.IsVisible())
		{
			ParticleManager particleManager = XRLCore.ParticleManager;
			particleManager.AddSinusoidal("&W*", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
			particleManager.AddSinusoidal("&Y*", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
			particleManager.AddSinusoidal("&W\r", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
			particleManager.AddSinusoidal("&Y\u000e", C.X, C.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
		}
	}

	public static void RapidAdvancement(int Amount, GameObject ParentObject)
	{
		if (Amount <= 0)
		{
			return;
		}
		SyncMutationLevelsEvent.Send(ParentObject);
		string text = GetMutationTermEvent.GetFor(ParentObject);
		bool flag = ParentObject.IsPlayer() && ParentObject.Stat("MP") >= 4;
		bool flag2 = false;
		if (flag && Popup.ShowYesNo("Your genome enters an excited state! Would you like to spend {{rules|4}} mutation points to buy " + Grammar.A(text) + " before rapidly mutating?", "Sounds/UI/ui_notification_question", AllowEscape: false) == DialogResult.Yes)
		{
			flag2 = MutationsAPI.BuyRandomMutation(ParentObject, 4, Confirm: false, text);
		}
		List<BaseMutation> list = (from m in ParentObject.GetPhysicalMutations()
			where m.CanLevel()
			select m).ToList();
		if (list.Count > 0)
		{
			if (!flag && ParentObject.IsPlayer())
			{
				Popup.Show("Your genome enters an excited state!");
			}
			if (ParentObject.IsPlayer())
			{
				string[] options = list.Select((BaseMutation m) => m.GetDisplayName() + " ({{C|" + m.Level + "}})").ToArray();
				int index = Popup.PickOption("Choose a physical " + text + " to rapidly advance.", null, "", "Sounds/Misc/sfx_characterMod_mutation_windowPopup", options);
				Popup.Show("You have rapidly advanced " + list[index].GetDisplayName() + " by " + Grammar.Cardinal(Amount) + " ranks to rank {{C|" + (list[index].Level + Amount) + "}}!", null, "Sounds/Misc/sfx_characterMod_mutation_rankUp_quickSuccession");
				list[index].RapidLevel(Amount);
			}
			else
			{
				list.GetRandomElement().RapidLevel(Amount);
			}
		}
		else if (flag2)
		{
			Popup.Show("You have no physical " + Grammar.Pluralize(text) + " to rapidly advance!");
		}
	}

	public void SifrahInsights()
	{
		if (SifrahGame.Installed)
		{
			if (ParentObject.HasSkill("Tinkering_Tinker1") && 10.in100())
			{
				TinkeringSifrah.AwardInsight();
			}
			if (ParentObject.HasSkill("Persuasion_Proselytize") && 10.in100())
			{
				SocialSifrah.AwardInsight();
			}
			if (ParentObject.HasSkill("Customs_Sharer") && 10.in100())
			{
				RitualSifrah.AwardInsight();
			}
			if (ParentObject.HasSkill("Discipline_Meditate") && ParentObject.GetPsychicGlimmer() > 0 && 10.in100())
			{
				PsionicSifrah.AwardInsight();
			}
		}
	}
}

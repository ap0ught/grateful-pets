using System;
using XRL.Core;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class AdrenalControl2 : BaseMutation
{
	public int CurrentBonus;

	public Guid ReleaseAdrenalineAbilityID = Guid.Empty;

	public int Duration;

	public int CurrentMLBonus;

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("Duration", GetQuicknessDuration(Level));
		stats.Set("QuicknessBonus", GetQuicknessBonus(Level), !stats.mode.Contains("ability"));
		stats.Set("MutationBonus", GetMutationBonus(Level), !stats.mode.Contains("ability"));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ReleaseAdrenalineAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 4 && IsMyActivatedAbilityAIUsable(ReleaseAdrenalineAbilityID))
		{
			E.Add("CommandReleaseAdrenaline");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		Registrar.Register("CommandReleaseAdrenaline");
		base.Register(Object, Registrar);
	}

	public override string GetDescription()
	{
		return "You regulate your body's release of adrenaline.";
	}

	public int GetQuicknessBonus(int Level)
	{
		return 9 + Level;
	}

	public int GetMutationBonus(int Level)
	{
		return Level / 3 + 1;
	}

	public int GetQuicknessDuration(int Level)
	{
		return 20;
	}

	public int GetCooldown(int Level)
	{
		return 200;
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "You can increase your body's adrenaline flow for " + GetQuicknessDuration(Level) + " rounds.\n";
		text = text + "While it's flowing, you gain +{{C|" + GetQuicknessBonus(Level) + "}} quickness and other physical mutations gain +{{C|" + GetMutationBonus(Level) + "}} rank.\n";
		return text + "Cooldown: " + GetCooldown(Level) + " rounds";
	}

	public void SyncBonus()
	{
		int num = GetQuicknessBonus(Level);
		if (Duration <= 0)
		{
			num = 0;
		}
		if (ParentObject.HasStat("Speed") && num != CurrentBonus)
		{
			ParentObject.GetStat("Speed").Bonus -= CurrentBonus;
			CurrentBonus = num;
			ParentObject.GetStat("Speed").Bonus += CurrentBonus;
		}
		int num2 = GetMutationBonus(Level);
		if (Duration <= 0)
		{
			num2 = 0;
		}
		if (num2 != CurrentMLBonus)
		{
			ParentObject.ModIntProperty("AdrenalLevelModifier", -CurrentMLBonus);
			CurrentMLBonus = num2;
			ParentObject.ModIntProperty("AdrenalLevelModifier", CurrentMLBonus);
			SyncMutationLevelsEvent.Send(ParentObject);
		}
		if (num2 == 0 || num == 0)
		{
			ParentObject.RemoveEffect(typeof(AdrenalControl2Boosted));
			return;
		}
		AdrenalControl2Boosted adrenalControl2Boosted = ParentObject.GetEffect<AdrenalControl2Boosted>();
		if (adrenalControl2Boosted == null)
		{
			adrenalControl2Boosted = new AdrenalControl2Boosted(this);
			ParentObject.ApplyEffect(adrenalControl2Boosted);
		}
		adrenalControl2Boosted.AppliedMutationBonus = num2;
		adrenalControl2Boosted.AppliedSpeedBonus = num;
		adrenalControl2Boosted.Duration = Duration;
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = 60 - Duration / 2;
			int num2 = XRLCore.CurrentFrame % num;
			if (num2 > 21 && num2 < 31)
			{
				E.Tile = null;
				E.RenderString = "\u0003";
				E.ColorString = "&r";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (Duration > 0)
			{
				Duration--;
				if (Duration == 0 && ParentObject.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("Your adrenaline subsides.");
				}
			}
			SyncBonus();
		}
		else if (E.ID == "CommandReleaseAdrenaline")
		{
			if (ParentObject.IsPlayer())
			{
				IComponent<GameObject>.AddPlayerMessage("{{G|Your adrenaline starts to flow.}}");
			}
			Duration = GetQuicknessDuration(Level);
			CooldownMyActivatedAbility(ReleaseAdrenalineAbilityID, GetCooldown(Level));
			SyncBonus();
			ParentObject?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		SyncBonus();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ReleaseAdrenalineAbilityID = AddMyActivatedAbility("Release Adrenaline", "CommandReleaseAdrenaline", "Physical Mutations", null, "å", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		Duration = 0;
		SyncBonus();
		RemoveMyActivatedAbility(ref ReleaseAdrenalineAbilityID);
		return base.Unmutate(GO);
	}
}

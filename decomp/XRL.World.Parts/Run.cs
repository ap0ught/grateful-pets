using System;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Run : IPart
{
	public const string SUPPORT_TYPE = "Run";

	public Guid ActivatedAbilityID = Guid.Empty;

	public string ActiveAbilityName;

	public string ActiveVerb;

	public string ActiveEffectDisplayName;

	public string ActiveEffectMessageName;

	public int ActiveEffectDuration;

	public bool ActiveSpringingEffective;

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveAbilityListEvent.ID && ID != SingletonEvent<CommandTakeActionEvent>.ID && ID != PooledEvent<NeedPartSupportEvent>.ID && ID != PooledEvent<CommandEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		GetRunningBehaviorEvent.Retrieve(ParentObject, out var _, out var _, out var _, out var _, out var _, out var SpringingEffective, stats);
		if (ParentObject.HasPart<Tactics_Hurdle>())
		{
			stats.Set("DV", 0, changes: true, 5);
			stats.postfix += "\nDV penalty and no jumping removed due to Hurdle skill.";
		}
		else
		{
			stats.Set("DV", -5);
		}
		if (ParentObject.HasPart<Pistol_SlingAndRun>())
		{
			stats.Set("MissileAccuracy", "Pistol");
			stats.postfix += "\nPistol accuracy penalty removed due to Sling and Run skill.";
		}
		else
		{
			stats.Set("MissileAccuracy", "");
		}
		Running.GetMovespeedMultiplier(ParentObject, SpringingEffective, stats);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), 100);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance >= 6 && !ActiveAbilityName.IsNullOrEmpty() && !E.Actor.IsFlying && !E.Actor.HasEffect<Running>() && E.Actor.CanChangeMovementMode(ActiveEffectMessageName) && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandToggleRunning");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "Run" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandTakeActionEvent E)
	{
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = IsRunning();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == "CommandToggleRunning" && !ToggleRunning())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void SyncAbility(bool Silent = false)
	{
		GetRunningBehaviorEvent.Retrieve(ParentObject, out var AbilityName, out var Verb, out var EffectDisplayName, out var EffectMessageName, out var EffectDuration, out var SpringingEffective);
		if (ActivatedAbilityID == Guid.Empty || AbilityName != ActiveAbilityName || Verb != ActiveVerb || EffectDisplayName != ActiveEffectDisplayName || EffectMessageName != ActiveEffectMessageName || EffectDuration != ActiveEffectDuration || SpringingEffective != ActiveSpringingEffective)
		{
			bool flag = ActiveAbilityName == AbilityName;
			RemoveMyActivatedAbility(ref ActivatedAbilityID);
			if (!flag)
			{
				ParentObject.RemoveAllEffects<Running>();
			}
			ActiveAbilityName = AbilityName;
			ActiveVerb = Verb;
			ActiveEffectDisplayName = EffectDisplayName;
			ActiveEffectMessageName = EffectMessageName;
			ActiveEffectDuration = EffectDuration;
			ActiveSpringingEffective = SpringingEffective;
			if (!ActiveAbilityName.IsNullOrEmpty())
			{
				ActivatedAbilityID = AddMyActivatedAbility(ActiveAbilityName, "CommandToggleRunning", "Maneuvers", null, "\u001a", null, Toggleable: true, DefaultToggleState: false, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent | flag);
			}
		}
	}

	public static void SyncAbility(GameObject who, bool Silent = false)
	{
		who.GetPart<Run>()?.SyncAbility(Silent);
	}

	public bool ToggleRunning()
	{
		if (!IsRunning())
		{
			return StartRunning();
		}
		return StopRunning();
	}

	public bool IsRunning()
	{
		return ParentObject.HasEffect<Running>();
	}

	public bool StartRunning()
	{
		if (IsRunning())
		{
			return false;
		}
		if (!ParentObject.CheckFrozen())
		{
			return false;
		}
		if (ActiveAbilityName.IsNullOrEmpty())
		{
			return false;
		}
		if (ParentObject.OnWorldMap())
		{
			Popup.ShowFail("You cannot " + ActiveVerb + " on the world map.");
			return false;
		}
		if (!ParentObject.CanChangeMovementMode(ActiveEffectMessageName, ShowMessage: true))
		{
			return false;
		}
		ParentObject.ApplyEffect(new Running(ActiveEffectDuration, ActiveEffectDisplayName, ActiveEffectMessageName, ActiveSpringingEffective));
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = true;
		}
		CooldownMyActivatedAbility(ActivatedAbilityID, 100, null, "Agility");
		return true;
	}

	public bool StopRunning()
	{
		if (!IsRunning())
		{
			return false;
		}
		ParentObject.RemoveAllEffects<Running>();
		ActivatedAbilityEntry activatedAbilityEntry = MyActivatedAbility(ActivatedAbilityID);
		if (activatedAbilityEntry != null)
		{
			activatedAbilityEntry.ToggleState = false;
		}
		return true;
	}
}

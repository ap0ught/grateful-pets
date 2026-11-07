using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LifeDrain : BaseMutation
{
	public bool RealityDistortionBased = true;

	public LifeDrain()
	{
		Type = "Mental";
	}

	public override string GetDescription()
	{
		return "You bond with a nearby organic creature and leech its life force.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Mental attack versus an organic creature\n";
		text = text + "Drains {{rules|" + Level + "}} hit " + ((Level == 1) ? "point" : "points") + " per round\n";
		text += "Target gets a mental save to resist damage each round\n";
		text += "Duration: 20 rounds\n";
		return text + "Cooldown: 200 rounds\n";
	}

	public int GetCooldown(int Level)
	{
		return 200;
	}

	public int GetDuration(int Level)
	{
		return 20;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		if (Level == 1)
		{
			stats.Set("HP", "1 hit point");
		}
		else
		{
			stats.Set("HP", Level + " hit points");
		}
		stats.Set("Duration", GetDuration(Level));
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), GetCooldown(Level));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AIGetOffensiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance <= 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && IsValidTarget(E.Target) && (!RealityDistortionBased || (CheckMyRealityDistortionAdvisability() && IComponent<GameObject>.CheckRealityDistortionAdvisability(E.Target, null, E.Actor, null, this))))
		{
			E.Add("CommandLifeDrain");
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandLifeDrain");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandLifeDrain")
		{
			if (RealityDistortionBased && !ParentObject.IsRealityDistortionUsable())
			{
				RealityStabilized.ShowGenericInterdictMessage(ParentObject);
				return false;
			}
			Cell cell = PickDirection("Syphon Vim");
			if (cell != null)
			{
				GameObject combatTarget = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 5, null, null, null, null, null, AllowInanimate: false);
				if (combatTarget == ParentObject)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot syphon vim from " + ParentObject.itself + ".");
					}
					return false;
				}
				if (combatTarget == null)
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("No one is there for you to syphon vim from.");
					}
					return false;
				}
				if (!IsValidTarget(combatTarget))
				{
					if (ParentObject.IsPlayer())
					{
						Popup.ShowFail("You cannot syphon vim from " + combatTarget.t() + ".");
					}
					return false;
				}
				if (RealityDistortionBased)
				{
					Event e = Event.New("InitiateRealityDistortionTransit", "Object", ParentObject, "Mutation", this, "Cell", cell);
					if (!ParentObject.FireEvent(e, E) || !cell.FireEvent(e, E))
					{
						return false;
					}
				}
				combatTarget.ApplyEffect(new XRL.World.Effects.LifeDrain(20, Level, Level.ToString(), ParentObject, RealityDistortionBased));
				UseEnergy(1000, "Mental Mutation SyphonVim");
				CooldownMyActivatedAbility(ActivatedAbilityID, 200);
			}
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Syphon Vim", "CommandLifeDrain", "Mental Mutations", null, "í", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public static bool IsValidTarget(GameObject Object)
	{
		if (!GameObject.Validate(ref Object))
		{
			return false;
		}
		if (!Object.IsCombatObject())
		{
			return false;
		}
		if (!Object.HasHitpoints())
		{
			return false;
		}
		if (!Object.IsOrganic)
		{
			return false;
		}
		return true;
	}
}

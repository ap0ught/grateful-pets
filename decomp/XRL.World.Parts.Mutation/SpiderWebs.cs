using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SpiderWebs : BaseMutation
{
	public int SpinTimer;

	public bool Active;

	public static readonly int COOLDOWN = 50;

	public static readonly int DURATION = 4;

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("SpinDuration", DURATION);
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID), COOLDOWN);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetMovementAbilityListEvent.ID && ID != AIGetOffensiveAbilityListEvent.ID)
		{
			return ID == LeftCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetMovementAbilityListEvent E)
	{
		if (IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSpinSimpleWeb");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveAbilityListEvent E)
	{
		if (E.Distance > 1 && IsMyActivatedAbilityAIUsable(ActivatedAbilityID))
		{
			E.Add("CommandSpinSimpleWeb");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeftCellEvent E)
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			if (E.Cell.OnWorldMap())
			{
				SpinTimer = -1;
			}
			else
			{
				SpinTimer--;
			}
			if (SpinTimer < 0)
			{
				ToggleMyActivatedAbility(ActivatedAbilityID);
			}
			else
			{
				GameObject gameObject = null;
				gameObject = ((!ParentObject.HasEffect<Phased>()) ? GameObject.Create("Web") : GameObject.Create("PhaseWeb"));
				gameObject.GetPart<Sticky>().SaveTarget = 15 + Level;
				E.Cell.AddObject(gameObject);
				DidX("spin", gameObject.a + gameObject.ShortDisplayName);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyStuck");
		Registrar.Register("CanApplyStuck");
		Registrar.Register("CommandSpinSimpleWeb");
		Registrar.Register("VillageInit");
		base.Register(Object, Registrar);
	}

	public override string GetLevelText(int Level)
	{
		return "You bear two spinnerets with which you spin a sticky silk.\n";
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyStuck" || E.ID == "CanApplyStuck")
		{
			return false;
		}
		if (E.ID == "CommandSpinSimpleWeb")
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
			{
				SpinTimer = DURATION;
			}
			UseEnergy(1000, "Physical Mutation Spin Web");
			CooldownMyActivatedAbility(ActivatedAbilityID, COOLDOWN);
		}
		else if (E.ID == "VillageInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = AddMyActivatedAbility("Spin Webs", "CommandSpinSimpleWeb", "Physical Mutations", null, "#", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

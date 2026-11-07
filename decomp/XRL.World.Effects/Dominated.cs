using System;
using ConsoleLib.Console;
using UnityEngine;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class Dominated : Effect
{
	public GameObject Dominator;

	public bool RoboDom;

	public Guid AAID;

	public string DominatorDebugName;

	public bool FromOriginalPlayerBody;

	[NonSerialized]
	public bool BeingRemovedBySource;

	[NonSerialized]
	public bool Metempsychosis;

	public Dominated()
	{
		DisplayName = "dominated";
	}

	public Dominated(GameObject Dominator, int Duration, bool RoboDom = false)
		: this()
	{
		this.Dominator = Dominator;
		base.Duration = Duration;
		DominatorDebugName = Dominator?.DebugName;
		FromOriginalPlayerBody = Dominator.IsOriginalPlayerBody();
		this.RoboDom = RoboDom;
	}

	public override int GetEffectType()
	{
		return 32770;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Under someone else's control.";
	}

	public override string GetDescription()
	{
		return "dominated (" + Duration.Things("turn") + " remaining)";
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.Validate(ref Dominator))
		{
			return false;
		}
		if (RoboDom)
		{
			if (!Object.FireEvent("ApplyRoboDomination"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "RoboDomination", this))
			{
				return false;
			}
		}
		else
		{
			if (!Object.FireEvent("ApplyDomination"))
			{
				return false;
			}
			if (!ApplyEffectEvent.Check(Object, "Domination", this))
			{
				return false;
			}
		}
		if (!Dominator.ApplyEffect(new Dominating(Object)))
		{
			return false;
		}
		try
		{
			Object.FireEvent(Event.New("DominationStarted", "Subject", Object, "Dominator", Dominator, "Effect", this));
		}
		catch (Exception ex)
		{
			Debug.LogError("Exception in DominationStarted event: " + ex.ToString());
		}
		AAID = Object.AddActivatedAbility("End Domination", "CommandEndDomination", "Mental Mutations", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: false, -1, null, Renderable.UITile("Abilities/abil_end_domination.bmp", 'm', 'r'));
		Object.StopFighting(Dominator);
		Dominator.StopFighting(Object);
		foreach (GameObject item in The.ZoneManager.FindObjectsReadonly((GameObject o) => o.IsAlliedTowards(Dominator)))
		{
			item.StopFighting(Object);
			Object.StopFighting(item);
		}
		return true;
	}

	public override void Remove(GameObject Object)
	{
		try
		{
			Object?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_mental_generic_deactivate");
			Event obj = Event.New("DominationEnded");
			obj.SetParameter("Subject", Object);
			obj.SetParameter("Dominator", Dominator);
			obj.SetParameter("Effect", this);
			if (Metempsychosis)
			{
				obj.SetFlag("Metempsychosis", State: true);
			}
			Object.FireEvent(obj);
		}
		catch (Exception x)
		{
			MetricsManager.LogError("exception from DominationEnded event", x);
		}
		try
		{
			Object.RemoveActivatedAbility(ref AAID);
		}
		catch (Exception x2)
		{
			MetricsManager.LogError("exception removing End Domination ability", x2);
		}
		if (BeingRemovedBySource)
		{
			return;
		}
		try
		{
			if (Object.OnWorldMap())
			{
				Object.PullDown();
			}
			if (GameObject.Validate(ref Dominator) && Dominator.HasHitpoints())
			{
				Dominator.FireEvent("DominationBroken");
			}
			else if (Object.IsPlayer())
			{
				Domination.Metempsychosis(Object, FromOriginalPlayerBody);
			}
		}
		catch (Exception x3)
		{
			MetricsManager.LogError("exception in Domination cleanupestatus", x3);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != OnDestroyObjectEvent.ID)
		{
			return ID == PooledEvent<JoinPartyLeaderPossibleEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Duration > 0)
		{
			if (!GameObject.Validate(ref Dominator))
			{
				MetricsManager.LogError("ending domination because of loss of dominator, was " + (DominatorDebugName ?? "null"));
				Object.RemoveEffect(this);
			}
			else if (Duration != 9999)
			{
				Duration--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		Object.RemoveEffect(this);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(JoinPartyLeaderPossibleEvent E)
	{
		return E.Result = false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeDie");
		Registrar.Register("CommandEndDomination");
		Registrar.Register("InterruptDomination");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie" || E.ID == "CommandEndDomination")
		{
			Object.RemoveEffect(this);
		}
		else if (E.ID == "InterruptDomination" && Object.FireEvent("ChainInterruptDomination"))
		{
			Object.RemoveEffect(this);
			return false;
		}
		return base.FireEvent(E);
	}
}

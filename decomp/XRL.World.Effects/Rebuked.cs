using System;
using HistoryKit;
using Qud.API;
using XRL.World.AI;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Rebuked : Effect
{
	public GameObject Rebuker;

	public Rebuked()
	{
		DisplayName = "{{C|rebuked}}";
		Duration = 1;
	}

	public Rebuked(GameObject Rebuker)
		: this()
	{
		this.Rebuker = Rebuker;
	}

	public override int GetEffectType()
	{
		return 2;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "Rebuked into obedience by another creature.";
	}

	public override string GetDescription()
	{
		return "{{C|rebuked}}";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeforeBeginTakeActionEvent>.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeBeginTakeActionEvent E)
	{
		if (!IsSupported())
		{
			Duration = 0;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor == Rebuker)
		{
			E.AddAction("Dismiss", "dismiss", "DismissServitor", null, 'd', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DismissServitor" && E.Actor == Rebuker && E.Item == Object && Rebuker.CheckCompanionDirection(Object))
		{
			IComponent<GameObject>.XDidYToZ(Rebuker, "dismiss", Object, "from " + Rebuker.its + " service");
			Object.RemoveEffect(this);
			E.Actor.PlayWorldSound("Sounds/Interact/sfx_interact_follower_dismiss");
			E.Actor.CompanionDirectionEnergyCost(E.Item, 100, "Dismiss");
			E.Item.FireEvent(Event.New("DismissedFromService", "Object", E.Item, "Leader", E.Actor, "Effect", this));
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (!GameObject.Validate(ref Rebuker))
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		Object.RemoveEffect<Rebuked>();
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_charm");
		IComponent<GameObject>.XDidYToZ(Rebuker, "rebuke", Object, "into submission", null, null, null, Rebuker);
		if (Rebuker.IsPlayer() && !Rebuker.HasEffect<Dominated>())
		{
			JournalAPI.AddAccomplishment("You rebuked " + Object.an() + " into submission.", HistoricStringExpander.ExpandString("<spice.commonPhrases.onlooker.!random.capitalize>! <spice.commonPhrases.remember.!random.capitalize> the admonishment =name= gave " + Object.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true, WithoutTitles: true) + " when " + Object.it + " presumed to speak the sacred tongue!"), "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Object.GetPrimaryFactionName(VisibleOnly: true, Formatted: true, Base: true) + " by marrying " + Object.an() + ".", null, "general", MuralCategory.Trysts, MuralWeight.Low, null, -1L);
		}
		Object.Heartspray("&C", "&c", "&B");
		Object.SetAlliedLeader<AllyRebuke>(Rebuker);
		Persuasion_RebukeRobot.SyncTarget(Rebuker, Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Rebuker) && Object.PartyLeader == Rebuker && !Rebuker.SupportsFollower(Object, 7))
		{
			Object.Brain.PartyLeader = null;
			Persuasion_RebukeRobot.Neutralize(Rebuker, Object);
			IComponent<GameObject>.XDidY(Object, "wander", "away disinterestedly");
		}
		Object.Brain.RemoveAllegiance<AllyRebuke>(Rebuker?.BaseID ?? 0);
		Rebuker = null;
		base.Remove(Object);
	}

	public bool IsSupported()
	{
		if (GameObject.Validate(ref Rebuker))
		{
			return Rebuker.SupportsFollower(Object, 8);
		}
		return false;
	}
}

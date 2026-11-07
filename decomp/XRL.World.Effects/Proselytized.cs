using System;
using Qud.API;
using XRL.World.AI;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Proselytized : Effect
{
	public GameObject Proselytizer;

	public Proselytized()
	{
		DisplayName = "{{Y|proselytized}}";
		Duration = 1;
	}

	public Proselytized(GameObject Proselytizer)
		: this()
	{
		this.Proselytizer = Proselytizer;
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
		return "Recruited by another creature via proselytization.";
	}

	public override string GetDescription()
	{
		return "{{Y|proselytized}}";
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
		if (E.Actor == Proselytizer)
		{
			E.AddAction("Dismiss", "dismiss", "DismissProselyte", null, 'd', FireOnActor: false, 0, 0, Override: false, WorksAtDistance: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "DismissProselyte" && E.Actor == Proselytizer && E.Item == Object && Proselytizer.CheckCompanionDirection(Object))
		{
			IComponent<GameObject>.XDidYToZ(Proselytizer, "dismiss", Object, "from " + Proselytizer.its + " service");
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
		if (!GameObject.Validate(ref Proselytizer))
		{
			return false;
		}
		if (Object.Brain == null)
		{
			return false;
		}
		if (!Object.FireEvent("ApplyProselytize"))
		{
			return false;
		}
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_charm");
		Object.RemoveEffect<Proselytized>();
		IComponent<GameObject>.XDidYToZ(Proselytizer, "convince", Object, "to join " + Proselytizer.them, "!", null, null, Proselytizer);
		if (Proselytizer.IsPlayer() && !Proselytizer.HasEffect<Dominated>() && !Object.HasIntProperty("TurnsAsPlayerMinion"))
		{
			JournalAPI.AddAccomplishment("You convinced " + Object.an() + " to join your cause.", "Few were possessed of such potent charm as =name=, who -- on the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + " -- bent the will of " + Object.an(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " with mere words.", "<spice.elements." + The.Player.GetMythicDomain() + ".weddingConditions.!random.capitalize>, =name= cemented " + The.Player.GetPronounProvider().PossessiveAdjective + " friendship with " + Object.GetPrimaryFactionName(VisibleOnly: true, Formatted: true, Base: true) + " by marrying " + Object.an() + ".", null, "general", MuralCategory.Treats, MuralWeight.Low, null, -1L);
		}
		Object.Heartspray();
		Object.AddOpinion<OpinionProselytize>(Proselytizer);
		Object.SetAlliedLeader<AllyProselytize>(Proselytizer);
		Persuasion_Proselytize.SyncTarget(Proselytizer, Object);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (GameObject.Validate(ref Proselytizer) && Object.PartyLeader == Proselytizer && !Proselytizer.SupportsFollower(Object, 14))
		{
			Object.Brain.PartyLeader = null;
			Object.Brain.Goals.Clear();
		}
		Object.Brain.RemoveAllegiance<AllyProselytize>(Proselytizer?.BaseID ?? 0);
		Proselytizer = null;
		base.Remove(Object);
	}

	public bool IsSupported()
	{
		if (GameObject.Validate(ref Proselytizer))
		{
			return Proselytizer.SupportsFollower(Object, 1);
		}
		return false;
	}
}

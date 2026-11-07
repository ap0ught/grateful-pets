using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public abstract class IBondedCompanion : IPart
{
	public GameObject CompanionOf;

	public string Faction;

	public string Honorific;

	public string Title;

	public string ConversationID;

	public bool StripGear;

	public IBondedCompanion(GameObject CompanionOf = null, string Faction = null, string Honorific = null, string Title = null, string ConversationID = null, bool StripGear = false)
	{
		this.CompanionOf = CompanionOf;
		this.Faction = Faction;
		this.Honorific = Honorific;
		this.Title = Title;
		this.ConversationID = ConversationID;
		this.StripGear = StripGear;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetDisplayNameEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.WithoutTitles && GameObject.Validate(ref CompanionOf) && E.Understood())
		{
			if (!Honorific.IsNullOrEmpty())
			{
				E.AddHonorific(Honorific);
			}
			if (!Title.IsNullOrEmpty())
			{
				E.AddTitle(Title);
			}
		}
		return base.HandleEvent(E);
	}

	public override void Initialize()
	{
		base.Initialize();
		if (ParentObject.Brain != null)
		{
			if (CompanionOf != null)
			{
				ParentObject.Brain.PartyLeader = CompanionOf;
				ParentObject.Brain.TakeAllegiance(CompanionOf, GetAllyReasonFor(CompanionOf));
			}
			else if (!Faction.IsNullOrEmpty())
			{
				ParentObject.Brain.Factions = Faction + "-100";
				ParentObject.FireEvent("FactionsAdded");
			}
		}
		if (!ConversationID.IsNullOrEmpty())
		{
			ParentObject.RequirePart<ConversationScript>().ConversationID = ConversationID;
		}
		if (StripGear)
		{
			ParentObject.StripOffGear();
		}
		ParentObject.FireEvent("VillageInit");
		InitializeBondedCompanion();
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		base.FinalizeRead(Reader);
		GameObject.Validate(ref CompanionOf);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanAIDoIndependentBehavior");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanAIDoIndependentBehavior" && GameObject.Validate(ref CompanionOf))
		{
			return false;
		}
		return base.FireEvent(E);
	}

	public virtual void InitializeBondedCompanion()
	{
	}

	public virtual IAllyReason GetAllyReasonFor(GameObject Leader)
	{
		return new AllyBond();
	}
}

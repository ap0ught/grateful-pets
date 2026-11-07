using System;

namespace XRL.World.Parts;

[Serializable]
public class ExistenceSupport : IPart
{
	public string SupportedById;

	public bool ValidateEveryTurn;

	public bool SilentRemoval;

	[NonSerialized]
	private GameObject SupportedByCache;

	public GameObject SupportedBy
	{
		get
		{
			if ((SupportedByCache == null || SupportedByCache.IsInvalid()) && !string.IsNullOrEmpty(SupportedById))
			{
				SupportedByCache = GameObject.FindByID(SupportedById);
			}
			return SupportedByCache;
		}
		set
		{
			SupportedById = value?.ID;
			SupportedByCache = value;
		}
	}

	public ExistenceSupport()
	{
	}

	public ExistenceSupport(ExistenceSupport Source)
		: this()
	{
		SupportedById = Source.SupportedById;
		ValidateEveryTurn = Source.ValidateEveryTurn;
		SilentRemoval = Source.SilentRemoval;
	}

	public override void Attach()
	{
		ParentObject.Flags |= 8;
	}

	public override void Remove()
	{
		if (!Temporary.CheckTemporary(ParentObject))
		{
			ParentObject.Flags &= -9;
		}
	}

	public override bool SameAs(IPart Part)
	{
		ExistenceSupport existenceSupport = Part as ExistenceSupport;
		if (existenceSupport.SupportedById != SupportedById)
		{
			return false;
		}
		if (existenceSupport.ValidateEveryTurn != ValidateEveryTurn)
		{
			return false;
		}
		if (existenceSupport.SilentRemoval != SilentRemoval)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != PooledEvent<CanBeTradedEvent>.ID && ID != DerivationCreatedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<ReplaceInContextEvent>.ID && ID != SynchronizeExistenceEvent.ID && ID != WasDerivedFromEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneDeactivatedEvent.ID)
		{
			return ID == ZoneThawedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "SupportedById", SupportedById);
		E.AddEntry(this, "ValidateEveryTurn", ValidateEveryTurn);
		E.AddEntry(this, "SilentRemoval", SilentRemoval);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.ApplyToEach((GameObject obj) =>
		{
			obj.RemovePart<ExistenceSupport>();
		});
		E.ApplyToEach((GameObject obj) =>
		{
			obj.AddPart(new ExistenceSupport(this));
		});
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DerivationCreatedEvent E)
	{
		if (!E.Original.HasPart<ExistenceSupport>())
		{
			E.Object.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(SynchronizeExistenceEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneDeactivatedEvent E)
	{
		CheckSupport();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		E.Replacement.RemovePart<ExistenceSupport>();
		E.Replacement.AddPart(new ExistenceSupport(this));
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return ValidateEveryTurn;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckSupport();
	}

	public void CheckSupport()
	{
		if (!IsSupported())
		{
			Unsupported();
		}
	}

	public bool IsSupported()
	{
		return CheckExistenceSupportEvent.Check(SupportedBy, ParentObject);
	}

	public void Unsupported(bool Silent = false)
	{
		if (SilentRemoval)
		{
			Silent = true;
		}
		if (ParentObject.TryGetPart<Temporary>(out var Part))
		{
			Part.Expire(Silent);
			return;
		}
		ParentObject.RemoveContents(Silent);
		if (!Silent)
		{
			DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		ParentObject.Obliterate(null, Silent);
	}
}

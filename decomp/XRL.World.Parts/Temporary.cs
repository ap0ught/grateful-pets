using System;
using XRL.Collections;

namespace XRL.World.Parts;

[Serializable]
public class Temporary : IPart
{
	public int Duration = 12;

	public string TurnInto;

	public long LastTurn = long.MaxValue;

	public Temporary()
	{
	}

	public Temporary(int Duration)
		: this()
	{
		this.Duration = Duration;
	}

	public Temporary(int Duration, string TurnInto)
		: this(Duration)
	{
		this.TurnInto = TurnInto;
	}

	public Temporary(Temporary src)
		: this(src.Duration, src.TurnInto)
	{
	}

	public override bool SameAs(IPart p)
	{
		Temporary temporary = p as Temporary;
		if (temporary.Duration != Duration)
		{
			return false;
		}
		if (temporary.TurnInto != TurnInto)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AdjustValueEvent.ID && ID != AfterAfterThrownEvent.ID && ID != PooledEvent<CanBeTradedEvent>.ID && ID != PooledEvent<CheckExistenceSupportEvent>.ID && ID != DerivationCreatedEvent.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<ReplaceInContextEvent>.ID && ID != WasDerivedFromEvent.ID && ID != ZoneActivatedEvent.ID && ID != ZoneThawedEvent.ID)
		{
			if (ID == PooledEvent<RealityStabilizeEvent>.ID)
			{
				return TurnInto == "*fugue";
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.ApplyToEach((GameObject obj) =>
		{
			obj.RemovePart<Temporary>();
		});
		E.ApplyToEach((GameObject obj) =>
		{
			obj.AddPart(new Temporary(this));
		});
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DerivationCreatedEvent E)
	{
		if (!E.Original.HasPart<Temporary>())
		{
			E.Object.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterAfterThrownEvent E)
	{
		if (TurnInto == "*fugue" && ParentObject.IsValid())
		{
			Expire();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeTradedEvent E)
	{
		return false;
	}

	public override bool HandleEvent(AdjustValueEvent E)
	{
		E.AdjustValue(0.0);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		Temporary part = E.Object.GetPart<Temporary>();
		if (part != null && part.TurnInto == TurnInto)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RealityStabilizeEvent E)
	{
		if (TurnInto == "*fugue" && E.Check(CanDestroy: true))
		{
			ParentObject.ParticleBlip("&K~", 10, 0L);
			if (Visible())
			{
				if (ParentObject.it == "it")
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("worldline through spacetime") + " snaps back to its canonical path, and " + ParentObject.t() + ParentObject.GetVerb("vanish") + ".");
				}
				else
				{
					IComponent<GameObject>.AddPlayerMessage(ParentObject.Poss("worldline through spacetime") + " snaps back to its canonical path, and " + ParentObject.it + ParentObject.GetVerb("vanish", PrependSpace: true, PronounAntecedent: true) + ".");
				}
			}
			Expire(Silent: true);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		int num = (int)Math.Max(The.Game.Turns - LastTurn, 0L);
		if (Duration > 0 && (Duration -= num) <= 0)
		{
			Expire();
		}
		LastTurn = The.Game.Turns;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneThawedEvent E)
	{
		int num = (int)Math.Max(The.Game.Turns - LastTurn, 0L);
		if (Duration > 0 && (Duration -= num) <= 0)
		{
			Expire();
		}
		LastTurn = The.Game.Turns;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		E.Replacement.RemovePart<Temporary>();
		E.Replacement.AddPart(new Temporary(this));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Duration", Duration);
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Duration > 0 && (Duration -= Amount) <= 0)
		{
			Expire();
		}
		LastTurn = TimeTick;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.ModIntProperty("WontSell", 1, RemoveIfZero: true);
	}

	public override void Attach()
	{
		ParentObject.Flags |= 8;
	}

	public override void Remove()
	{
		ParentObject.ModIntProperty("WontSell", -1, RemoveIfZero: true);
		if (!CheckTemporary(ParentObject))
		{
			ParentObject.Flags &= -9;
		}
	}

	public bool TurnsIntoSomething()
	{
		if (!TurnInto.IsNullOrEmpty())
		{
			return TurnInto != "*fugue";
		}
		return false;
	}

	public void Expire(bool Silent = false)
	{
		Cell cell = null;
		GameObject Object = null;
		if (TurnsIntoSomething())
		{
			cell = ParentObject.GetCurrentCell();
			Object = ParentObject.InInventory ?? ParentObject.Equipped ?? ParentObject.Implantee;
		}
		using ScopeDisposedList<GameObject> scopeDisposedList = ScopeDisposedList<GameObject>.GetFromPool();
		ParentObject.GetContents(scopeDisposedList);
		foreach (GameObject item in scopeDisposedList)
		{
			if (item.TryGetPart<Temporary>(out var Part))
			{
				Part.Expire(Silent: true);
			}
		}
		ParentObject.RemoveContents(Silent);
		if (!Silent && !TurnsIntoSomething())
		{
			DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: true);
		}
		ParentObject.Obliterate("Faded from existence.", Silent: true);
		ParentObject.RemoveFromContext();
		if (TurnsIntoSomething())
		{
			if (cell != null)
			{
				cell.AddObject(TurnInto);
			}
			else if (GameObject.Validate(ref Object))
			{
				Object.Inventory.AddObject(TurnInto);
			}
		}
	}

	public static void AddHierarchically(GameObject Object, int Duration = -1, string TurnInto = null, GameObject DependsOn = null, bool RootObjectValidateEveryTurn = false)
	{
		if (GameObject.Validate(ref Object))
		{
			MakeTemporaryEvent.Send(Object, Duration, TurnInto, DependsOn, RootObjectValidateEveryTurn);
		}
	}

	public static void CarryOver(GameObject src, GameObject dest, bool CanRemove = false)
	{
		if (src.TryGetPart<Temporary>(out var Part))
		{
			dest.RemovePart<Temporary>();
			dest.AddPart(new Temporary(Part));
		}
		else if (CanRemove)
		{
			dest.RemovePart<Temporary>();
		}
		if (src.TryGetPart<ExistenceSupport>(out var Part2))
		{
			dest.RemovePart<ExistenceSupport>();
			dest.AddPart(new ExistenceSupport(Part2));
		}
		else if (CanRemove)
		{
			dest.RemovePart<ExistenceSupport>();
		}
	}

	public static bool IsTemporary(GameObject Object)
	{
		return (Object.Flags & 8) != 0;
	}

	public static bool CheckTemporary(GameObject Object)
	{
		PartRack partsList = Object.PartsList;
		int i = 0;
		for (int count = partsList.Count; i < count; i++)
		{
			Type type = partsList[i].GetType();
			if ((object)type == typeof(Temporary) || (object)type == typeof(ExistenceSupport))
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsNotTemporary(GameObject obj)
	{
		return !IsTemporary(obj);
	}
}

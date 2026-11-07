using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class NeutronFluxPourExplodesEvent : PooledEvent<NeutronFluxPourExplodesEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject PouredFrom;

	public GameObject PouredTo;

	public GameObject PouredBy;

	public LiquidVolume PouredLiquid;

	public bool Prospective;

	public bool Interrupt;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		PouredFrom = null;
		PouredTo = null;
		PouredBy = null;
		PouredLiquid = null;
		Prospective = false;
		Interrupt = false;
	}

	public static bool Check(out bool Interrupt, GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy = null, LiquidVolume PouredLiquid = null, bool Prospective = false)
	{
		Interrupt = false;
		bool flag = true;
		if (flag && GameObject.Validate(ref PouredFrom) && PouredFrom.HasRegisteredEvent("NeutronFluxPourExplodes"))
		{
			Event obj = Event.New("NeutronFluxPourExplodes");
			obj.SetParameter("PouredFrom", PouredFrom);
			obj.SetParameter("PouredTo", PouredTo);
			obj.SetParameter("PouredBy", PouredBy);
			obj.SetParameter("PouredLiquid", PouredLiquid);
			obj.SetFlag("Interrupt", Interrupt);
			obj.SetFlag("Prospective", Prospective);
			flag = PouredFrom.FireEvent(obj);
			Interrupt = obj.HasFlag("Interrupt");
		}
		if (flag && GameObject.Validate(ref PouredTo) && PouredTo.HasRegisteredEvent("NeutronFluxPourExplodes"))
		{
			Event obj2 = Event.New("NeutronFluxPourExplodes");
			obj2.SetParameter("PouredFrom", PouredFrom);
			obj2.SetParameter("PouredTo", PouredTo);
			obj2.SetParameter("PouredBy", PouredBy);
			obj2.SetParameter("PouredLiquid", PouredLiquid);
			obj2.SetFlag("Interrupt", Interrupt);
			obj2.SetFlag("Prospective", Prospective);
			flag = PouredTo.FireEvent(obj2);
			Interrupt = obj2.HasFlag("Interrupt");
		}
		if (flag && GameObject.Validate(ref PouredBy) && PouredBy.HasRegisteredEvent("NeutronFluxPourExplodes"))
		{
			Event obj3 = Event.New("NeutronFluxPourExplodes");
			obj3.SetParameter("PouredFrom", PouredFrom);
			obj3.SetParameter("PouredTo", PouredTo);
			obj3.SetParameter("PouredBy", PouredBy);
			obj3.SetParameter("PouredLiquid", PouredLiquid);
			obj3.SetFlag("Interrupt", Interrupt);
			obj3.SetFlag("Prospective", Prospective);
			flag = PouredBy.FireEvent(obj3);
			Interrupt = obj3.HasFlag("Interrupt");
		}
		NeutronFluxPourExplodesEvent neutronFluxPourExplodesEvent = null;
		if (flag && GameObject.Validate(ref PouredFrom) && PouredFrom.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
		{
			if (neutronFluxPourExplodesEvent == null)
			{
				neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
			}
			neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
			neutronFluxPourExplodesEvent.PouredTo = PouredTo;
			neutronFluxPourExplodesEvent.PouredBy = PouredBy;
			neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
			neutronFluxPourExplodesEvent.Interrupt = Interrupt;
			neutronFluxPourExplodesEvent.Prospective = Prospective;
			flag = PouredFrom.HandleEvent(neutronFluxPourExplodesEvent);
			Interrupt = neutronFluxPourExplodesEvent.Interrupt;
		}
		if (flag && GameObject.Validate(ref PouredTo) && PouredTo.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
		{
			if (neutronFluxPourExplodesEvent == null)
			{
				neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
			}
			neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
			neutronFluxPourExplodesEvent.PouredTo = PouredTo;
			neutronFluxPourExplodesEvent.PouredBy = PouredBy;
			neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
			neutronFluxPourExplodesEvent.Interrupt = Interrupt;
			neutronFluxPourExplodesEvent.Prospective = Prospective;
			flag = PouredTo.HandleEvent(neutronFluxPourExplodesEvent);
			Interrupt = neutronFluxPourExplodesEvent.Interrupt;
		}
		if (flag && GameObject.Validate(ref PouredBy) && PouredBy.WantEvent(PooledEvent<NeutronFluxPourExplodesEvent>.ID, CascadeLevel))
		{
			if (neutronFluxPourExplodesEvent == null)
			{
				neutronFluxPourExplodesEvent = PooledEvent<NeutronFluxPourExplodesEvent>.FromPool();
			}
			neutronFluxPourExplodesEvent.PouredFrom = PouredFrom;
			neutronFluxPourExplodesEvent.PouredTo = PouredTo;
			neutronFluxPourExplodesEvent.PouredBy = PouredBy;
			neutronFluxPourExplodesEvent.PouredLiquid = PouredLiquid;
			neutronFluxPourExplodesEvent.Interrupt = Interrupt;
			neutronFluxPourExplodesEvent.Prospective = Prospective;
			flag = PouredBy.HandleEvent(neutronFluxPourExplodesEvent);
			Interrupt = neutronFluxPourExplodesEvent.Interrupt;
		}
		return flag;
	}

	public static bool Check(GameObject PouredFrom, GameObject PouredTo, GameObject PouredBy = null, LiquidVolume PouredLiquid = null, bool Prospective = false)
	{
		bool Interrupt;
		return Check(out Interrupt, PouredFrom, PouredTo, PouredBy, PouredLiquid, Prospective);
	}
}

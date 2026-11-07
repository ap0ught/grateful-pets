using XRL.World.Tinkering;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class ModifyBitCostEvent : PooledEvent<ModifyBitCostEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public BitCost Bits;

	public string Context;

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
		Actor = null;
		Bits = null;
		Context = null;
	}

	public static ModifyBitCostEvent FromPool(GameObject Actor, BitCost Bits, string Context)
	{
		ModifyBitCostEvent modifyBitCostEvent = PooledEvent<ModifyBitCostEvent>.FromPool();
		modifyBitCostEvent.Actor = Actor;
		modifyBitCostEvent.Bits = Bits;
		modifyBitCostEvent.Context = Context;
		return modifyBitCostEvent;
	}

	public static bool Process(GameObject Actor, BitCost Bits, string Context)
	{
		if (GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("ModifyBitCost"))
		{
			Event obj = Event.New("ModifyBitCost");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Bits", Bits);
			obj.SetParameter("Context", Context);
			if (!Actor.FireEvent(obj))
			{
				return false;
			}
		}
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<ModifyBitCostEvent>.ID, CascadeLevel))
		{
			ModifyBitCostEvent e = FromPool(Actor, Bits, Context);
			if (!Actor.HandleEvent(e))
			{
				return false;
			}
		}
		return true;
	}
}

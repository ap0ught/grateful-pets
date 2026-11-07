using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class LiquidMixedEvent : PooledEvent<LiquidMixedEvent>
{
	public LiquidVolume Liquid;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Liquid = null;
	}

	public static LiquidMixedEvent FromPool(LiquidVolume Liquid)
	{
		LiquidMixedEvent liquidMixedEvent = PooledEvent<LiquidMixedEvent>.FromPool();
		liquidMixedEvent.Liquid = Liquid;
		return liquidMixedEvent;
	}

	public static void Send(LiquidVolume Liquid)
	{
		if (Liquid.ParentObject != null)
		{
			if (Liquid.ParentObject.HasRegisteredEvent("LiquidMixed"))
			{
				Liquid.ParentObject.FireEvent(Event.New("LiquidMixed", "Volume", Liquid));
			}
			if (Liquid.ParentObject.WantEvent(PooledEvent<LiquidMixedEvent>.ID, MinEvent.CascadeLevel))
			{
				Liquid.ParentObject.HandleEvent(FromPool(Liquid));
			}
		}
	}
}

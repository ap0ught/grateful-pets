using XRL.World.Effects;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class RealityStabilizeEvent : PooledEvent<RealityStabilizeEvent>
{
	public new static readonly int CascadeLevel = 15;

	public RealityStabilized Effect;

	public GameObject Object;

	public bool Projecting;

	public bool Relevant;

	public bool CanDestroy;

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
		Effect = null;
		Object = null;
		Projecting = false;
		Relevant = false;
		CanDestroy = false;
	}

	public static RealityStabilizeEvent FromPool(RealityStabilized Effect, GameObject Object, bool Projecting = false)
	{
		RealityStabilizeEvent realityStabilizeEvent = PooledEvent<RealityStabilizeEvent>.FromPool();
		realityStabilizeEvent.Effect = Effect;
		realityStabilizeEvent.Object = Object;
		realityStabilizeEvent.Projecting = Projecting;
		realityStabilizeEvent.Relevant = false;
		realityStabilizeEvent.CanDestroy = false;
		return realityStabilizeEvent;
	}

	public bool Check(bool CanDestroy = false)
	{
		Relevant = true;
		if (CanDestroy)
		{
			this.CanDestroy = true;
			GameObject gameObject = Object;
			if (gameObject != null && gameObject.IsNatural())
			{
				return false;
			}
		}
		return Effect.RandomlyTakeEffect();
	}

	public static void Send(RealityStabilized Effect, GameObject Object, bool Projecting, out bool Relevant, out bool CanDestroy)
	{
		if (Object.WantEvent(PooledEvent<RealityStabilizeEvent>.ID, CascadeLevel))
		{
			RealityStabilizeEvent realityStabilizeEvent = FromPool(Effect, Object, Projecting);
			Object.HandleEvent(realityStabilizeEvent);
			Relevant = realityStabilizeEvent.Relevant;
			CanDestroy = realityStabilizeEvent.CanDestroy;
		}
		else
		{
			Relevant = false;
			CanDestroy = false;
		}
	}
}

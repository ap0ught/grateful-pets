namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class AdjustVisibilityRadiusEvent : PooledEvent<AdjustVisibilityRadiusEvent>
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Object;

	public int Radius;

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
		Object = null;
		Radius = 0;
	}

	public static AdjustVisibilityRadiusEvent FromPool(GameObject Object, int Radius)
	{
		AdjustVisibilityRadiusEvent adjustVisibilityRadiusEvent = PooledEvent<AdjustVisibilityRadiusEvent>.FromPool();
		adjustVisibilityRadiusEvent.Object = Object;
		adjustVisibilityRadiusEvent.Radius = Radius;
		return adjustVisibilityRadiusEvent;
	}

	public static int GetFor(GameObject Actor, int Radius = 80)
	{
		if (Actor.WantEvent(PooledEvent<AdjustVisibilityRadiusEvent>.ID, CascadeLevel))
		{
			AdjustVisibilityRadiusEvent adjustVisibilityRadiusEvent = FromPool(Actor, Radius);
			Actor.HandleEvent(adjustVisibilityRadiusEvent);
			Radius = adjustVisibilityRadiusEvent.Radius;
		}
		return Radius;
	}
}

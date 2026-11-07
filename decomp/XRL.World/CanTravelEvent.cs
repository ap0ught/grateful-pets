namespace XRL.World;

[GameEvent(Cascade = 17)]
public class CanTravelEvent : PooledEvent<CanTravelEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

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
	}

	public static bool Check(GameObject Object)
	{
		if (!GameObject.Validate(Object))
		{
			return false;
		}
		CanTravelEvent E = PooledEvent<CanTravelEvent>.FromPool();
		try
		{
			E.Object = Object;
			if (!Object.HandleEvent(E))
			{
				return false;
			}
			if (!The.Game.HandleEvent(E))
			{
				return false;
			}
		}
		finally
		{
			PooledEvent<CanTravelEvent>.ResetTo(ref E);
		}
		return true;
	}
}

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class CheckExistenceSupportEvent : PooledEvent<CheckExistenceSupportEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject SupportedBy;

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
		SupportedBy = null;
		Object = null;
	}

	public static CheckExistenceSupportEvent FromPool(GameObject SupportedBy, GameObject Object)
	{
		CheckExistenceSupportEvent checkExistenceSupportEvent = PooledEvent<CheckExistenceSupportEvent>.FromPool();
		checkExistenceSupportEvent.SupportedBy = SupportedBy;
		checkExistenceSupportEvent.Object = Object;
		return checkExistenceSupportEvent;
	}

	public static bool Check(GameObject SupportedBy, GameObject Object)
	{
		if (GameObject.Validate(ref SupportedBy) && SupportedBy.WantEvent(PooledEvent<CheckExistenceSupportEvent>.ID, CascadeLevel) && !SupportedBy.HandleEvent(FromPool(SupportedBy, Object)))
		{
			return true;
		}
		return false;
	}
}

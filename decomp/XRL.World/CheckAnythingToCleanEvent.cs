namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class CheckAnythingToCleanEvent : PooledEvent<CheckAnythingToCleanEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject CascadeFrom;

	public GameObject Using;

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
		CascadeFrom = null;
		Using = null;
	}

	public static CheckAnythingToCleanEvent FromPool(GameObject CascadeFrom, GameObject Using = null)
	{
		CheckAnythingToCleanEvent checkAnythingToCleanEvent = PooledEvent<CheckAnythingToCleanEvent>.FromPool();
		checkAnythingToCleanEvent.CascadeFrom = CascadeFrom;
		checkAnythingToCleanEvent.Using = Using;
		return checkAnythingToCleanEvent;
	}

	public static bool Check(GameObject CascadeFrom, GameObject Using = null)
	{
		if (GameObject.Validate(ref CascadeFrom) && CascadeFrom.WantEvent(PooledEvent<CheckAnythingToCleanEvent>.ID, CascadeLevel) && !CascadeFrom.HandleEvent(FromPool(CascadeFrom, Using)))
		{
			return true;
		}
		return false;
	}
}

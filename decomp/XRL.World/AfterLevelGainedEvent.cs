namespace XRL.World;

[GameEvent(Cascade = 17)]
public class AfterLevelGainedEvent : PooledEvent<AfterLevelGainedEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Kill;

	public GameObject InfluencedBy;

	public int Level;

	public bool Detail;

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
		Kill = null;
		InfluencedBy = null;
		Level = 0;
		Detail = false;
	}

	public static void Send(BeforeLevelGainedEvent E)
	{
		AfterLevelGainedEvent afterLevelGainedEvent = PooledEvent<AfterLevelGainedEvent>.FromPool();
		GameObject gameObject = (afterLevelGainedEvent.Actor = E.Actor);
		afterLevelGainedEvent.Kill = E.Kill;
		afterLevelGainedEvent.InfluencedBy = E.InfluencedBy;
		afterLevelGainedEvent.Level = E.Level;
		afterLevelGainedEvent.Detail = E.Detail;
		gameObject.HandleEvent(afterLevelGainedEvent);
		The.Game.HandleEvent(afterLevelGainedEvent);
		gameObject.FireEvent("AfterLevelGained", afterLevelGainedEvent);
		E.Kill = afterLevelGainedEvent.Kill;
		E.InfluencedBy = afterLevelGainedEvent.InfluencedBy;
		E.Detail = afterLevelGainedEvent.Detail;
		E.ProcessChildEvent(afterLevelGainedEvent);
	}
}

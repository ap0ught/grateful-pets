namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AfterPlayerBodyChangeEvent : PooledEvent<AfterPlayerBodyChangeEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject NewBody;

	public GameObject OldBody;

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
		NewBody = null;
		OldBody = null;
	}

	public static void Send(GameObject NewBody, GameObject OldBody)
	{
		if (NewBody == OldBody)
		{
			return;
		}
		bool flag = The.Game.WantEvent(PooledEvent<AfterPlayerBodyChangeEvent>.ID, CascadeLevel);
		bool flag2 = NewBody?.WantEvent(PooledEvent<AfterPlayerBodyChangeEvent>.ID, CascadeLevel) ?? false;
		bool flag3 = OldBody?.WantEvent(PooledEvent<AfterPlayerBodyChangeEvent>.ID, CascadeLevel) ?? false;
		if (flag | flag2 | flag3)
		{
			AfterPlayerBodyChangeEvent afterPlayerBodyChangeEvent = PooledEvent<AfterPlayerBodyChangeEvent>.FromPool();
			afterPlayerBodyChangeEvent.NewBody = NewBody;
			afterPlayerBodyChangeEvent.OldBody = OldBody;
			if (flag)
			{
				The.Game.HandleEvent(afterPlayerBodyChangeEvent);
			}
			if (flag2)
			{
				NewBody.HandleEvent(afterPlayerBodyChangeEvent);
			}
			if (flag3)
			{
				OldBody.HandleEvent(afterPlayerBodyChangeEvent);
			}
		}
	}
}

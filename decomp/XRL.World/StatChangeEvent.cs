namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class StatChangeEvent : PooledEvent<StatChangeEvent>
{
	public GameObject Object;

	public string Name;

	public string Type;

	public int OldValue;

	public int NewValue;

	public int OldBaseValue;

	public int NewBaseValue;

	public Statistic Stat;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Name = null;
		Type = null;
		OldValue = 0;
		NewValue = 0;
		OldBaseValue = 0;
		NewBaseValue = 0;
		Stat = null;
	}

	public static StatChangeEvent FromPool(GameObject Object, string Name = null, string Type = null, int OldValue = 0, int NewValue = 0, int OldBaseValue = 0, int NewBaseValue = 0, Statistic Stat = null)
	{
		StatChangeEvent statChangeEvent = PooledEvent<StatChangeEvent>.FromPool();
		statChangeEvent.Object = Object;
		statChangeEvent.Name = Name;
		statChangeEvent.Type = Type;
		statChangeEvent.OldValue = OldValue;
		statChangeEvent.NewValue = NewValue;
		statChangeEvent.OldBaseValue = OldBaseValue;
		statChangeEvent.NewBaseValue = NewBaseValue;
		statChangeEvent.Stat = Stat;
		return statChangeEvent;
	}
}

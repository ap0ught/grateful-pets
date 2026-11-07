namespace XRL.World;

public abstract class SingletonEvent<T> : CachedEvent where T : MinEvent, new()
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(T));

	public static readonly T Instance = new T();

	public SingletonEvent()
	{
		base.ID = ID;
	}
}

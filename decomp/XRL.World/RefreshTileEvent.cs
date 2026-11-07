namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class RefreshTileEvent : SingletonEvent<RefreshTileEvent>
{
	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public static void Send(GameObject obj)
	{
		obj.HandleEvent(SingletonEvent<RefreshTileEvent>.Instance);
	}
}

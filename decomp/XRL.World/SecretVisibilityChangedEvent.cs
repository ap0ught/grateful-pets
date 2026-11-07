using Qud.API;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class SecretVisibilityChangedEvent : PooledEvent<SecretVisibilityChangedEvent>
{
	public IBaseJournalEntry Entry;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Entry = null;
	}

	public static void Send(IBaseJournalEntry Entry)
	{
		SecretVisibilityChangedEvent E = PooledEvent<SecretVisibilityChangedEvent>.FromPool();
		E.Entry = Entry;
		The.Game.HandleEvent(E, DispatchPlayer: true);
		PooledEvent<SecretVisibilityChangedEvent>.ResetTo(ref E);
	}
}

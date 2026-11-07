using XRL.CharacterBuilds;

namespace XRL.World;

[GameEvent(Cache = Cache.Singleton)]
public class EmbarkEvent : SingletonEvent<EmbarkEvent>
{
	public EmbarkInfo Info;

	public string EventID;

	public object Element;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Info = null;
		EventID = null;
		Element = null;
	}

	public static void Send(XRLGame Game, EmbarkInfo Info, string EventID)
	{
		SingletonEvent<EmbarkEvent>.Instance.Info = Info;
		SingletonEvent<EmbarkEvent>.Instance.EventID = EventID;
		Game?.HandleEvent(SingletonEvent<EmbarkEvent>.Instance, DispatchPlayer: true);
		foreach (IGameStateSingleton gameStateSingleton in Info.GameStateSingletons)
		{
			gameStateSingleton.HandleEvent(SingletonEvent<EmbarkEvent>.Instance);
		}
		SingletonEvent<EmbarkEvent>.Instance.Reset();
	}

	public static void Send(XRLGame Game, EmbarkInfo Info, string EventID, ref object Element)
	{
		SingletonEvent<EmbarkEvent>.Instance.Info = Info;
		SingletonEvent<EmbarkEvent>.Instance.EventID = EventID;
		SingletonEvent<EmbarkEvent>.Instance.Element = Element;
		Game?.HandleEvent(SingletonEvent<EmbarkEvent>.Instance, DispatchPlayer: true);
		foreach (IGameStateSingleton gameStateSingleton in Info.GameStateSingletons)
		{
			gameStateSingleton.HandleEvent(SingletonEvent<EmbarkEvent>.Instance);
		}
		Element = SingletonEvent<EmbarkEvent>.Instance.Element;
		SingletonEvent<EmbarkEvent>.Instance.Reset();
	}
}

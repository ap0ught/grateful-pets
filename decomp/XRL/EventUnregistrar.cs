using XRL.World;

namespace XRL;

public class EventUnregistrar : IEventRegistrar
{
	public static EventUnregistrar Instance = new EventUnregistrar();

	public IEventSource Source;

	public IEventHandler Handler;

	public bool IsUnregister => true;

	public static EventUnregistrar Get(IEventSource Source = null, IEventHandler Handler = null)
	{
		Instance.Source = Source;
		Instance.Handler = Handler;
		return Instance;
	}

	public void Register(IEventSource Source, IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false)
	{
		Source.UnregisterEvent(Handler, EventID);
	}

	public void Register(IEventSource Source, int EventID, int Order = 0, bool Serialize = false)
	{
		Source.UnregisterEvent(Handler, EventID);
	}

	public void Register(int EventID, int Order = 0, bool Serialize = false)
	{
		Source.UnregisterEvent(Handler, EventID);
	}

	public void Register(string EventID)
	{
		if (Source is GameObject gameObject)
		{
			if (Handler is IPart part)
			{
				gameObject.UnregisterPartEvent(part, EventID);
			}
			else if (Handler is Effect ef)
			{
				gameObject.UnregisterEffectEvent(ef, EventID);
			}
		}
	}
}

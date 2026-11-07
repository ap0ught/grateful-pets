using System.Runtime.CompilerServices;
using XRL.World;

namespace XRL;

public class EventRegistrar : IEventRegistrar
{
	public static EventRegistrar Instance = new EventRegistrar();

	public IEventSource Source;

	public IEventHandler Handler;

	public bool IsUnregister => false;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static EventRegistrar Get(IEventSource Source = null, IEventHandler Handler = null)
	{
		Instance.Source = Source;
		Instance.Handler = Handler;
		return Instance;
	}

	public void Register(IEventSource Source, IEventHandler Handler, int EventID, int Order = 0, bool Serialize = false)
	{
		Source.RegisterEvent(Handler, EventID, Order, Serialize);
	}

	public void Register(IEventSource Source, int EventID, int Order = 0, bool Serialize = false)
	{
		Source.RegisterEvent(Handler, EventID, Order, Serialize);
	}

	public void Register(int EventID, int Order = 0, bool Serialize = false)
	{
		Source.RegisterEvent(Handler, EventID, Order, Serialize);
	}

	public void Register(string EventID)
	{
		if (Source is GameObject gameObject)
		{
			if (Handler is IPart ef)
			{
				gameObject.RegisterPartEvent(ef, EventID);
			}
			else if (Handler is Effect effect)
			{
				gameObject.RegisterEffectEvent(effect, EventID);
			}
			else
			{
				MetricsManager.LogAssemblyWarning(Handler.GetType(), "Cannot register for string events with '" + Handler.GetType().GetName(Full: true) + "', only IPart and Effect handlers are supported.");
			}
		}
		else
		{
			MetricsManager.LogAssemblyWarning(Handler.GetType(), "Cannot register for string events on '" + Source.GetType().GetName(Full: true) + "', only GameObject sources are supported.");
		}
	}
}

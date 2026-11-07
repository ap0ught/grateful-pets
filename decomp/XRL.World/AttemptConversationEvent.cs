using XRL.World.Conversations;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Singleton)]
public class AttemptConversationEvent : SingletonEvent<AttemptConversationEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Listener;

	public GameObject Speaker;

	public GameObject Transmitter;

	public GameObject Receiver;

	public ConversationXMLBlueprint Blueprint;

	public bool Silent;

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
		Listener = null;
		Speaker = null;
		Transmitter = null;
		Receiver = null;
		Blueprint = null;
		Silent = false;
	}

	public bool IsParticipant(GameObject Object)
	{
		if (Object != null)
		{
			if (Object != Listener)
			{
				return Object == Speaker;
			}
			return true;
		}
		return false;
	}

	public bool IsDevice(GameObject Object)
	{
		if (Object != null)
		{
			if (Object != Transmitter)
			{
				return Object == Receiver;
			}
			return true;
		}
		return false;
	}

	public static bool Check(ref GameObject Speaker, ref GameObject Listener, ref GameObject Transmitter, ref GameObject Receiver, ref ConversationXMLBlueprint Blueprint, bool Silent)
	{
		SingletonEvent<AttemptConversationEvent>.Instance.Listener = Listener;
		SingletonEvent<AttemptConversationEvent>.Instance.Speaker = Speaker;
		SingletonEvent<AttemptConversationEvent>.Instance.Transmitter = Transmitter;
		SingletonEvent<AttemptConversationEvent>.Instance.Receiver = Receiver;
		SingletonEvent<AttemptConversationEvent>.Instance.Blueprint = Blueprint;
		SingletonEvent<AttemptConversationEvent>.Instance.Silent = Silent;
		int result;
		if (Listener.HandleEvent(SingletonEvent<AttemptConversationEvent>.Instance) && Speaker.HandleEvent(SingletonEvent<AttemptConversationEvent>.Instance))
		{
			GameObject obj = Transmitter;
			if (obj == null || obj.HandleEvent(SingletonEvent<AttemptConversationEvent>.Instance))
			{
				result = ((Receiver?.HandleEvent(SingletonEvent<AttemptConversationEvent>.Instance) ?? true) ? 1 : 0);
				goto IL_0090;
			}
		}
		result = 0;
		goto IL_0090;
		IL_0090:
		Listener = SingletonEvent<AttemptConversationEvent>.Instance.Listener;
		Speaker = SingletonEvent<AttemptConversationEvent>.Instance.Speaker;
		Transmitter = SingletonEvent<AttemptConversationEvent>.Instance.Transmitter;
		Receiver = SingletonEvent<AttemptConversationEvent>.Instance.Receiver;
		Blueprint = SingletonEvent<AttemptConversationEvent>.Instance.Blueprint;
		SingletonEvent<AttemptConversationEvent>.Instance.Reset();
		return (byte)result != 0;
	}
}

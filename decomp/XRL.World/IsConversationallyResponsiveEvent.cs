namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class IsConversationallyResponsiveEvent : PooledEvent<IsConversationallyResponsiveEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Speaker;

	public GameObject Actor;

	public bool Physical;

	public bool Mental;

	public string Message;

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
		Speaker = null;
		Actor = null;
		Physical = false;
		Mental = false;
		Message = null;
	}

	public static bool Check(GameObject Speaker, GameObject Actor, out string Message, bool Physical = false, bool Mental = false)
	{
		Message = null;
		bool flag = true;
		if (flag && GameObject.Validate(ref Speaker) && Speaker.HasRegisteredEvent("IsConversationallyResponsive"))
		{
			Event obj = Event.New("IsConversationallyResponsive");
			obj.SetParameter("Speaker", Speaker);
			obj.SetParameter("Actor", Actor);
			obj.SetFlag("Physical", Physical);
			obj.SetFlag("Mental", Mental);
			obj.SetParameter("Message", Message);
			flag = Speaker.FireEvent(obj);
			Message = obj.GetStringParameter("Message");
		}
		if (flag && GameObject.Validate(ref Speaker) && Speaker.WantEvent(PooledEvent<IsConversationallyResponsiveEvent>.ID, CascadeLevel))
		{
			IsConversationallyResponsiveEvent isConversationallyResponsiveEvent = PooledEvent<IsConversationallyResponsiveEvent>.FromPool();
			isConversationallyResponsiveEvent.Speaker = Speaker;
			isConversationallyResponsiveEvent.Actor = Actor;
			isConversationallyResponsiveEvent.Physical = Physical;
			isConversationallyResponsiveEvent.Mental = Mental;
			isConversationallyResponsiveEvent.Message = Message;
			flag = Speaker.HandleEvent(isConversationallyResponsiveEvent);
			Message = isConversationallyResponsiveEvent.Message;
		}
		return flag;
	}

	public static bool Check(GameObject Speaker, GameObject Actor, bool Physical = false, bool Mental = false)
	{
		string Message;
		return Check(Speaker, Actor, out Message, Physical, Mental);
	}
}

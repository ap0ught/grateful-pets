using System.Text;
using XRL.UI;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class InduceVomitingEvent : PooledEvent<InduceVomitingEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public StringBuilder MessageHolder;

	public bool Vomited;

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
		Object = null;
		MessageHolder = null;
		Vomited = false;
	}

	public void Message(string Text)
	{
		if (MessageHolder != null)
		{
			MessageHolder.Compound(Text);
		}
		else if (GameObject.Validate(ref Object) && Object.IsPlayer())
		{
			Popup.Show(Text);
		}
	}

	public static bool Send(GameObject Object, ref bool InterfaceExitRequested, StringBuilder MessageHolder = null)
	{
		bool flag = true;
		bool flag2 = false;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("InduceVomiting"))
		{
			Event obj = Event.New("InduceVomiting");
			obj.SetParameter("Object", Object);
			obj.SetParameter("MessageHolder", MessageHolder);
			obj.SetFlag("Vomited", flag2);
			obj.SetFlag("InterfaceExitRequested", InterfaceExitRequested);
			flag = Object.FireEvent(obj);
			flag2 = obj.HasFlag("Vomited");
			InterfaceExitRequested = obj.HasFlag("InterfaceExitRequested");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<InduceVomitingEvent>.ID, CascadeLevel))
		{
			InduceVomitingEvent induceVomitingEvent = PooledEvent<InduceVomitingEvent>.FromPool();
			induceVomitingEvent.Object = Object;
			induceVomitingEvent.MessageHolder = MessageHolder;
			induceVomitingEvent.Vomited = flag2;
			induceVomitingEvent.InterfaceExit = InterfaceExitRequested;
			flag = Object.HandleEvent(induceVomitingEvent);
			flag2 = induceVomitingEvent.Vomited;
			InterfaceExitRequested = induceVomitingEvent.InterfaceExit;
		}
		return flag2;
	}

	public static bool Send(GameObject Object, StringBuilder MessageHolder = null, IEvent FromEvent = null)
	{
		bool InterfaceExitRequested = FromEvent?.InterfaceExitRequested() ?? false;
		bool result = Send(Object, ref InterfaceExitRequested, MessageHolder);
		if (InterfaceExitRequested)
		{
			FromEvent.RequestInterfaceExit();
		}
		return result;
	}
}

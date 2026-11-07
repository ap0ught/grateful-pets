using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeginBeingUnequippedEvent : PooledEvent<BeginBeingUnequippedEvent>
{
	public GameObject Object;

	public GameObject Equipper;

	public GameObject Actor;

	public BodyPart BodyPart;

	public bool Silent;

	public bool Forced;

	public bool SemiForced;

	public bool DestroyOnUnequipDeclined;

	public int AutoEquipTry;

	public string FailureMessage;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Equipper = null;
		Actor = null;
		BodyPart = null;
		Silent = false;
		Forced = false;
		SemiForced = false;
		DestroyOnUnequipDeclined = false;
		AutoEquipTry = 0;
		FailureMessage = null;
	}

	public static bool Check(GameObject Object, ref string FailureMessage, ref bool DestroyOnUnequipDeclined, GameObject Equipper = null, GameObject Actor = null, BodyPart BodyPart = null, bool Silent = false, bool Forced = false, bool SemiForced = false, int AutoEquipTry = 0)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("BeginBeingUnequipped"))
		{
			Event obj = Event.New("BeginBeingUnequipped");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Equipper", Equipper);
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("BodyPart", BodyPart);
			obj.SetParameter("AutoEquipTry", AutoEquipTry);
			obj.SetParameter("FailureMessage", FailureMessage);
			obj.SetFlag("Silent", Silent);
			obj.SetFlag("Forced", Forced);
			obj.SetFlag("SemiForced", SemiForced);
			obj.SetFlag("DestroyOnUnequipDeclined", DestroyOnUnequipDeclined);
			flag = Object.FireEvent(obj);
			FailureMessage = obj.GetStringParameter("FailureMessage");
			DestroyOnUnequipDeclined = obj.HasFlag("DestroyOnUnequipDeclined");
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<BeginBeingUnequippedEvent>.ID, MinEvent.CascadeLevel))
		{
			BeginBeingUnequippedEvent beginBeingUnequippedEvent = PooledEvent<BeginBeingUnequippedEvent>.FromPool();
			beginBeingUnequippedEvent.Object = Object;
			beginBeingUnequippedEvent.Equipper = Equipper;
			beginBeingUnequippedEvent.Actor = Actor;
			beginBeingUnequippedEvent.BodyPart = BodyPart;
			beginBeingUnequippedEvent.AutoEquipTry = AutoEquipTry;
			beginBeingUnequippedEvent.FailureMessage = FailureMessage;
			beginBeingUnequippedEvent.Silent = Silent;
			beginBeingUnequippedEvent.Forced = Forced;
			beginBeingUnequippedEvent.SemiForced = SemiForced;
			beginBeingUnequippedEvent.DestroyOnUnequipDeclined = DestroyOnUnequipDeclined;
			flag = Object.HandleEvent(beginBeingUnequippedEvent);
			FailureMessage = beginBeingUnequippedEvent.FailureMessage;
			DestroyOnUnequipDeclined = beginBeingUnequippedEvent.DestroyOnUnequipDeclined;
		}
		return flag;
	}

	public void AddFailureMessage(string Message)
	{
		if (FailureMessage.IsNullOrEmpty())
		{
			FailureMessage = Message;
		}
		else if (!FailureMessage.Contains(Message))
		{
			FailureMessage = FailureMessage + " " + Message;
		}
	}
}

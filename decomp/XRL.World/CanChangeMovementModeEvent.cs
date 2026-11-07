namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class CanChangeMovementModeEvent : PooledEvent<CanChangeMovementModeEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public string To;

	public bool Involuntary;

	public bool ShowMessage;

	public bool AllowTelekinetic;

	public bool FrozenOkay;

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
		To = null;
		Involuntary = false;
		ShowMessage = false;
		AllowTelekinetic = false;
		FrozenOkay = false;
	}

	public static bool Check(GameObject Object, string To = null, bool Involuntary = false, bool ShowMessage = false, bool AllowTelekinetic = false, bool FrozenOkay = false)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("CanChangeMovementMode"))
		{
			Event obj = Event.New("CanChangeMovementMode");
			obj.SetParameter("Object", Object);
			obj.SetParameter("To", To);
			obj.SetFlag("Involuntary", Involuntary);
			obj.SetFlag("ShowMessage", ShowMessage);
			obj.SetFlag("AllowTelekinetic", AllowTelekinetic);
			obj.SetFlag("FrozenOkay", FrozenOkay);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<CanChangeMovementModeEvent>.ID, CascadeLevel))
		{
			CanChangeMovementModeEvent canChangeMovementModeEvent = PooledEvent<CanChangeMovementModeEvent>.FromPool();
			canChangeMovementModeEvent.Object = Object;
			canChangeMovementModeEvent.To = To;
			canChangeMovementModeEvent.Involuntary = Involuntary;
			canChangeMovementModeEvent.ShowMessage = ShowMessage;
			canChangeMovementModeEvent.AllowTelekinetic = AllowTelekinetic;
			canChangeMovementModeEvent.FrozenOkay = FrozenOkay;
			flag = Object.HandleEvent(canChangeMovementModeEvent);
		}
		return flag;
	}
}

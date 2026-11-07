namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IDestroyObjectEvent : MinEvent
{
	public GameObject Object;

	public bool Obliterate;

	public bool Silent;

	public string Reason;

	public string ThirdPersonReason;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Obliterate = false;
		Silent = false;
		Reason = null;
		ThirdPersonReason = null;
	}
}

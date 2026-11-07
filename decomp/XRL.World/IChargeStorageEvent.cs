namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IChargeStorageEvent : IChargeEvent
{
	public int Transient;

	public bool UnlimitedTransient;

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Transient = 0;
		UnlimitedTransient = false;
	}

	public override Event GenerateRegisteredEvent(string ID)
	{
		Event obj = base.GenerateRegisteredEvent(ID);
		obj.SetParameter("Transient", Transient);
		obj.SetFlag("UnlimitedTransient", UnlimitedTransient);
		return obj;
	}

	public override void SyncFromRegisteredEvent(Event E, bool AllFields = false)
	{
		base.SyncFromRegisteredEvent(E, AllFields);
		Transient = E.GetIntParameter("Transient");
		UnlimitedTransient = E.HasFlag("UnlimitedTransient");
	}
}

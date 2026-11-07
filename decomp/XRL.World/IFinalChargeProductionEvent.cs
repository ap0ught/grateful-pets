namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IFinalChargeProductionEvent : IChargeProductionEvent
{
	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}
}

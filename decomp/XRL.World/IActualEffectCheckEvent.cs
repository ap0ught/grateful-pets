namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IActualEffectCheckEvent : IEffectCheckEvent
{
	public Effect Effect;

	public GameObject Actor;

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
		Effect = null;
		Actor = null;
	}
}

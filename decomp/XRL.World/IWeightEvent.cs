namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IWeightEvent : MinEvent
{
	public GameObject Object;

	public double BaseWeight;

	public double Weight;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		BaseWeight = 0.0;
		Weight = 0.0;
	}
}

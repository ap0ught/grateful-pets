namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Singleton)]
public class ProducesLiquidEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ProducesLiquidEvent));

	public new static readonly int CascadeLevel = 0;

	public static readonly ProducesLiquidEvent Instance = new ProducesLiquidEvent();

	public GameObject Object;

	public ProducesLiquidEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

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
		Object = null;
	}

	public static bool Check(GameObject Object, string Liquid)
	{
		if (!Object.Understood())
		{
			return false;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			Instance.Object = Object;
			Instance.Liquid = Liquid;
			if (!Object.HandleEvent(Instance))
			{
				return true;
			}
		}
		return false;
	}
}

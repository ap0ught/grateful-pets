namespace XRL.World;

[GameEvent(Cascade = 1, Cache = Cache.Pool)]
public class GetBleedLiquidEvent : PooledEvent<GetBleedLiquidEvent>
{
	public new static readonly int CascadeLevel = 1;

	public GameObject Actor;

	public string BaseLiquid;

	public string Liquid;

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
		Actor = null;
		BaseLiquid = null;
		Liquid = null;
	}

	public static string GetFor(GameObject Actor, string Default = "blood-1000")
	{
		GetBleedLiquidEvent E = PooledEvent<GetBleedLiquidEvent>.FromPool();
		E.Actor = Actor;
		E.BaseLiquid = (E.Liquid = Actor.GetPropertyOrTag("BleedLiquid", Default));
		Actor.HandleEvent(E);
		string liquid = E.Liquid;
		PooledEvent<GetBleedLiquidEvent>.ResetTo(ref E);
		return liquid;
	}
}

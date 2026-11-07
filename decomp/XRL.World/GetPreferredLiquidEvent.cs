using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 0, Cache = Cache.Pool)]
public class GetPreferredLiquidEvent : ILiquidEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetPreferredLiquidEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 0;

	private static List<GetPreferredLiquidEvent> Pool;

	private static int PoolCounter;

	public GameObject Object;

	public GetPreferredLiquidEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref GetPreferredLiquidEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetPreferredLiquidEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
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

	public static string GetFor(GameObject Object, GameObject Actor = null)
	{
		if (!Object.Understood())
		{
			return null;
		}
		if (Object.WantEvent(ID, CascadeLevel))
		{
			GetPreferredLiquidEvent getPreferredLiquidEvent = FromPool();
			getPreferredLiquidEvent.Object = Object;
			getPreferredLiquidEvent.Actor = Actor;
			getPreferredLiquidEvent.Liquid = null;
			getPreferredLiquidEvent.LiquidVolume = null;
			getPreferredLiquidEvent.Drams = 0;
			getPreferredLiquidEvent.Skip = null;
			getPreferredLiquidEvent.SkipList = null;
			getPreferredLiquidEvent.Filter = null;
			getPreferredLiquidEvent.Auto = false;
			getPreferredLiquidEvent.ImpureOkay = false;
			getPreferredLiquidEvent.SafeOnly = false;
			if (Object.HandleEvent(getPreferredLiquidEvent))
			{
				return getPreferredLiquidEvent.Liquid;
			}
		}
		return null;
	}
}

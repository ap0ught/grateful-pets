using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class BeforeConsumeEvent : IConsumeEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeConsumeEvent), null, CountPool, ResetPool);

	private static List<BeforeConsumeEvent> Pool;

	private static int PoolCounter;

	public BeforeConsumeEvent()
	{
		base.ID = ID;
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

	public static void ResetTo(ref BeforeConsumeEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static BeforeConsumeEvent FromPool()
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

	public static bool Check(GameObject Actor, GameObject Subject, GameObject Object, bool Eat = false, bool Drink = false, bool Inject = false, bool Inhale = false, bool Absorb = false, bool Voluntary = true)
	{
		BeforeConsumeEvent E = FromPool();
		E.Actor = Actor;
		E.Subject = Subject;
		E.Object = Object;
		E.Eat = Eat;
		E.Drink = Drink;
		E.Inject = Inject;
		E.Inhale = Inhale;
		E.Absorb = Absorb;
		E.Voluntary = Voluntary;
		bool result = IConsumeEvent.DispatchAll(E);
		ResetTo(ref E);
		return result;
	}
}

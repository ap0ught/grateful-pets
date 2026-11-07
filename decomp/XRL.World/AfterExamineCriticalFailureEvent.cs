using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AfterExamineCriticalFailureEvent : IExamineEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterExamineCriticalFailureEvent), null, CountPool, ResetPool);

	private static List<AfterExamineCriticalFailureEvent> Pool;

	private static int PoolCounter;

	public AfterExamineCriticalFailureEvent()
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

	public static void ResetTo(ref AfterExamineCriticalFailureEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterExamineCriticalFailureEvent FromPool()
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

	public static void Send(GameObject Actor, GameObject Item)
	{
		bool flag = true;
		AfterExamineCriticalFailureEvent afterExamineCriticalFailureEvent = null;
		if (flag && (Actor.HasRegisteredEvent("AfterExamineCriticalFailure") || Item.HasRegisteredEvent("AfterExamineCriticalFailure")))
		{
			if (afterExamineCriticalFailureEvent == null)
			{
				afterExamineCriticalFailureEvent = FromPool();
				afterExamineCriticalFailureEvent.Actor = Actor;
				afterExamineCriticalFailureEvent.Item = Item;
				afterExamineCriticalFailureEvent.Setup();
			}
			Event obj = Event.New("AfterExamineCriticalFailure");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Item", Item);
			obj.SetFlag("Identify", afterExamineCriticalFailureEvent.Identify);
			obj.SetFlag("IdentifyIfDestroyed", afterExamineCriticalFailureEvent.IdentifyIfDestroyed);
			flag = Actor.FireEvent(obj) && Item.FireEvent(obj);
			afterExamineCriticalFailureEvent.Identify = obj.HasFlag("Identify");
			afterExamineCriticalFailureEvent.IdentifyIfDestroyed = obj.HasFlag("IdentifyIfDestroyed");
		}
		if (flag)
		{
			bool flag2 = Actor.WantEvent(ID, MinEvent.CascadeLevel);
			bool flag3 = Item.WantEvent(ID, MinEvent.CascadeLevel);
			if (flag2 | flag3)
			{
				if (afterExamineCriticalFailureEvent == null)
				{
					afterExamineCriticalFailureEvent = FromPool();
					afterExamineCriticalFailureEvent.Actor = Actor;
					afterExamineCriticalFailureEvent.Item = Item;
					afterExamineCriticalFailureEvent.Setup();
				}
				flag = (!flag2 || Actor.HandleEvent(afterExamineCriticalFailureEvent)) && (!flag3 || Item.HandleEvent(afterExamineCriticalFailureEvent));
			}
		}
		afterExamineCriticalFailureEvent?.ProcessIdentify();
	}
}

using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class AnimateEvent : PooledEvent<AnimateEvent>
{
	public GameObject Actor;

	public GameObject Object;

	public GameObject Using;

	public List<IPart> PartsToRemove;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		Using = null;
		PartsToRemove = null;
	}

	public void WantToRemove(IPart Part)
	{
		if (PartsToRemove != null)
		{
			if (!PartsToRemove.Contains(Part))
			{
				PartsToRemove.Add(Part);
			}
		}
		else
		{
			Object.RemovePart(Part);
		}
	}

	public static void Send(GameObject Actor, GameObject Object, GameObject Using)
	{
		bool flag = true;
		List<IPart> list = null;
		try
		{
			if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("Animate"))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				Event obj = Event.New("Animate");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Object", Object);
				obj.SetParameter("Using", Using);
				obj.SetParameter("PartsToRemove", list);
				flag = Object.FireEvent(obj);
			}
			if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<AnimateEvent>.ID, MinEvent.CascadeLevel))
			{
				if (list == null)
				{
					list = new List<IPart>();
				}
				AnimateEvent animateEvent = PooledEvent<AnimateEvent>.FromPool();
				animateEvent.Actor = Actor;
				animateEvent.Object = Object;
				animateEvent.Using = Using;
				animateEvent.PartsToRemove = list;
				flag = Object.HandleEvent(animateEvent);
			}
		}
		finally
		{
			if (list != null)
			{
				foreach (IPart item in list)
				{
					Object.RemovePart(item);
				}
			}
		}
	}
}

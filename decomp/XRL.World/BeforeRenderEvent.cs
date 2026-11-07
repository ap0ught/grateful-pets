using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Singleton, Cascade = 273)]
public class BeforeRenderEvent : MinEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(BeforeRenderEvent));

	public new static readonly int CascadeLevel = 273;

	public static readonly BeforeRenderEvent Instance = new BeforeRenderEvent();

	public int Pass = 1;

	public List<IEventHandler> AfterHandlers = new List<IEventHandler>();

	public BeforeRenderEvent()
	{
		base.ID = ID;
	}

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
		Pass = 1;
		AfterHandlers.Clear();
	}

	public static bool Send(Zone Z)
	{
		BeforeRenderEvent instance = Instance;
		try
		{
			Z.HandleEvent(instance);
			if (instance.AfterHandlers.Count > 0)
			{
				instance.Pass = 2;
				foreach (IEventHandler afterHandler in instance.AfterHandlers)
				{
					afterHandler.HandleEvent(instance);
				}
			}
			TutorialManager.BeforeRenderEvent();
		}
		finally
		{
			instance.Reset();
		}
		return true;
	}
}

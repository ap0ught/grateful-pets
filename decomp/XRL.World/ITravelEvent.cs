using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 17, Base = true)]
public abstract class ITravelEvent : MinEvent
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public string TravelClass;

	public int PercentageBonus;

	public Dictionary<string, int> ApplicationTracking;

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
		TravelClass = null;
		PercentageBonus = 0;
		ApplicationTracking = null;
	}

	public int GetApplied(string Application)
	{
		if (ApplicationTracking != null && !string.IsNullOrEmpty(Application) && ApplicationTracking.TryGetValue(Application, out var value))
		{
			return value;
		}
		return 0;
	}

	public void SetApplied(string Application, int Value)
	{
		if (!string.IsNullOrEmpty(Application))
		{
			if (ApplicationTracking == null)
			{
				ApplicationTracking = new Dictionary<string, int>();
			}
			ApplicationTracking[Application] = Value;
		}
	}
}

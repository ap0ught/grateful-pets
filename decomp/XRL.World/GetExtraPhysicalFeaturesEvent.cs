using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class GetExtraPhysicalFeaturesEvent : PooledEvent<GetExtraPhysicalFeaturesEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Object;

	public List<string> Features;

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
		Object = null;
		Features = null;
	}

	public static GetExtraPhysicalFeaturesEvent FromPool(GameObject Object, List<string> Features)
	{
		GetExtraPhysicalFeaturesEvent getExtraPhysicalFeaturesEvent = PooledEvent<GetExtraPhysicalFeaturesEvent>.FromPool();
		getExtraPhysicalFeaturesEvent.Object = Object;
		getExtraPhysicalFeaturesEvent.Features = Features;
		return getExtraPhysicalFeaturesEvent;
	}

	public static void Send(GameObject Object, List<string> Features)
	{
		bool flag = true;
		if (flag && GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetExtraPhysicalFeatures"))
		{
			Event obj = Event.New("GetExtraPhysicalFeatures");
			obj.SetParameter("Object", Object);
			obj.SetParameter("Features", Features);
			flag = Object.FireEvent(obj);
		}
		if (flag && GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetExtraPhysicalFeaturesEvent>.ID, CascadeLevel))
		{
			flag = Object.HandleEvent(FromPool(Object, Features));
		}
	}
}

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetRunningBehaviorEvent : PooledEvent<GetRunningBehaviorEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public string AbilityName;

	public string Verb;

	public string EffectDisplayName;

	public string EffectMessageName;

	public int EffectDuration;

	public bool SpringingEffective;

	public Templates.StatCollector Stats;

	public int Priority;

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
		AbilityName = null;
		Verb = null;
		EffectDisplayName = null;
		EffectMessageName = null;
		EffectDuration = 0;
		SpringingEffective = false;
		Stats = null;
		Priority = 0;
	}

	public static void Retrieve(GameObject Actor, out string AbilityName, out string Verb, out string EffectDisplayName, out string EffectMessageName, out int EffectDuration, out bool SpringingEffective, Templates.StatCollector Stats = null)
	{
		AbilityName = null;
		Verb = null;
		EffectDisplayName = null;
		EffectMessageName = null;
		EffectDuration = 0;
		SpringingEffective = false;
		bool flag = true;
		int num = 0;
		if (flag && GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetRunningBehavior"))
		{
			Event obj = Event.New("GetRunningBehavior");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("AbilityName", AbilityName);
			obj.SetParameter("Verb", Verb);
			obj.SetParameter("EffectDisplayName", EffectDisplayName);
			obj.SetParameter("EffectMessageName", EffectMessageName);
			obj.SetParameter("EffectDuration", EffectDuration);
			obj.SetParameter("Stats", Stats);
			obj.SetFlag("SpringingEffective", SpringingEffective);
			obj.SetParameter("Priority", num);
			flag = Actor.FireEvent(obj);
			AbilityName = obj.GetStringParameter("AbilityName");
			Verb = obj.GetStringParameter("Verb");
			EffectDisplayName = obj.GetStringParameter("EffectDisplayName");
			EffectMessageName = obj.GetStringParameter("EffectMessageName");
			EffectDuration = obj.GetIntParameter("EffectDuration");
			SpringingEffective = obj.HasFlag("SpringingEffective");
			num = obj.GetIntParameter("Priority");
		}
		if (flag && GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetRunningBehaviorEvent>.ID, CascadeLevel))
		{
			GetRunningBehaviorEvent getRunningBehaviorEvent = PooledEvent<GetRunningBehaviorEvent>.FromPool();
			getRunningBehaviorEvent.Actor = Actor;
			getRunningBehaviorEvent.AbilityName = AbilityName;
			getRunningBehaviorEvent.Verb = Verb;
			getRunningBehaviorEvent.EffectDisplayName = EffectDisplayName;
			getRunningBehaviorEvent.EffectMessageName = EffectMessageName;
			getRunningBehaviorEvent.EffectDuration = EffectDuration;
			getRunningBehaviorEvent.SpringingEffective = SpringingEffective;
			getRunningBehaviorEvent.Stats = Stats;
			getRunningBehaviorEvent.Priority = num;
			flag = Actor.HandleEvent(getRunningBehaviorEvent);
			AbilityName = getRunningBehaviorEvent.AbilityName;
			Verb = getRunningBehaviorEvent.Verb;
			EffectDisplayName = getRunningBehaviorEvent.EffectDisplayName;
			EffectMessageName = getRunningBehaviorEvent.EffectMessageName;
			EffectDuration = getRunningBehaviorEvent.EffectDuration;
			SpringingEffective = getRunningBehaviorEvent.SpringingEffective;
			num = getRunningBehaviorEvent.Priority;
		}
	}
}

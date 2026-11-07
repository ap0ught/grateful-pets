namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetJumpingBehaviorEvent : PooledEvent<GetJumpingBehaviorEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public string AbilityName;

	public string Verb;

	public string ProviderKey;

	public int RangeModifier;

	public int MinimumRange;

	public int Priority;

	public bool CanJumpOverCreatures;

	public Templates.StatCollector Stats;

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
		ProviderKey = null;
		RangeModifier = 0;
		MinimumRange = 0;
		Priority = 0;
		CanJumpOverCreatures = false;
		Stats = null;
	}

	public static void Retrieve(GameObject Actor, out int RangeModifier, out int MinimumRange, out string AbilityName, out string Verb, out string ProviderKey, out bool CanJumpOverCreatures, Templates.StatCollector Stats = null)
	{
		RangeModifier = 0;
		MinimumRange = 2;
		AbilityName = "Jump";
		Verb = "jump";
		ProviderKey = "Acrobatics_Jump";
		CanJumpOverCreatures = false;
		int priority = 0;
		GetJumpingBehaviorEvent getJumpingBehaviorEvent = PooledEvent<GetJumpingBehaviorEvent>.FromPool();
		getJumpingBehaviorEvent.Actor = Actor;
		getJumpingBehaviorEvent.AbilityName = AbilityName;
		getJumpingBehaviorEvent.Verb = Verb;
		getJumpingBehaviorEvent.ProviderKey = ProviderKey;
		getJumpingBehaviorEvent.RangeModifier = RangeModifier;
		getJumpingBehaviorEvent.MinimumRange = MinimumRange;
		getJumpingBehaviorEvent.Priority = priority;
		getJumpingBehaviorEvent.CanJumpOverCreatures = CanJumpOverCreatures;
		getJumpingBehaviorEvent.Stats = Stats;
		Actor.HandleEvent(getJumpingBehaviorEvent);
		AbilityName = getJumpingBehaviorEvent.AbilityName;
		Verb = getJumpingBehaviorEvent.Verb;
		ProviderKey = getJumpingBehaviorEvent.ProviderKey;
		RangeModifier = getJumpingBehaviorEvent.RangeModifier;
		MinimumRange = getJumpingBehaviorEvent.MinimumRange;
		CanJumpOverCreatures = getJumpingBehaviorEvent.CanJumpOverCreatures;
		priority = getJumpingBehaviorEvent.Priority;
	}
}

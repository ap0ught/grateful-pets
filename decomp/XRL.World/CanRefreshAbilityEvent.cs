using XRL.World.Parts;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class CanRefreshAbilityEvent : PooledEvent<CanRefreshAbilityEvent>
{
	public GameObject Actor;

	public ActivatedAbilityEntry Ability;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Ability = null;
	}

	public static bool Check(GameObject Actor, ActivatedAbilityEntry Ability)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<CanRefreshAbilityEvent>.ID, MinEvent.CascadeLevel))
		{
			CanRefreshAbilityEvent canRefreshAbilityEvent = PooledEvent<CanRefreshAbilityEvent>.FromPool();
			canRefreshAbilityEvent.Actor = Actor;
			canRefreshAbilityEvent.Ability = Ability;
			return Actor.HandleEvent(canRefreshAbilityEvent);
		}
		return true;
	}
}

namespace XRL.World;

[GameEvent(Cascade = 64, Cache = Cache.Pool)]
public class GetFeelingEvent : PooledEvent<GetFeelingEvent>
{
	public new static readonly int CascadeLevel = 64;

	public GameObject Actor;

	public GameObject ActorLeader;

	public GameObject Target;

	public GameObject TargetLeader;

	public int BaseFeeling;

	public int Feeling;

	/// <summary>Actor and Target are engaged in combat.</summary>
	public bool Combat;

	/// <summary>BaseFeeling was retrieved from personal memory.</summary>
	public bool Personal;

	/// <summary>BaseFeeling is based on faction feeling.</summary>
	public bool Faction;

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
		ActorLeader = null;
		Target = null;
		TargetLeader = null;
		BaseFeeling = 0;
		Feeling = 0;
		Combat = false;
		Personal = false;
		Faction = false;
	}

	public static int GetFor(GameObject Actor, GameObject Target, int BaseFeeling, GameObject ActorLeader = null, GameObject TargetLeader = null, bool Combat = false, bool Personal = false, bool Faction = false)
	{
		int result = BaseFeeling;
		if (Actor.RegisteredEvents != null && Actor.RegisteredEvents.TryGetValue(PooledEvent<GetFeelingEvent>.ID, out var Handlers))
		{
			GetFeelingEvent E = PooledEvent<GetFeelingEvent>.FromPool();
			E.Actor = Actor;
			E.ActorLeader = ActorLeader;
			E.Target = Target;
			E.TargetLeader = TargetLeader;
			E.BaseFeeling = BaseFeeling;
			E.Feeling = BaseFeeling;
			E.Combat = Combat;
			E.Personal = Personal;
			E.Faction = Faction;
			Handlers.Dispatch(E);
			result = E.Feeling;
			PooledEvent<GetFeelingEvent>.ResetTo(ref E);
		}
		return result;
	}
}

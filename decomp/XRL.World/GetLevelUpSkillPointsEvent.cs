namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetLevelUpSkillPointsEvent : PooledEvent<GetLevelUpSkillPointsEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public int BaseAmount;

	public int Amount;

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
		BaseAmount = 0;
		Amount = 0;
	}

	public static GetLevelUpSkillPointsEvent FromPool(GameObject Actor, int BaseAmount, int Amount)
	{
		GetLevelUpSkillPointsEvent getLevelUpSkillPointsEvent = PooledEvent<GetLevelUpSkillPointsEvent>.FromPool();
		getLevelUpSkillPointsEvent.Actor = Actor;
		getLevelUpSkillPointsEvent.BaseAmount = BaseAmount;
		getLevelUpSkillPointsEvent.Amount = Amount;
		return getLevelUpSkillPointsEvent;
	}

	public static int GetFor(GameObject Actor, int BaseAmount)
	{
		if (GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetLevelUpSkillPointsEvent>.ID, CascadeLevel))
		{
			GetLevelUpSkillPointsEvent getLevelUpSkillPointsEvent = FromPool(Actor, BaseAmount, BaseAmount);
			Actor.HandleEvent(getLevelUpSkillPointsEvent);
			return getLevelUpSkillPointsEvent.Amount;
		}
		return BaseAmount;
	}
}

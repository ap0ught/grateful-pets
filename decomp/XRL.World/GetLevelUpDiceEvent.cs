namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetLevelUpDiceEvent : PooledEvent<GetLevelUpDiceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public int Level;

	public string BaseHPGain;

	public string BaseSPGain;

	public string BaseMPGain;

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
		Level = 0;
		BaseHPGain = null;
		BaseSPGain = null;
		BaseMPGain = null;
	}

	public static void GetFor(GameObject Actor, int Level, ref string BaseHPGain, ref string BaseSPGain, ref string BaseMPGain)
	{
		if (Actor.HasRegisteredEvent("GetLevelUpDice"))
		{
			Event obj = Event.New("GetLevelUpDice");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Level", Level);
			obj.SetParameter("BaseHPGain", BaseHPGain);
			obj.SetParameter("BaseSPGain", BaseSPGain);
			obj.SetParameter("BaseMPGain", BaseMPGain);
			bool flag = Actor.FireEvent(obj);
			BaseHPGain = obj.GetStringParameter("BaseHPGain");
			BaseSPGain = obj.GetStringParameter("BaseSPGain");
			BaseMPGain = obj.GetStringParameter("BaseMPGain");
			if (!flag)
			{
				return;
			}
		}
		if (Actor.WantEvent(PooledEvent<GetLevelUpDiceEvent>.ID, CascadeLevel))
		{
			GetLevelUpDiceEvent getLevelUpDiceEvent = PooledEvent<GetLevelUpDiceEvent>.FromPool();
			getLevelUpDiceEvent.Actor = Actor;
			getLevelUpDiceEvent.Level = Level;
			getLevelUpDiceEvent.BaseHPGain = BaseHPGain;
			getLevelUpDiceEvent.BaseSPGain = BaseSPGain;
			getLevelUpDiceEvent.BaseMPGain = BaseMPGain;
			Actor.HandleEvent(getLevelUpDiceEvent);
			BaseHPGain = getLevelUpDiceEvent.BaseHPGain;
			BaseSPGain = getLevelUpDiceEvent.BaseSPGain;
			BaseMPGain = getLevelUpDiceEvent.BaseMPGain;
		}
	}
}

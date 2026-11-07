namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetLevelUpPointsEvent : PooledEvent<GetLevelUpPointsEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public int Level;

	public int HitPoints;

	public int SkillPoints;

	public int MutationPoints;

	public int AttributePoints;

	public int AttributeBonus;

	public int RapidAdvancement;

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
		HitPoints = 0;
		SkillPoints = 0;
		MutationPoints = 0;
		AttributePoints = 0;
		AttributeBonus = 0;
		RapidAdvancement = 0;
	}

	public static void GetFor(GameObject Actor, int Level, ref int HitPoints, ref int SkillPoints, ref int MutationPoints, ref int AttributePoints, ref int AttributeBonus, ref int RapidAdvancement)
	{
		if (Actor.HasRegisteredEvent("GetLevelUpPoints"))
		{
			Event obj = Event.New("GetLevelUpPoints");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Level", Level);
			obj.SetParameter("HitPoints", HitPoints);
			obj.SetParameter("SkillPoints", SkillPoints);
			obj.SetParameter("MutationPoints", MutationPoints);
			obj.SetParameter("AttributePoints", AttributePoints);
			obj.SetParameter("AttributeBonus", AttributeBonus);
			obj.SetParameter("RapidAdvancement", RapidAdvancement);
			bool flag = Actor.FireEvent(obj);
			HitPoints = obj.GetIntParameter("HitPoints");
			SkillPoints = obj.GetIntParameter("SkillPoints");
			MutationPoints = obj.GetIntParameter("MutationPoints");
			AttributePoints = obj.GetIntParameter("AttributePoints");
			AttributeBonus = obj.GetIntParameter("AttributeBonus");
			RapidAdvancement = obj.GetIntParameter("RapidAdvancement");
			if (!flag)
			{
				return;
			}
		}
		if (Actor.WantEvent(PooledEvent<GetLevelUpPointsEvent>.ID, CascadeLevel))
		{
			GetLevelUpPointsEvent getLevelUpPointsEvent = PooledEvent<GetLevelUpPointsEvent>.FromPool();
			getLevelUpPointsEvent.Actor = Actor;
			getLevelUpPointsEvent.Level = Level;
			getLevelUpPointsEvent.HitPoints = HitPoints;
			getLevelUpPointsEvent.SkillPoints = SkillPoints;
			getLevelUpPointsEvent.MutationPoints = MutationPoints;
			getLevelUpPointsEvent.AttributePoints = AttributePoints;
			getLevelUpPointsEvent.AttributeBonus = AttributeBonus;
			getLevelUpPointsEvent.RapidAdvancement = RapidAdvancement;
			Actor.HandleEvent(getLevelUpPointsEvent);
			HitPoints = getLevelUpPointsEvent.HitPoints;
			SkillPoints = getLevelUpPointsEvent.SkillPoints;
			MutationPoints = getLevelUpPointsEvent.MutationPoints;
			AttributePoints = getLevelUpPointsEvent.AttributePoints;
			AttributeBonus = getLevelUpPointsEvent.AttributeBonus;
			RapidAdvancement = getLevelUpPointsEvent.RapidAdvancement;
		}
	}
}

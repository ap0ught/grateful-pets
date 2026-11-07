using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetSkillEffectChanceEvent : PooledEvent<GetSkillEffectChanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Object;

	public BaseSkill Skill;

	public int BaseChance;

	public int Chance;

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
		Object = null;
		Skill = null;
		BaseChance = 0;
		Chance = 0;
	}

	public static int GetFor(GameObject Actor, GameObject Object, BaseSkill Skill, int Chance = 0, bool ConstrainToPercentage = true, bool ConstrainToPermillage = false)
	{
		int num = Chance;
		bool flag = true;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Actor) && Actor.HasRegisteredEvent("GetSpecialEffectChance");
			bool flag3 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetSpecialEffectChance");
			if (flag2 | flag3)
			{
				Event obj = Event.New("GetSpecialEffectChance");
				obj.SetParameter("Actor", Actor);
				obj.SetParameter("Object", Object);
				obj.SetParameter("Skill", Skill);
				obj.SetParameter("BaseChance", num);
				obj.SetParameter("Chance", Chance);
				if (flag & flag2)
				{
					flag = Actor.FireEvent(obj);
				}
				if (flag & flag3)
				{
					flag = Object.FireEvent(obj);
				}
				Chance = obj.GetIntParameter("Chance");
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Actor) && Actor.WantEvent(PooledEvent<GetSkillEffectChanceEvent>.ID, CascadeLevel);
			bool flag5 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetSkillEffectChanceEvent>.ID, CascadeLevel);
			if (flag4 | flag5)
			{
				GetSkillEffectChanceEvent getSkillEffectChanceEvent = PooledEvent<GetSkillEffectChanceEvent>.FromPool();
				getSkillEffectChanceEvent.Actor = Actor;
				getSkillEffectChanceEvent.Object = Object;
				getSkillEffectChanceEvent.Skill = Skill;
				getSkillEffectChanceEvent.BaseChance = num;
				getSkillEffectChanceEvent.Chance = Chance;
				if (flag & flag4)
				{
					flag = Actor.HandleEvent(getSkillEffectChanceEvent);
				}
				if (flag & flag5)
				{
					flag = Object.HandleEvent(getSkillEffectChanceEvent);
				}
				Chance = getSkillEffectChanceEvent.Chance;
			}
		}
		if (ConstrainToPercentage)
		{
			if (Chance > 100)
			{
				Chance = 100;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		else if (ConstrainToPermillage)
		{
			if (Chance > 1000)
			{
				Chance = 1000;
			}
			else if (Chance < 0)
			{
				Chance = 0;
			}
		}
		return Chance;
	}
}

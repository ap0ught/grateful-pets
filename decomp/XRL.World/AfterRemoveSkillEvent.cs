using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AfterRemoveSkillEvent : PooledEvent<AfterRemoveSkillEvent>
{
	public new static readonly int CascadeLevel = 15;

	public GameObject Actor;

	public BaseSkill Skill;

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
		Skill = null;
	}

	public static AfterRemoveSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		AfterRemoveSkillEvent afterRemoveSkillEvent = PooledEvent<AfterRemoveSkillEvent>.FromPool();
		afterRemoveSkillEvent.Actor = Actor;
		afterRemoveSkillEvent.Skill = Skill;
		return afterRemoveSkillEvent;
	}

	public static void Send(GameObject Actor, BaseSkill Skill)
	{
		if (!GameObject.Validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("AfterRemoveSkill"))
		{
			Event e = Event.New("AfterRemoveSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(PooledEvent<AfterRemoveSkillEvent>.ID, CascadeLevel))
		{
			AfterRemoveSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

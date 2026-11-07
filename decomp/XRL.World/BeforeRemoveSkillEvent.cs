using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class BeforeRemoveSkillEvent : PooledEvent<BeforeRemoveSkillEvent>
{
	public new static readonly int CascadeLevel = 17;

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

	public static BeforeRemoveSkillEvent FromPool(GameObject Actor, BaseSkill Skill)
	{
		BeforeRemoveSkillEvent beforeRemoveSkillEvent = PooledEvent<BeforeRemoveSkillEvent>.FromPool();
		beforeRemoveSkillEvent.Actor = Actor;
		beforeRemoveSkillEvent.Skill = Skill;
		return beforeRemoveSkillEvent;
	}

	public static void Send(GameObject Actor, BaseSkill Skill)
	{
		if (!GameObject.Validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("BeforeRemoveSkill"))
		{
			Event e = Event.New("BeforeRemoveSkill", "Actor", Actor, "Skill", Skill);
			if (!Actor.FireEvent(e))
			{
				return;
			}
		}
		if (Actor.WantEvent(PooledEvent<BeforeRemoveSkillEvent>.ID, CascadeLevel))
		{
			BeforeRemoveSkillEvent e2 = FromPool(Actor, Skill);
			Actor.HandleEvent(e2);
		}
	}
}

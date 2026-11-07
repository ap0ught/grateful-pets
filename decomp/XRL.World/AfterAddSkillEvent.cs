using System.Collections.Generic;
using XRL.World.Parts.Skill;

namespace XRL.World;

[GameEvent(Cascade = 15, Cache = Cache.Pool)]
public class AfterAddSkillEvent : IAddSkillEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(AfterAddSkillEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 15;

	private static List<AfterAddSkillEvent> Pool;

	private static int PoolCounter;

	public List<BaseSkill> Include;

	public AfterAddSkillEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref AfterAddSkillEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static AfterAddSkillEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Include = null;
	}

	public static void Send(GameObject Actor, BaseSkill Skill, IBaseSkillEntry Entry, GameObject Source = null, string Context = null, List<BaseSkill> Include = null)
	{
		if (!GameObject.Validate(Actor))
		{
			return;
		}
		if (Actor.HasRegisteredEvent("AfterAddSkill"))
		{
			Event obj = Event.New("AfterAddSkill");
			obj.SetParameter("Actor", Actor);
			obj.SetParameter("Skill", Skill);
			obj.SetParameter("Entry", Entry);
			obj.SetParameter("Source", Source);
			obj.SetParameter("Context", Context);
			obj.SetParameter("Include", Include);
			if (!Actor.FireEvent(obj))
			{
				return;
			}
		}
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			AfterAddSkillEvent afterAddSkillEvent = FromPool();
			afterAddSkillEvent.Actor = Actor;
			afterAddSkillEvent.Source = Source;
			afterAddSkillEvent.Skill = Skill;
			afterAddSkillEvent.Entry = Entry;
			afterAddSkillEvent.Context = Context;
			afterAddSkillEvent.Include = Include;
			Actor.HandleEvent(afterAddSkillEvent);
		}
	}
}

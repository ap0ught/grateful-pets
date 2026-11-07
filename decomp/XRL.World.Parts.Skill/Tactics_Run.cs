using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tactics_Run : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetRunningBehaviorEvent>.ID)
		{
			return ID == PooledEvent<PartSupportEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Run")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRunningBehaviorEvent E)
	{
		if (E.Priority < 10)
		{
			int num = GetSprintDurationEvent.GetFor(E.Actor, 10, 0, 0, 0, 0, E.Stats);
			if (num > 0)
			{
				E.AbilityName = "Sprint";
				E.Verb = "sprint";
				E.EffectDisplayName = "sprinting";
				E.EffectMessageName = "sprinting";
				E.EffectDuration = num;
				E.SpringingEffective = true;
				E.Priority = 10;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.RequirePart<Run>();
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		NeedPartSupportEvent.Send(GO, "Run", this);
		return base.AddSkill(GO);
	}

	public override void Initialize()
	{
		base.Initialize();
		Run.SyncAbility(ParentObject);
	}
}

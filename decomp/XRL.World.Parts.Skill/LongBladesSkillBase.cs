namespace XRL.World.Parts.Skill;

public abstract class LongBladesSkillBase : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<PartSupportEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "LongBladesCore")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AddSkill(GameObject Object)
	{
		Object.RequirePart<LongBladesCore>();
		return base.AddSkill(Object);
	}

	public override bool RemoveSkill(GameObject Object)
	{
		NeedPartSupportEvent.Send(Object, "LongBladesCore", this);
		return base.RemoveSkill(Object);
	}
}

using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Physic_Apothecary : BaseSkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetTonicDurationEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 3);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTonicDurationEvent E)
	{
		if (E.Pass == 1 && E.Healing && E.Checking == "Actor" && E.Actor == ParentObject)
		{
			E.Duration++;
		}
		return base.HandleEvent(E);
	}
}

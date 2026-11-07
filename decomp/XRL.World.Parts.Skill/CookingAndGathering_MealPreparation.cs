using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_MealPreparation : BaseSkill
{
	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("salt", 3);
		}
		return base.HandleEvent(E);
	}
}

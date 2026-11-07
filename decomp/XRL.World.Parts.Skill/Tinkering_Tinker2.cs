using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Tinkering_Tinker2 : BaseSkill
{
	public override bool AddSkill(GameObject GO)
	{
		if (GO.GetIntProperty("ReceivedTinker2Recipe") <= 0 && (GO.CurrentCell != null || GO.IsOriginalPlayerBody()))
		{
			Tinkering.LearnNewRecipe(GO, 4, 6);
			GO.SetIntProperty("ReceivedTinker2Recipe", 1);
		}
		if (GO.IsPlayer())
		{
			TinkeringSifrah.AwardInsight();
		}
		return base.AddSkill(GO);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", 2);
		}
		return base.HandleEvent(E);
	}
}

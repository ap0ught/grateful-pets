using System;
using XRL.World.Effects;

namespace XRL.World.Parts.Skill;

[Serializable]
public class CookingAndGathering_CarbideChef : BaseSkill
{
	public void Inspire()
	{
		if (!ParentObject.HasEffect<Inspired>())
		{
			ParentObject.ApplyEffect(new Inspired(2400));
		}
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != PooledEvent<AfterLevelGainedEvent>.ID)
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

	public override bool HandleEvent(AfterLevelGainedEvent E)
	{
		if (E.Actor.IsPlayer() && E.Actor == ParentObject)
		{
			Inspire();
		}
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VisitingNewZone" && ParentObject.IsPlayer() && 5.in100())
		{
			Inspire();
		}
		return base.FireEvent(E);
	}

	public override bool AddSkill(GameObject GO)
	{
		GO.RegisterPartEvent(this, "VisitingNewZone");
		return base.AddSkill(GO);
	}

	public override bool RemoveSkill(GameObject GO)
	{
		GO.UnregisterPartEvent(this, "VisitingNewZone");
		return base.AddSkill(GO);
	}
}

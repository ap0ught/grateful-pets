using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class Yonderbrush : IPart
{
	public bool Harvested;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != GetNavigationWeightEvent.ID || Harvested))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!Harvested)
		{
			Hidden part = ParentObject.GetPart<Hidden>();
			if ((part == null || part.Found) && E.PhaseMatches(ParentObject))
			{
				E.MinWeight(95);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!Harvested && ParentObject.CurrentCell != null)
		{
			if (E.Object != null && E.Object.IsCombat && ParentObject.Brain.IsHostileTowards(E.Object) && E.Object != ParentObject)
			{
				Hidden part = ParentObject.GetPart<Hidden>();
				if ((part == null || part.Found) && E.Object.HasPart<CookingAndGathering_Harvestry>())
				{
					CookingAndGathering_Harvestry part2 = E.Object.GetPart<CookingAndGathering_Harvestry>();
					if (part2.IsMyActivatedAbilityToggledOn(part2.ActivatedAbilityID))
					{
						Harvested = true;
						return true;
					}
				}
				if (part != null)
				{
					part.Found = true;
				}
				E.Object.RandomTeleport(Swirl: true);
			}
			else if (E.Object != null && E.Object.IsPlayer() && !ParentObject.IsHostileTowards(E.Object))
			{
				ParentObject.GetPart<Hidden>()?.Reveal();
			}
		}
		return base.HandleEvent(E);
	}
}

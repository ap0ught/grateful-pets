using System;
using System.Linq;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class SurroundOnStep : IPart
{
	public string SurroundObject = "PlantWall";

	public bool EmptyOnly = true;

	public bool AllowHarvest;

	public bool Harvested;

	public int Radius = 1;

	[NonSerialized]
	public static string[] ColorList = new string[6] { "&R", "&G", "&B", "&M", "&Y", "&W" };

	public static string GetRandomRainbowColor()
	{
		return ColorList.GetRandomElement();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		if (!Harvested && E.Object != null && E.Object != ParentObject && E.Object.IsCombatObject() && ParentObject.IsHostileTowards(E.Object) && ParentObject.PhaseAndFlightMatches(E.Object))
		{
			Hidden part = ParentObject.GetPart<Hidden>();
			if (AllowHarvest && (part == null || part.Found) && E.Object.HasPart<CookingAndGathering_Harvestry>())
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
			foreach (Cell item in from c in E.Cell.GetLocalAdjacentCells(Radius)
				where c.Location.ManhattanDistance(ParentObject.Physics.CurrentCell.Location) == Radius
				select c)
			{
				if (!EmptyOnly || item.IsEmpty())
				{
					item.AddObject(SurroundObject, "Spawning");
					if (50.in100())
					{
						The.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\r", item.X, item.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
					}
					else
					{
						The.ParticleManager.AddSinusoidal(GetRandomRainbowColor() + "\u000e", item.X, item.Y, 1.5f * (float)Stat.Random(1, 6), 0.1f * (float)Stat.Random(1, 60), 0.1f + 0.025f * (float)Stat.Random(0, 4), 1f, 0f, 0f, -0.1f - 0.05f * (float)Stat.Random(1, 6), 999, 0L);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}
}

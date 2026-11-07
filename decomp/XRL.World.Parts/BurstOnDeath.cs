using System;

namespace XRL.World.Parts;

[Serializable]
public class BurstOnDeath : IPart
{
	public string Blueprint;

	public int Radius = 1;

	public int Chance = 100;

	public bool SkipOccluding;

	public bool SkipSolid;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDeathRemovalEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDeathRemovalEvent E)
	{
		if (!Blueprint.IsNullOrEmpty() && !ParentObject.IsNowhere())
		{
			foreach (Cell item in ParentObject.CurrentCell.IterateAdjacent(Radius, IncludeSelf: true))
			{
				if ((!SkipOccluding || !item.IsOccluding()) && (!SkipSolid || !item.IsSolid()) && Chance.in100())
				{
					item.AddObject(Blueprint.GetRandomSubstring(','));
				}
			}
		}
		return base.HandleEvent(E);
	}
}

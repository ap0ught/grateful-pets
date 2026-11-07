using System;

namespace XRL.World.Parts;

[Serializable]
public class MissileCover : IPart
{
	public int Percentage;

	public bool DisabledByPenetrateCreatures = true;

	public bool DisabledByPenetrateWalls = true;

	public override bool SameAs(IPart p)
	{
		MissileCover missileCover = p as MissileCover;
		if (missileCover.Percentage != Percentage)
		{
			return false;
		}
		if (missileCover.DisabledByPenetrateCreatures != DisabledByPenetrateCreatures)
		{
			return false;
		}
		if (missileCover.DisabledByPenetrateWalls != DisabledByPenetrateWalls)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetMissileCoverPercentageEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMissileCoverPercentageEvent E)
	{
		if (!DisabledByPenetrateCreatures || !E.PenetrateCreatures || !DisabledByPenetrateWalls || !E.PenetrateWalls)
		{
			E.MinPercentage(Percentage);
		}
		return base.HandleEvent(E);
	}
}

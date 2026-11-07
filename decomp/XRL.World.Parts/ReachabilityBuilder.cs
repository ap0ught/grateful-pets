using System;

namespace XRL.World.Parts;

[Serializable]
public class ReachabilityBuilder : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			Zone currentZone = ParentObject.CurrentZone;
			for (int i = 0; i < currentZone.Height; i++)
			{
				for (int j = 0; j < currentZone.Width; j++)
				{
					Cell cell = currentZone.GetCell(j, i);
					if (!cell.IsReachable() && !cell.IsSolid())
					{
						currentZone.BuildReachableMap(j, i);
					}
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("Reachability build", x);
		}
		return base.HandleEvent(E);
	}
}

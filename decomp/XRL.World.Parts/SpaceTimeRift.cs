using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SpaceTimeRift : SpaceTimeVortex
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetPointsOfInterestEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetPointsOfInterestEvent E)
	{
		if (E.StandardChecks(this, E.Actor))
		{
			E.Add(ParentObject, ParentObject.GetReferenceDisplayName(), null, null, null, null, null, 2);
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Stat.RandomCosmetic(1, 60) < 3)
		{
			string text = "&C";
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&W";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&R";
			}
			if (Stat.RandomCosmetic(1, 3) == 1)
			{
				text = "&B";
			}
			Cell cell = ParentObject.CurrentCell;
			XRLCore.ParticleManager.AddRadial(text + "ù", cell.X, cell.Y, Stat.RandomCosmetic(0, 7), Stat.RandomCosmetic(5, 10), 0.01f * (float)Stat.RandomCosmetic(4, 6), -0.01f * (float)Stat.RandomCosmetic(3, 7));
		}
		switch (Stat.RandomCosmetic(0, 4))
		{
		case 0:
			E.ColorString = "&B^k";
			break;
		case 1:
			E.ColorString = "&R^k";
			break;
		case 2:
			E.ColorString = "&C^k";
			break;
		case 3:
			E.ColorString = "&W^k";
			break;
		case 4:
			E.ColorString = "&K^k";
			break;
		}
		switch (Stat.RandomCosmetic(0, 3))
		{
		case 0:
			E.RenderString = "\t";
			break;
		case 1:
			E.RenderString = "é";
			break;
		case 2:
			E.RenderString = "\u0015";
			break;
		case 3:
			E.RenderString = "\u000f";
			break;
		}
		return true;
	}

	public override int SpaceTimeAnomalyEmergencePermillageBaseChance()
	{
		return 5;
	}

	public override int SpaceTimeAnomalyEmergenceExplodePercentageBaseChance()
	{
		return 5;
	}

	public override bool SpaceTimeAnomalyStationary()
	{
		return true;
	}
}

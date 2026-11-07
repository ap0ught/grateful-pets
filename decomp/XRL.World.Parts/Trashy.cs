using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Trashy : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

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
			if (!Options.DisableFloorTextureObjects)
			{
				Zone parentZone = E.Cell.ParentZone;
				for (int i = 0; i < parentZone.Height; i++)
				{
					for (int j = 0; j < parentZone.Width; j++)
					{
						PaintCell(parentZone.GetCell(j, i));
					}
				}
			}
		}
		catch
		{
		}
		return base.HandleEvent(E);
	}

	public static void PaintCell(Cell C)
	{
		C.PaintTile = "Tiles/tile-dirt1.png";
		int num = Stat.Random(1, 8);
		if (num <= 5)
		{
			C.PaintColorString = "&g";
		}
		else
		{
			switch (num)
			{
			case 6:
				C.PaintColorString = "&c";
				break;
			case 7:
				C.PaintColorString = "&w";
				break;
			default:
				C.PaintColorString = "&g";
				break;
			}
		}
		C.PaintRenderString = "ú";
	}
}

using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class UndergroundGrassy : IPart
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
		int num = Stat.Random(1, 8);
		if (num <= 2)
		{
			C.PaintColorString = "&g";
			C.PaintDetailColor = "w";
			C.PaintTile = "Creatures/sw_plant3.bmp";
		}
		else if (num <= 4)
		{
			C.PaintColorString = "&G";
			C.PaintDetailColor = "w";
			C.PaintTile = "Creatures/sw_plant3.bmp";
		}
		else if (num <= 6)
		{
			C.PaintColorString = "&w";
			C.PaintDetailColor = "k";
			C.PaintTile = "Tiles/tile-dirt1.png";
		}
		else
		{
			C.PaintColorString = "&y";
			C.PaintDetailColor = "k";
			C.PaintTile = "Tiles/tile-dirt1.png";
		}
		int num2 = Stat.Random(1, 5);
		if (num2 == 1)
		{
			C.PaintRenderString = ".";
		}
		if (num2 == 2)
		{
			C.PaintRenderString = ",";
		}
		if (num2 == 3)
		{
			C.PaintRenderString = "`";
		}
		if (num2 == 4)
		{
			C.PaintRenderString = "'";
		}
	}
}

using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class BlueTile : IPart
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
		if ((C.X + C.Y) % 2 == 0)
		{
			C.PaintColorString = "&y";
			C.PaintTile = "terrain/sw_ground_dots1.png";
			C.PaintDetailColor = "Y";
		}
		else
		{
			C.PaintColorString = "&c";
			C.PaintTile = "terrain/sw_ground_dots1.png";
			C.PaintDetailColor = "C";
		}
	}
}

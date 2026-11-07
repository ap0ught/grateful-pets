using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ConcreteFloor : IPart
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
		if (string.IsNullOrEmpty(C.PaintTile))
		{
			C.PaintColorString = "&y";
			C.PaintTile = "Tiles/tile-dirt1.png";
			C.PaintDetailColor = "k";
			C.PaintColorString = "&y";
			C.PaintRenderString = "ú";
		}
	}
}

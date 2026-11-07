using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class PaleDirty : IPart
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
		if (num <= 5)
		{
			C.PaintColorString = "&y";
			C.PaintTile = "Tiles/tile-dirt1.png";
			C.PaintDetailColor = "k";
		}
		else
		{
			switch (num)
			{
			case 6:
				C.PaintColorString = "&y";
				C.PaintTile = DirtPicker.GetRandomGrassTile();
				break;
			case 7:
				C.PaintColorString = "&Y";
				C.PaintTile = DirtPicker.GetRandomGrassTile();
				break;
			default:
				C.PaintColorString = "&y";
				C.PaintTile = "Tiles/tile-dirt1.png";
				C.PaintDetailColor = "k";
				break;
			}
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

using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Sultan1Tiles : IPart
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
						Paint(parentZone.GetCell(j, i));
					}
				}
			}
		}
		catch
		{
		}
		return base.HandleEvent(E);
	}

	public static void Paint(Cell C)
	{
		if (string.IsNullOrEmpty(C.PaintTile))
		{
			C.PaintColorString = "&K";
			C.PaintTile = "assets_content_textures_tiles_sw_floor_diamonds.bmp";
			C.PaintDetailColor = "k";
			if (Stat.RandomCosmetic(1, 100) <= 5)
			{
				C.PaintDetailColor = "k";
			}
			if (Stat.RandomCosmetic(1, 100) <= 1)
			{
				C.PaintDetailColor = "W";
			}
			C.PaintRenderString = ".";
		}
	}
}

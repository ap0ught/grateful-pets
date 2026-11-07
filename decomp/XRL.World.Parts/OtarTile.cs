using System;
using Genkit;

namespace XRL.World.Parts;

[Serializable]
public class OtarTile : IPart
{
	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupTile();
		return base.HandleEvent(E);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ZoneLoaded")
		{
			SetupTile();
		}
		return base.FireEvent(E);
	}

	public static int GetSeededRange(string Seed, int Low, int High)
	{
		return new Random(Hash.String(Seed)).Next(Low, High);
	}

	private void SetupTile()
	{
		Cell cell = ParentObject.CurrentCell;
		cell.PaintTile = "tiles/sw_floor_squares.bmp";
		cell.PaintTileColor = "&r";
		cell.PaintRenderString = "-";
		cell.PaintColorString = "&r";
		ParentObject.RemovePart(this);
	}
}

using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PaintedFlowers : IPart
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
		Paint(E.Cell);
		ParentObject.Obliterate();
		return false;
	}

	public static void Paint(Cell C)
	{
		C.PaintTile = "terrain/tile_flowers" + Stat.Random(1, 2) + ".bmp";
		switch (Stat.Random(1, 7))
		{
		case 1:
			C.PaintColorString = "&R";
			break;
		case 2:
			C.PaintColorString = "&M";
			break;
		case 3:
			C.PaintColorString = "&B";
			break;
		case 4:
			C.PaintColorString = "&C";
			break;
		case 5:
			C.PaintColorString = "&Y";
			break;
		case 6:
			C.PaintColorString = "&G";
			break;
		case 7:
			C.PaintColorString = "&W";
			break;
		}
		if (Stat.Random(0, 1) == 0)
		{
			C.PaintColorString = C.PaintColorString.ToLower();
		}
		int num = Stat.Random(1, 5);
		if (num == 1)
		{
			C.PaintRenderString = ",";
		}
		if (num == 2)
		{
			C.PaintRenderString = ".";
		}
		if (num == 3)
		{
			C.PaintRenderString = "ù";
		}
		if (num == 4)
		{
			C.PaintRenderString = "ú";
		}
	}
}

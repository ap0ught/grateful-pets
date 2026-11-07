using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AnimatedMaterialSaltDunes : IPart
{
	public int nFrameOffset;

	public AnimatedMaterialSaltDunes()
	{
		nFrameOffset = Stat.RandomCosmetic(0, 60);
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.Physics == null || ParentObject.Render == null || ParentObject.Physics.CurrentCell == null)
		{
			return true;
		}
		if ((XRLCore.CurrentFrame + nFrameOffset) % 60 % 30 == 0 && (ParentObject.Render.RenderString == "÷" || ParentObject.Render.RenderString == "~") && (ParentObject.Render.Tile == "Terrain/sw_desert.bmp" || ParentObject.Render.Tile == "Terrain/sw_plains.bmp") && Stat.RandomCosmetic(1, 20) == 1)
		{
			Cell cell = ParentObject.Physics.CurrentCell;
			Zone parentZone = cell.ParentZone;
			string direction = "W";
			if (Stat.RandomCosmetic(1, 2) == 1)
			{
				direction = "SW";
			}
			Cell cellFromDirection = cell.GetCellFromDirection(direction);
			if (cell.X != 0 && cell.Y != parentZone.Height - 1 && cellFromDirection != null && cellFromDirection.Objects.Count > 0 && cellFromDirection.Objects[0].GetPart("AnimatedMaterialSaltDunes") != null)
			{
				Render part = cellFromDirection.Objects[0].GetPart<Render>();
				ParentObject.Render.RenderString = part.RenderString;
				ParentObject.Render.Tile = part.Tile;
			}
			else if (Stat.RandomCosmetic(1, 2) == 1)
			{
				ParentObject.Render.RenderString = "÷";
			}
			else
			{
				ParentObject.Render.RenderString = "~";
			}
			if ((!(ParentObject.Render.RenderString == "÷") && !(ParentObject.Render.RenderString == "~")) || (!(ParentObject.Render.Tile == "Terrain/sw_desert.bmp") && !(ParentObject.Render.Tile == "Terrain/sw_plains.bmp")))
			{
				ParentObject.Render.RenderString = "÷";
				ParentObject.Render.Tile = "Terrain/sw_desert.bmp";
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}

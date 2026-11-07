using System;
using System.Collections.Generic;
using Genkit;
using XRL.EditorFormats.Map;

namespace XRL.World.Parts;

[Serializable]
public class MultiMapChunkPlacement : IPart
{
	public string Maps;

	public int MapsWide = 1;

	public int MapsHigh = 1;

	public int Width;

	public int Height;

	public int ColumnPadding;

	public int RowPadding;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			List<MapFile> list = new List<MapFile>();
			string[] array = Maps.Split(',');
			foreach (string iD in array)
			{
				list.Add(MapFile.Resolve(iD));
			}
			MapFileCell[,] array2 = new MapFileCell[(Width + ColumnPadding) * MapsWide - ColumnPadding, (Height + RowPadding) * MapsHigh - RowPadding];
			int num = 0;
			for (int j = 0; j < MapsHigh; j++)
			{
				for (int k = 0; k < MapsWide; k++)
				{
					MapFile mapFile = list[num];
					for (int l = 0; l < Width; l++)
					{
						for (int m = 0; m < Height; m++)
						{
							array2[l + k * Width + k * ColumnPadding, m + j * Height + j * RowPadding] = mapFile.Cells[l, m];
						}
					}
					num++;
				}
			}
			Zone parentZone = ParentObject.GetCurrentCell().ParentZone;
			Point2D pos2D = ParentObject.GetCurrentCell().Pos2D;
			int num2 = 2;
			if (HasTag("ChunkPadding"))
			{
				num2 = Convert.ToInt32("ChunkPadding");
			}
			int num3 = Width * MapsWide;
			int num4 = Height * MapsHigh;
			if (pos2D.x + num3 + num2 >= parentZone.Width)
			{
				pos2D.x -= pos2D.x + num3 + num2 - parentZone.Width;
			}
			if (pos2D.y + num4 + num2 >= parentZone.Height)
			{
				pos2D.y -= pos2D.y + num4 + num2 - parentZone.Height;
			}
			if (HasTag("ChunkHint") && GetTag("ChunkHint", "") == "Center")
			{
				pos2D.x = parentZone.Width / 2 - num3 / 2;
				pos2D.y = parentZone.Height / 2 - num4 / 2;
			}
			for (int n = 0; n < num4; n++)
			{
				for (int num5 = 0; num5 < num3; num5++)
				{
					Cell cell = parentZone.GetCell(pos2D.x + num5, pos2D.y + n);
					if (cell != null)
					{
						array2[num5, n]?.ApplyTo(cell, CheckEmpty: true, (Cell X) =>
						{
							X.ClearWalls();
						});
					}
				}
			}
		}
		return true;
	}
}

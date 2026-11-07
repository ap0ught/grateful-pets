using System.Linq;

namespace XRL.World.ZoneBuilders;

public class BananaSky : ZoneBuilderSandbox
{
	public bool BuildZone(Zone Z)
	{
		if (Z.Z > 0)
		{
			if (Z.Z < 10)
			{
				ZoneBuilderSandbox.PlaceObject("ConcreteFloor", Z);
			}
			_ = Z.GetTerrainNameFromDirection(".") == "TerrainTheSpindle";
			if (Z.Y == 2 && Z.GetTerrainNameFromDirection("S") == "TerrainTheSpindle")
			{
				if (Z.X == 0)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNW.rpm", PlacePrefabAlign.S, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.X == 1)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_N.rpm", PlacePrefabAlign.S, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.X == 2)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NNE.rpm", PlacePrefabAlign.S, null, (Cell c) =>
					{
						c.Clear();
					});
				}
			}
			if (Z.Y == 0 && Z.GetTerrainNameFromDirection("N") == "TerrainTheSpindle")
			{
				if (Z.X == 0)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSW.rpm", PlacePrefabAlign.N, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.X == 1)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_S.rpm", PlacePrefabAlign.N, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.X == 2)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SSE.rpm", PlacePrefabAlign.N, null, (Cell c) =>
					{
						c.Clear();
					});
				}
			}
			if (Z.X == 0 && Z.GetTerrainNameFromDirection("W") == "TerrainTheSpindle")
			{
				if (Z.Y == 0)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ENE.rpm", PlacePrefabAlign.W, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.Y == 1)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_E.rpm", PlacePrefabAlign.W, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.Y == 2)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_ESE.rpm", PlacePrefabAlign.W, null, (Cell c) =>
					{
						c.Clear();
					});
				}
			}
			if (Z.X == 2 && Z.GetTerrainNameFromDirection("E") == "TerrainTheSpindle")
			{
				if (Z.Y == 0)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WNW.rpm", PlacePrefabAlign.E, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.Y == 1)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_W.rpm", PlacePrefabAlign.E, null, (Cell c) =>
					{
						c.Clear();
					});
				}
				if (Z.Y == 2)
				{
					ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_WSW.rpm", PlacePrefabAlign.E, null, (Cell c) =>
					{
						c.Clear();
					});
				}
			}
			if (Z.X == 0 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NW") == "TerrainTheSpindle")
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SE.rpm", PlacePrefabAlign.NW, null, (Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 2 && Z.Y == 0 && Z.GetTerrainNameFromDirection("NE") == "TerrainTheSpindle")
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_SW.rpm", PlacePrefabAlign.NE, null, (Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 0 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SW") == "TerrainTheSpindle")
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NE.rpm", PlacePrefabAlign.SW, null, (Cell c) =>
				{
					c.Clear();
				});
			}
			if (Z.X == 2 && Z.Y == 2 && Z.GetTerrainNameFromDirection("SE") == "TerrainTheSpindle")
			{
				ZoneBuilderSandbox.PlacePrefab(Z, "preset_tile_chunks/TombExteriorWall_NW.rpm", PlacePrefabAlign.SE, null, (Cell c) =>
				{
					c.Clear();
				});
			}
		}
		foreach (Cell item in from c in Z.GetCells()
			where !c.HasObjectWithPart("Physics")
			select c)
		{
			item.AddObject("Air");
		}
		Z.GetCell(0, 0).AddObject(GameObjectFactory.Factory.CreateObject("DaylightWidget"));
		Z.ClearReachableMap();
		Z.BuildReachableMap(Z.Width / 2, Z.Height / 2);
		return true;
	}
}

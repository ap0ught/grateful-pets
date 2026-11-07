using System.Collections.Generic;

namespace XRL.World.ZoneBuilders;

public class ChildrenOfTheTomb : ZoneBuilderSandbox
{
	public void BuildZone(Zone zone)
	{
		The.Game.RequireGameState("ChildrenOfTheTombZoneCache", () => new Dictionary<int, string>());
		List<string> list = new List<string>();
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".0.0." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".1.0." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".2.0." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".0.1." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".2.1." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".0.2." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".1.2." + zone.Z);
		list.Add(zone.ZoneWorld + "." + zone.wX + "." + zone.wY + ".2.2." + zone.Z);
		List<string> list2 = The.Game.RequireGameState("ListOfUnplacedChildren", () => new List<string> { "Nacham,Doyoba", "Vaam,Dadogom", "Dagasha,Yona", "Kah,Gyamyo" });
		if (GetOracleListEntryFromInt(zone.Z, list) == zone.ZoneID)
		{
			string text = list2.RemoveRandomElement();
			zone.GetSpawnCell().AddObject(text.Split(',')[0]);
			Cell cell = zone.GetCellsWithObject("AnchorRoomTile").GetRandomElement();
			if (cell == null)
			{
				cell = zone.GetSpawnCell();
			}
			cell.AddObject(text.Split(',')[1]);
		}
	}
}

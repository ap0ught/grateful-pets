using System.Collections.Generic;

namespace XRL.World;

public class ZoneEncounterBlueprint
{
	public string Table;

	public string Amount;

	public Dictionary<string, string> Parameters = new Dictionary<string, string>();

	public List<ZoneFeature> Features = new List<ZoneFeature>();

	public ZoneEncounterBlueprint()
	{
		Table = "";
		Amount = "";
	}

	public ZoneEncounterBlueprint(string Table)
	{
		this.Table = Table;
		Amount = "medium";
	}

	public override string ToString()
	{
		return "ZoneEncounter<" + Table + ">";
	}
}

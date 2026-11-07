using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;

namespace XRL.World.Biomes;

public static class FungusColonizedTemplate
{
	public static void Apply(GameObject GO, string InfectionBlueprint, Zone Z)
	{
		if (GO?.Render == null || !GO.HasPart<Body>())
		{
			return;
		}
		GO.RequirePart<DisplayNameAdjectives>().RequireAdjective("fungus-ridden");
		GO.RequirePart<FungusRiddenIconColor>();
		FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		for (int num = Stat.Random(1, 4); num > 0; num--)
		{
			FungalSporeInfection.ApplyFungalInfection(GO, InfectionBlueprint);
		}
		if (!Z.GetZoneProperty("relaxedbiomes").EqualsNoCase("true"))
		{
			if (!GO.Brain.Allegiance.ContainsKey("Fungi"))
			{
				GO.Brain.Allegiance.Add("Fungi", 100);
			}
			else
			{
				GO.Brain.Allegiance["Fungi"] = 100;
			}
		}
	}
}

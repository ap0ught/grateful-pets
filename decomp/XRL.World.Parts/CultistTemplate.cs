namespace XRL.World.Parts;

public static class CultistTemplate
{
	public static void Apply(GameObject GO, string CultFaction)
	{
		GO.Brain.Allegiance.Clear();
		GO.Brain.Allegiance.Add(CultFaction, 100);
	}
}

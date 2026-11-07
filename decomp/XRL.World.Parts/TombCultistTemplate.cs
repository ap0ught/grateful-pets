using HistoryKit;
using XRL.Annals;

namespace XRL.World.Parts;

public static class TombCultistTemplate
{
	public static void Apply(GameObject GO, HistoricEntitySnapshot SultanSnapshot)
	{
		GO.DisplayName = GO.Render.DisplayName + " and death pilgrim of the {{Y|" + SultanSnapshot.GetProperty("cultName") + "}}";
		ConversationScript part = GO.GetPart<ConversationScript>();
		if (part != null)
		{
			part.Append = "\n\nTread in peace at the tomb of " + QudHistoryHelpers.GetRandomCognomen(SultanSnapshot) + ".";
		}
	}
}

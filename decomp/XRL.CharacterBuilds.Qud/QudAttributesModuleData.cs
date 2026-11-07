using System.Collections.Generic;

namespace XRL.CharacterBuilds.Qud;

public class QudAttributesModuleData : AbstractEmbarkBuilderModuleData
{
	public Dictionary<string, int> PointsPurchased = new Dictionary<string, int>();

	public int apSpent;

	public int apRemaining;

	public int baseAp;
}

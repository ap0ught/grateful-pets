using System.Collections.Generic;

namespace XRL.CharacterBuilds.Qud;

public class QudCyberneticsModuleData : AbstractEmbarkBuilderModuleData
{
	public int lp = -1;

	public List<QudCyberneticsModuleDataRow> selections = new List<QudCyberneticsModuleDataRow>();
}

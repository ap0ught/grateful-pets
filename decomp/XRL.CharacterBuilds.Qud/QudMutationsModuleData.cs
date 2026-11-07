using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace XRL.CharacterBuilds.Qud;

public class QudMutationsModuleData : AbstractEmbarkBuilderModuleData
{
	public static readonly Version CURRENT_VERSION = new Version(1, 2);

	public int mp = -1;

	public List<QudMutationModuleDataRow> selections = new List<QudMutationModuleDataRow>();

	[JsonIgnore]
	public static List<MutationCategory> categories => MutationFactory.GetCategories();

	public static MutationEntry getMutationEntryByName(string name)
	{
		return MutationFactory.GetMutationEntryByName(name);
	}

	public QudMutationsModuleData()
	{
		Version = CURRENT_VERSION;
	}

	[OnDeserialized]
	private void LegacySupport(StreamingContext Context)
	{
		if (!(Version < CURRENT_VERSION))
		{
			return;
		}
		foreach (QudMutationModuleDataRow selection in selections)
		{
			selection.Upgrade(Version);
		}
	}
}

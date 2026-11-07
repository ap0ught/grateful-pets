using System;

namespace XRL.CharacterBuilds.Qud;

public class QudSubtypeModuleData : AbstractEmbarkBuilderModuleData
{
	public string Subtype;

	[Obsolete]
	public SubtypeEntry info => SubtypeFactory.GetSubtypeEntry(Subtype);

	public SubtypeEntry Entry => SubtypeFactory.GetSubtypeEntry(Subtype);

	public QudSubtypeModuleData()
	{
	}

	public QudSubtypeModuleData(string subtype)
	{
		Subtype = subtype;
	}
}

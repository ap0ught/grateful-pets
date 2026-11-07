using System;

namespace XRL.CharacterBuilds.Qud;

public class QudGenotypeModuleData : AbstractEmbarkBuilderModuleData
{
	public string Genotype;

	[Obsolete]
	public GenotypeEntry info => GenotypeFactory.RequireGenotypeEntry(Genotype);

	public GenotypeEntry Entry => GenotypeFactory.RequireGenotypeEntry(Genotype);

	public QudGenotypeModuleData(string id)
	{
		Genotype = id;
	}
}

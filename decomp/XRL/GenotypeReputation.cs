using System;

namespace XRL;

[Serializable]
public class GenotypeReputation
{
	public string With = "";

	public int Value;

	public void MergeWith(GenotypeReputation newReputation)
	{
		if (newReputation.Value != -999)
		{
			Value = newReputation.Value;
		}
	}
}

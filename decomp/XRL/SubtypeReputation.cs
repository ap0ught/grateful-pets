using System;

namespace XRL;

[Serializable]
public class SubtypeReputation
{
	public string With = "";

	public int Value;

	public void MergeWith(SubtypeReputation newReputation)
	{
		if (newReputation.Value != -999)
		{
			Value = newReputation.Value;
		}
	}
}

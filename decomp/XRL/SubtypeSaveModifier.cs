using System;

namespace XRL;

[Serializable]
public class SubtypeSaveModifier
{
	public string Vs = "";

	public int Amount;

	public void MergeWith(SubtypeSaveModifier newSaveModifier)
	{
		if (newSaveModifier.Amount != -999)
		{
			Amount = newSaveModifier.Amount;
		}
	}
}

using System;

namespace XRL;

[Serializable]
public class GenotypeSaveModifier
{
	public string Vs = "";

	public int Amount;

	public void MergeWith(GenotypeSaveModifier newSaveModifier)
	{
		if (newSaveModifier.Amount != -999)
		{
			Amount = newSaveModifier.Amount;
		}
	}
}

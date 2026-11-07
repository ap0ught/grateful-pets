using System;

namespace XRL.World.Effects;

[Serializable]
public class EnergyCellChargeLevelEffect : Effect
{
	public int Level = -1;

	public string Details = "";

	public EnergyCellChargeLevelEffect()
	{
		Duration = 1;
	}

	public override string GetDetails()
	{
		return Details;
	}
}

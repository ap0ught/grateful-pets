using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PlasmaGeneration : GasGeneration
{
	public PlasmaGeneration()
		: base("Plasma")
	{
	}

	public override string GetReleaseAbilityName()
	{
		return "Release Plasma";
	}
}

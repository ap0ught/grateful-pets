using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ConfusionGasGeneration : GasGeneration
{
	public ConfusionGasGeneration()
		: base("ConfusionGas")
	{
	}

	public override int GetReleaseDuration(int Level)
	{
		return (Level + 2) * 3 / 2;
	}

	public override int GetReleaseCooldown(int Level)
	{
		return 40;
	}

	public override string GetReleaseAbilityName()
	{
		return "Release Confusion Gas";
	}
}

using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FungicideGeneration : GasGeneration
{
	public FungicideGeneration()
		: base("FungicideGas")
	{
	}

	public override int GetReleaseDuration(int Level)
	{
		return (Level + 2) * 2;
	}

	public override int GetReleaseCooldown(int Level)
	{
		return 40;
	}

	public override string GetReleaseAbilityName()
	{
		return "Release Fungicide";
	}
}

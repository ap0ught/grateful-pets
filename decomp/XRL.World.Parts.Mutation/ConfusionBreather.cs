using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class ConfusionBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Confusion Gas";
	}

	public override string GetDescription()
	{
		return "You breathe confusion gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes confusion gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "ConfusionGas80";
	}

	public override string GetBreathName()
	{
		return "confusion gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "B", "b", "y");
	}
}

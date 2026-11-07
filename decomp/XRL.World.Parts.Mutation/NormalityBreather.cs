using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class NormalityBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Normality Gas";
	}

	public override string GetDescription()
	{
		return "You breathe normality gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes normality gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "NormalityGas80";
	}

	public override string GetBreathName()
	{
		return "normality gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "K", "y", "Y");
	}
}

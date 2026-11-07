using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class SleepBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Sleep Gas";
	}

	public override string GetDescription()
	{
		return "You breathe sleep gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes sleep gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override string GetGasBlueprint()
	{
		return "SleepGas80";
	}

	public override string GetBreathName()
	{
		return "sleep gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "w", "Y", "y");
	}
}

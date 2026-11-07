using System;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class CorrosiveBreather : BreatherBase
{
	public override string GetCommandDisplayName()
	{
		return "Breathe Corrosive Gas";
	}

	public override string GetDescription()
	{
		return "You breathe corrosive gas.";
	}

	public override string GetLevelText(int Level)
	{
		return string.Concat(string.Concat("Breathes corrosive gas in a cone.\n" + "Cone length: " + GetConeLength() + " tiles\n", "Cone angle: ", GetConeAngle().ToString(), " degrees\n"), "Cooldown: 15 rounds\n");
	}

	public override bool ChangeLevel(int NewLevel)
	{
		StatShifter.SetStatShift("AcidResistance", Level * 2);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public override string GetGasBlueprint()
	{
		return "AcidGas80";
	}

	public override string GetBreathName()
	{
		return "acid gas";
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		DrawBreathInCell(C, Buffer, "G", "g", "w");
	}
}

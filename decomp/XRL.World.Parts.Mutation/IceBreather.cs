using System;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class IceBreather : BreatherBase
{
	public override string GetFaceObject()
	{
		return "Icy Vapor";
	}

	public override string GetCommandDisplayName()
	{
		return "Breathe Ice";
	}

	public override string GetDescription()
	{
		return "You breathe ice.";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		base.CollectStats(stats, Level);
		stats.Set("Damage", ComputeDamage(Level));
	}

	public override string GetBreathName()
	{
		return "ice";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Breathes ice in a cone.\n";
		text = text + "Damage: " + ComputeDamage(Level) + "\n";
		text = text + "Cone length: " + GetConeLength() + " tiles\n";
		text = text + "Cone angle: " + GetConeAngle() + " degrees\n";
		text += "Cooldown: 15 rounds\n";
		if (Level != base.Level)
		{
			text += "Decreased temperature.";
		}
		return text;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		StatShifter.SetStatShift("ColdResistance", Level * 2);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public string ComputeDamage(int UseLevel)
	{
		string text = UseLevel + "d4";
		if (ParentObject != null)
		{
			int partCount = ParentObject.Body.GetPartCount(BodyPartType);
			if (partCount > 0)
			{
				text += partCount.Signed();
			}
		}
		else
		{
			text += "+1";
		}
		return text;
	}

	public string ComputeDamage()
	{
		return ComputeDamage(Level);
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (item.PhaseMatches(ParentObject))
				{
					item.TemperatureChange(-120 - 7 * Level, ParentObject);
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&C" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&c" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&Y" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					item2.TakeDamage(dice.RollCached(), "from %t freezing effect!", "Cold", null, null, null, ParentObject);
				}
			}
		}
		DrawBreathInCell(C, Buffer, "C", "B", "Y");
	}
}

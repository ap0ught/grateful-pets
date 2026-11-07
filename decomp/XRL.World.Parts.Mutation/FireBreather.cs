using System;
using ConsoleLib.Console;
using XRL.Rules;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class FireBreather : BreatherBase
{
	public override string GetFaceObject()
	{
		return "Ghostly Flames";
	}

	public override string GetCommandDisplayName()
	{
		return "Breathe Fire";
	}

	public override string GetDescription()
	{
		return "You breathe fire.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "Breathes fire in a cone.\n";
		text = text + "Damage: " + ComputeDamage(Level) + "\n";
		text = text + "Cone length: " + GetConeLength() + " tiles\n";
		text = text + "Cone angle: " + GetConeAngle() + " degrees\n";
		text += "Cooldown: 15 rounds\n";
		if (Level != base.Level)
		{
			text += "{{rules|Increased temperature}}";
		}
		return text;
	}

	public override string GetBreathName()
	{
		return "fire";
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

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		base.CollectStats(stats, Level);
		stats.Set("Damage", ComputeDamage(Level));
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		StatShifter.SetStatShift("HeatResistance", Level * 2);
		return base.ChangeLevel(NewLevel);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}

	public override void BreatheInCell(Cell C, ScreenBuffer Buffer, bool doEffect = true)
	{
		string dice = ComputeDamage();
		if (C != null)
		{
			foreach (GameObject item in C.GetObjectsInCell())
			{
				if (!item.PhaseMatches(ParentObject))
				{
					continue;
				}
				item.TemperatureChange(310 + 25 * Level, ParentObject);
				if (doEffect)
				{
					for (int i = 0; i < 5; i++)
					{
						item.ParticleText("&r" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int j = 0; j < 5; j++)
					{
						item.ParticleText("&R" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
					for (int k = 0; k < 5; k++)
					{
						item.ParticleText("&W" + (char)(219 + Stat.Random(0, 4)), 2.9f, 1);
					}
				}
			}
			foreach (GameObject item2 in C.GetObjectsWithPart("Combat"))
			{
				if (item2.PhaseMatches(ParentObject))
				{
					Damage damage = new Damage(Stat.Roll(dice));
					damage.AddAttribute("Fire");
					damage.AddAttribute("Heat");
					Event obj = Event.New("TakeDamage");
					obj.AddParameter("Damage", damage);
					obj.AddParameter("Owner", ParentObject);
					obj.AddParameter("Attacker", ParentObject);
					obj.AddParameter("Message", "from %t flames!");
					item2.FireEvent(obj);
				}
			}
		}
		if (doEffect)
		{
			C.Flameburst(Buffer);
		}
	}
}

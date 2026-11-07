using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Capabilities;

public static class Mental
{
	public delegate bool Attack(MentalAttackEvent E);

	public const int TYPE_PSIONIC = 1;

	public const int TYPE_SONIC = 2;

	public const int TYPE_OPTIC = 4;

	public const int TYPE_METABOLIC = 8;

	public const int TYPE_REFLECTABLE = 8388608;

	public const int TYPE_REFLECTED = 16777216;

	public static readonly Dictionary<string, int> StringToType = new Dictionary<string, int>
	{
		{ "PSIONIC", 1 },
		{ "SONIC", 2 },
		{ "OPTIC", 4 },
		{ "METABOLIC", 8 },
		{ "REFLECTABLE", 8388608 },
		{ "REFLECTED", 16777216 }
	};

	public static bool PerformAttack(Attack Handler, GameObject Attacker, GameObject Defender, GameObject Source = null, string Command = null, string Dice = null, int Type = 0, int Magnitude = int.MinValue, int Penetrations = int.MinValue, int AttackModifier = 0, int DefenseModifier = 0)
	{
		if (Source == null)
		{
			Source = Attacker;
		}
		if (!Defender.HasStat("MA"))
		{
			return false;
		}
		MentalAttackEvent mentalAttackEvent = MentalAttackEvent.FromPool(Attacker, Defender, Source, Command, Dice, Type, Magnitude);
		mentalAttackEvent.BaseDifficulty = Stats.GetCombatMA(Defender);
		mentalAttackEvent.Difficulty = mentalAttackEvent.BaseDifficulty + DefenseModifier;
		mentalAttackEvent.Modifier = AttackModifier;
		if (!BeginMentalAttackEvent.Check(mentalAttackEvent))
		{
			return false;
		}
		if (!BeginMentalDefendEvent.Check(mentalAttackEvent))
		{
			return false;
		}
		if (Penetrations >= 0)
		{
			mentalAttackEvent.Penetrations = Penetrations;
		}
		else if (mentalAttackEvent.Dice != null)
		{
			mentalAttackEvent.Penetrations = Stat.RollPenetratingSuccesses(mentalAttackEvent.Dice, mentalAttackEvent.Difficulty, mentalAttackEvent.Modifier);
		}
		else
		{
			mentalAttackEvent.Penetrations = Stat.RollDamagePenetrations(mentalAttackEvent.Difficulty, mentalAttackEvent.Modifier, mentalAttackEvent.Modifier);
		}
		if (!BeforeMentalAttackEvent.Check(mentalAttackEvent))
		{
			mentalAttackEvent.Penetrations = 0;
		}
		if (!BeforeMentalDefendEvent.Check(mentalAttackEvent))
		{
			mentalAttackEvent.Penetrations = 0;
		}
		if (!Handler(mentalAttackEvent))
		{
			mentalAttackEvent.Penetrations = 0;
		}
		AfterMentalAttackEvent.Send(mentalAttackEvent);
		AfterMentalDefendEvent.Send(mentalAttackEvent);
		return mentalAttackEvent.Penetrations > 0;
	}

	public static int TypeExpansion(string Types)
	{
		int num = 0;
		if (!string.IsNullOrEmpty(Types))
		{
			foreach (string item in Types.ToUpper().CachedCommaExpansion())
			{
				if (StringToType.TryGetValue(item, out var value))
				{
					num |= value;
				}
			}
		}
		return num;
	}
}

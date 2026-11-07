using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Genkit;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.World;

namespace XRL.Rules;

public static class Stat
{
	public static string rndAccessLog = null;

	public static int rndAccessIndex = 0;

	public static int rndAccessCount = 0;

	public static bool _logRndAccess = false;

	public static List<string> rndLogLines = new List<string>();

	public static Random Rand = new Random();

	public static Random _Rnd = new Random();

	public static Random Rnd2 = new Random();

	public static Random Rnd4 = new Random();

	public static Random Rnd5 = new Random();

	public static Random NamingRnd = new Random();

	private static Stack<(Random[], int)> State = new Stack<(Random[], int)>();

	[NonSerialized]
	public static char[] splitterD = new char[1] { 'd' };

	[NonSerialized]
	public static char[] splitterX = new char[1] { 'x' };

	[NonSerialized]
	public static char[] splitterSlash = new char[1] { '/' };

	[NonSerialized]
	public static char[] splitterMinus = new char[1] { '-' };

	[NonSerialized]
	public static char[] splitterPlus = new char[1] { '+' };

	[NonSerialized]
	public static Dictionary<string, DieRoll> DieRolls = null;

	private static Regex channelPattern = new Regex("\\[(\\w+)\\]");

	public static Random Rnd
	{
		get
		{
			return _Rnd;
		}
		set
		{
			_Rnd = value;
		}
	}

	public static bool logRndAccess
	{
		get
		{
			rndAccessCount = 0;
			return _logRndAccess;
		}
		set
		{
			if (value)
			{
				rndAccessLog = "c:\\logs\\rnglog" + rndAccessIndex + ".txt";
				File.WriteAllText(rndAccessLog, "start\n");
				rndAccessIndex++;
				rndLogLines.Clear();
			}
			else
			{
				File.WriteAllLines(rndAccessLog, rndLogLines.ToArray());
			}
			_logRndAccess = value;
			rndLogLines.Add("RND ACCESS COUNT:" + _logRndAccess + Environment.StackTrace.ToString() + "\n");
		}
	}

	/// <summary>This effectively pulls off a single use System.Random from RandomSeed3, then sets the next seed
	/// meerly getting this property will mutate it!</summary>
	public static Random LevelUpRandom => WithLevelUpRandom((Random rng) => rng);

	public static void ReseedFrom(string Seed, bool includeLifetimeSeeds = false)
	{
		if (XRLCore.Core != null && XRLCore.Core.Game != null)
		{
			ReseedFrom(XRLCore.Core.Game.GetWorldSeed(Seed), includeLifetimeSeeds);
		}
	}

	public static void ReseedFrom(int Seed, bool includeLifetimeSeeds = false)
	{
		Rand = new Random(Hash.String("Seed0" + Seed));
		Rnd = new Random(Hash.String("Seed1" + Seed));
		Rnd2 = new Random(Hash.String("Seed2" + Seed));
		if (includeLifetimeSeeds)
		{
			SeedLevelUpRandom(new Random(Hash.String("Seed3" + Seed)).Next());
			NamingRnd = new Random(Hash.String("Seed6" + Seed));
		}
		Rnd4 = new Random(Hash.String("Seed4" + Seed));
		Rnd5 = new Random(Hash.String("Seed5" + Seed));
		Calc.Reseed(Hash.String("CalcSeed" + Seed));
	}

	public static void PushState(string Seed, bool IncludeLifetime = false)
	{
		State.Push((new Random[7]
		{
			Rand,
			Rnd,
			Rnd2,
			IncludeLifetime ? NamingRnd : null,
			Rnd4,
			Rnd5,
			Calc.R
		}, IncludeLifetime ? The.Game.GetIntGameState("RandomSeed3") : int.MinValue));
		ReseedFrom(Seed, IncludeLifetime);
	}

	public static void PopState()
	{
		if (!State.IsReadOnlyNullOrEmpty())
		{
			(Random[], int) tuple = State.Pop();
			Random[] item = tuple.Item1;
			int item2 = tuple.Item2;
			Rand = item[0];
			Rnd = item[1];
			Rnd2 = item[2];
			if (item2 != int.MinValue)
			{
				The.Game.SetIntGameState("RandomSeed3", item2);
			}
			NamingRnd = item[3] ?? NamingRnd;
			Rnd4 = item[4];
			Rnd5 = item[5];
			Calc.R = item[6];
		}
	}

	public static int RollDamagePenetrations(int TargetInclusive, int Bonus, int MaxBonus)
	{
		int num = 0;
		int num2 = 3;
		bool debugDamagePenetrations = Options.DebugDamagePenetrations;
		while (num2 == 3)
		{
			num2 = 0;
			for (int i = 0; i < 3; i++)
			{
				int num3 = Random(1, 10) - 2;
				int num4 = 0;
				while (num3 == 8)
				{
					num4 += 8;
					num3 = Random(1, 10) - 2;
				}
				num4 += num3;
				int num5 = num4 + Math.Min(Bonus, MaxBonus);
				if (num5 > TargetInclusive)
				{
					if (debugDamagePenetrations)
					{
						MessageQueue.AddPlayerMessage("Penned with Roll:" + num4 + " Final:" + num5);
					}
					num2++;
				}
				else if (debugDamagePenetrations)
				{
					MessageQueue.AddPlayerMessage("Didn't pen with " + num4 + " Final:" + num5);
				}
			}
			if (debugDamagePenetrations)
			{
				MessageQueue.AddPlayerMessage("{{K|Penning Bonus: " + Bonus + " Max: " + MaxBonus + " Used: " + Math.Min(Bonus, MaxBonus) + " Target: " + TargetInclusive + "(Penned " + num2 + " times)}}");
			}
			if (num2 >= 1)
			{
				num++;
			}
			Bonus -= 2;
		}
		return num;
	}

	public static int RandomLevelUpChoice(int Low, int High)
	{
		return WithLevelUpRandom((Random rng) => rng.Next(Low, High + 1));
	}

	public static T WithLevelUpRandom<T>(Func<Random, T> proc)
	{
		Random random = null;
		if (XRLCore.Core.Game != null)
		{
			random = new Random(XRLCore.Core.Game.GetIntGameState("RandomSeed3"));
		}
		T result = proc(random);
		if (XRLCore.Core.Game != null)
		{
			XRLCore.Core.Game.SetIntGameState("RandomSeed3", random.Next());
		}
		return result;
	}

	private static void SeedLevelUpRandom(int seed)
	{
		if (XRLCore.Core.Game != null)
		{
			XRLCore.Core.Game.SetIntGameState("RandomSeed3", seed);
		}
	}

	public static int TinkerRandom(int Low, int High)
	{
		return Rnd4.Next(Low, High + 1);
	}

	public static int Random5(int Low, int High)
	{
		return Rnd5.Next(Low, High + 1);
	}

	public static int NamingRandom(int Low, int High)
	{
		return NamingRnd.Next(Low, High + 1);
	}

	public static int RandomCosmetic(int Low, int High)
	{
		return Rnd2.Next(Low, High + 1);
	}

	public static Random GetSeededRandomGenerator(string Seed)
	{
		if (XRLCore.Core.Game == null)
		{
			return new Random(Hash.String(Seed));
		}
		return new Random(Hash.String(XRLCore.Core.Game.GetWorldSeed() + Seed));
	}

	public static Random GetStableRandomGenerator(string Seed)
	{
		return new Random((int)Seed.GetStableHashCode32());
	}

	public static int SeededRandom(string Seed, int Low, int High)
	{
		if (Seed == null)
		{
			Seed = "none";
		}
		return new Random(Hash.String(XRLCore.Core.Game.GetWorldSeed() + Seed)).Next(Low, High + 1);
	}

	public static double GaussianRandom(float Mean, float StandardDeviation)
	{
		double d = Rnd.NextDouble();
		double num = Rnd.NextDouble();
		double num2 = Math.Sqrt(-2.0 * Math.Log(d)) * Math.Sin(Math.PI * 2.0 * num);
		return (double)Mean + (double)StandardDeviation * num2;
	}

	public static int Random(int Low, int High)
	{
		return Rnd.Next(Low, High + 1);
	}

	public static int Random(long Low, long High)
	{
		return Rnd.Next((int)Low, (int)High + 1);
	}

	public static int Random(int Low, long High)
	{
		return Rnd.Next(Low, (int)High + 1);
	}

	public static int Random(long Low, int High)
	{
		return Rnd.Next((int)Low, High + 1);
	}

	public static float Random(float Low, float High)
	{
		return (float)Rnd.Next((int)(Low * 10f), (int)(High * 10f) + 1) / 10f;
	}

	public static ulong NextULong(ulong Low, ulong High)
	{
		if (Low >= High)
		{
			return Low;
		}
		Span<byte> span = stackalloc byte[8];
		ulong num = 0uL;
		ulong num2 = High - Low;
		ulong num3 = ulong.MaxValue - (ulong.MaxValue % num2 + 1) % num2;
		do
		{
			Rnd.NextBytes(span);
			num = BitConverter.ToUInt64(span);
		}
		while (num > num3);
		return num % num2 + Low;
	}

	public static void RollSave(out int NaturalRoll, out int Roll, out int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool LogRoll = false, GameObject Source = null)
	{
		NaturalRoll = Random(1, 20);
		Roll = NaturalRoll;
		Difficulty = BaseDifficulty;
		Roll += Defender.StatMod(Stat);
		if (Attacker != null)
		{
			Difficulty += Attacker.StatMod(AttackerStat ?? Stat);
			ModifyAttackingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: true);
		}
		if (Source != null)
		{
			ModifyOriginatingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: true);
		}
		ModifyDefendingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: true);
		if (Defender.IsPlayer())
		{
			switch (Stat)
			{
			case "Intelligence":
			case "Ego":
			case "Willpower":
				Defender.PlayWorldSound("sfx_ability_mutation_mental_generic_save");
				break;
			default:
				Defender.PlayWorldSound("sfx_ability_mutation_physical_generic_save");
				break;
			}
		}
		if (!LogRoll || !Options.DebugSavingThrows)
		{
			return;
		}
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append(Defender.IsPlayer() ? "Player" : Defender.Blueprint).Append(" rolled ").Append(NaturalRoll);
		if (Roll != NaturalRoll)
		{
			stringBuilder.Append(" modified to ").Append(Roll);
		}
		stringBuilder.Append(" on ").Append(Stat).Append(" save");
		if (Vs != null)
		{
			stringBuilder.Append(" vs. ").Append(Vs);
		}
		if (Attacker != null)
		{
			stringBuilder.Append(" from ").Append(Attacker.IsPlayer() ? "player" : Attacker.Blueprint);
			if (AttackerStat != null && AttackerStat != Stat)
			{
				stringBuilder.Append(" (using ").Append(AttackerStat).Append(')');
			}
		}
		stringBuilder.Append(" with difficulty ").Append(BaseDifficulty);
		if (Difficulty != BaseDifficulty)
		{
			stringBuilder.Append(" modified to ").Append(Difficulty);
		}
		MessageQueue.AddPlayerMessage(stringBuilder.ToString());
	}

	public static void RollSave(out int NaturalRoll, out int Roll, out int Difficulty, GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool LogRoll = false)
	{
		bool IgnoreNatural = false;
		bool IgnoreNatural2 = false;
		RollSave(out NaturalRoll, out Roll, out Difficulty, ref IgnoreNatural, ref IgnoreNatural2, Defender, Stat, BaseDifficulty, Attacker, AttackerStat, Vs, LogRoll);
	}

	public static void RollSave(out int Roll, out int Difficulty, GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool LogRoll = false)
	{
		RollSave(out var _, out Roll, out Difficulty, Defender, Stat, BaseDifficulty, Attacker, AttackerStat, Vs, LogRoll);
	}

	public static int RollSave(GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool LogRoll = false)
	{
		RollSave(out var _, out var Roll, out var _, Defender, Stat, BaseDifficulty, Attacker, AttackerStat, Vs, LogRoll);
		return Roll;
	}

	public static bool MakeSave(out int SuccessMargin, out int FailureMargin, GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false, GameObject Source = null)
	{
		if (IgnoreNaturals)
		{
			IgnoreNatural1 = true;
			IgnoreNatural20 = true;
		}
		SuccessMargin = 0;
		FailureMargin = 0;
		RollSave(out var NaturalRoll, out var Roll, out var Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Defender, Stat, BaseDifficulty, Attacker, AttackerStat, Vs, LogRoll: false, Source);
		bool flag = (Defender.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode) || (NaturalRoll == 20 && !IgnoreNatural20) || ((NaturalRoll != 1 || IgnoreNatural1) && Roll >= Difficulty);
		if (flag)
		{
			if (Roll > Difficulty)
			{
				SuccessMargin = Roll - Difficulty;
			}
		}
		else if (Roll < Difficulty)
		{
			FailureMargin = Difficulty - Roll;
		}
		if (Options.DebugSavingThrows)
		{
			StringBuilder stringBuilder = Event.NewStringBuilder();
			stringBuilder.Append(Defender.DebugName).Append(flag ? " made " : " failed ").Append(Stat)
				.Append(" save");
			if (Vs != null)
			{
				stringBuilder.Append(" vs. ").Append(Vs);
			}
			if (Attacker != null)
			{
				stringBuilder.Append(" from ").Append(Attacker.DebugName);
				if (AttackerStat != null && AttackerStat != Stat)
				{
					stringBuilder.Append(" (using ").Append(AttackerStat).Append(')');
				}
			}
			if (Source != null)
			{
				stringBuilder.Append(" via ").Append(Source.DebugName);
			}
			stringBuilder.Append(" on ");
			if ((NaturalRoll == 1 && !IgnoreNatural1) || (NaturalRoll == 20 && !IgnoreNatural20))
			{
				stringBuilder.Append("natural ");
			}
			stringBuilder.Append(NaturalRoll);
			if (Roll != NaturalRoll)
			{
				stringBuilder.Append(" modified to ").Append(Roll);
			}
			stringBuilder.Append(" with difficulty ").Append(BaseDifficulty);
			if (Difficulty != BaseDifficulty)
			{
				stringBuilder.Append(" modified to ").Append(Difficulty);
			}
			if (Defender.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode)
			{
				stringBuilder.Append(" (godmode)");
			}
			MessageQueue.AddPlayerMessage(stringBuilder.ToString());
		}
		return flag;
	}

	public static bool MakeSave(GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false)
	{
		int SuccessMargin;
		int FailureMargin;
		return MakeSave(out SuccessMargin, out FailureMargin, Defender, Stat, BaseDifficulty, Attacker, AttackerStat, Vs, IgnoreNaturals, IgnoreNatural1, IgnoreNatural20, IgnoreGodmode);
	}

	public static int SaveChance(GameObject Defender, string Stat, int BaseDifficulty, GameObject Attacker = null, string AttackerStat = null, string Vs = null, bool IgnoreNaturals = false, bool IgnoreNatural1 = false, bool IgnoreNatural20 = false, bool IgnoreGodmode = false, GameObject Source = null)
	{
		if (IgnoreNaturals)
		{
			IgnoreNatural1 = true;
			IgnoreNatural20 = true;
		}
		int Difficulty = BaseDifficulty;
		int Roll = Defender.StatMod(Stat);
		if (Attacker != null)
		{
			Difficulty += Attacker.StatMod(AttackerStat ?? Stat);
			ModifyAttackingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, 0, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: false);
		}
		if (Source != null)
		{
			ModifyOriginatingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, 0, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: false);
		}
		ModifyDefendingSaveEvent.Process(Attacker, Defender, Source, Stat, AttackerStat, Vs, 0, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual: false);
		if (Defender.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode)
		{
			return 100;
		}
		int num = 21 - Difficulty + Roll;
		int num2 = ((!IgnoreNatural1) ? 1 : 0);
		int num3 = (IgnoreNatural20 ? 20 : 19);
		if (num < num2)
		{
			num = num2;
		}
		else if (num > num3)
		{
			num = num3;
		}
		return num * 5;
	}

	public static char GetResultColorChar(int Result)
	{
		if (Result == -1)
		{
			return 'K';
		}
		if (Result == 0)
		{
			return 'r';
		}
		if (Result == 1)
		{
			return 'w';
		}
		if (Result == 2)
		{
			return 'W';
		}
		if (Result == 3)
		{
			return 'r';
		}
		if (Result == 4)
		{
			return 'R';
		}
		if (Result >= 5)
		{
			return 'M';
		}
		return 'y';
	}

	public static string GetResultColor(int Result)
	{
		if (Result == -1)
		{
			return "&K";
		}
		if (Result == 0)
		{
			return "&r";
		}
		if (Result == 1)
		{
			return "&w";
		}
		if (Result == 2)
		{
			return "&W";
		}
		if (Result == 3)
		{
			return "&r";
		}
		if (Result == 4)
		{
			return "&R";
		}
		if (Result >= 5)
		{
			return "&M";
		}
		return "&y";
	}

	public static char GetChanceColorChar(int Chance)
	{
		if (Chance >= 100)
		{
			return 'M';
		}
		if (Chance >= 70)
		{
			return 'G';
		}
		if (Chance >= 40)
		{
			return 'W';
		}
		if (Chance >= 1)
		{
			return 'R';
		}
		return 'K';
	}

	public static string GetChanceColor(int Chance)
	{
		if (Chance >= 100)
		{
			return "&M";
		}
		if (Chance >= 70)
		{
			return "&G";
		}
		if (Chance >= 40)
		{
			return "&W";
		}
		if (Chance >= 1)
		{
			return "&R";
		}
		return "&K";
	}

	public static int RollResult(int Rating)
	{
		int num = Rnd.Next(1, 101);
		int num2 = Math.Min(Math.Max(Rating, -5), 35);
		if (num == 1)
		{
			return -1;
		}
		if ((double)num >= 100.0 - 0.26 * (double)num2)
		{
			return 10;
		}
		if ((double)num >= 99.0 - 0.27 * (double)num2)
		{
			return 9;
		}
		if (num >= 89 - num2)
		{
			return 8;
		}
		if ((double)num >= 86.0 - 1.31 * (double)num2)
		{
			return 7;
		}
		if ((double)num >= 78.0 - 1.74 * (double)num2)
		{
			return 6;
		}
		if ((double)num >= 74.0 - 1.92 * (double)num2)
		{
			return 5;
		}
		if ((double)num >= 67.0 - 3.1 * (double)num2)
		{
			return 4;
		}
		if ((double)num >= 55.0 - 1.5 * (double)num2)
		{
			return 3;
		}
		if ((double)num >= 44.0 - 1.4 * (double)num2)
		{
			return 2;
		}
		if ((double)num >= 15.0 - 0.5 * (double)num2)
		{
			return 1;
		}
		return 0;
	}

	public static int MixRandom(int Low, int High)
	{
		if (High < Low)
		{
			return Rand.Next(High, Low + 1);
		}
		return Rand.Next(Low, High + 1);
	}

	public static int Roll(int Low, int High)
	{
		return Rand.Next(Low, High + 1);
	}

	public static int Roll(DieRoll Roll)
	{
		return Roll.Resolve();
	}

	public static bool Chance(string chance)
	{
		if (chance.Contains(":"))
		{
			string[] array = chance.Split(':');
			return Random(1, Convert.ToInt32(array[1])) <= Convert.ToInt32(array[0]);
		}
		return Convert.ToInt32(chance).in100();
	}

	public static bool Chance(int chance)
	{
		return chance.in100();
	}

	/// <summary>
	///     Convert an attribute score to a roll modifier. 
	///     An attribute of 0 will have a -8 roll modifier, and will get +1 for every 2 points of attribute.
	///     Floor((Score - 16) * 0.5) 
	/// </summary>
	/// <param name="Score">The raw attribute score</param>
	/// <returns>A modifier to be used for a dice roll.</returns>
	public static int GetScoreModifier(int Score)
	{
		return (int)Math.Floor((double)(Score - 16) * 0.5);
	}

	public static int RollLevelupChoice(string Dice)
	{
		int num = 1;
		if (Dice.Length == 0)
		{
			return 0;
		}
		if (Dice[0] == '-')
		{
			num = -1;
			Dice = Dice.Substring(1);
		}
		if (!Dice.Contains("d"))
		{
			if (Dice.Contains("-"))
			{
				string[] array = Dice.Split('-');
				return RandomLevelUpChoice(Convert.ToInt32(array[0]), Convert.ToInt32(array[1])) * num;
			}
			return Convert.ToInt32(Dice) * num;
		}
		string[] array2 = Dice.Split('d');
		int num2 = 0;
		int num3 = 0;
		if (array2[1].Contains("+"))
		{
			string[] array3 = array2[1].Split('+');
			num2 = Convert.ToInt32(array3[0]);
			num3 = Convert.ToInt32(array3[1]);
		}
		else if (array2[1].Contains("-"))
		{
			string[] array4 = array2[1].Split('-');
			num2 = Convert.ToInt32(array4[0]);
			num3 = -Convert.ToInt32(array4[1]);
		}
		else
		{
			num2 = Convert.ToInt32(array2[1]);
		}
		int num4 = 0;
		int i = 0;
		for (int num5 = Convert.ToInt32(array2[0]); i < num5; i++)
		{
			num4 += RandomLevelUpChoice(1, num2);
		}
		return num * (num4 + num3);
	}

	public static int Roll(string Dice, string Channel = null)
	{
		if (string.IsNullOrEmpty(Dice))
		{
			return 0;
		}
		TryExtractChannel(ref Dice, ref Channel);
		if (Dice[0] == '-')
		{
			return -Roll(Dice.Substring(1), Channel);
		}
		if (Dice.Contains("|"))
		{
			string[] array = Dice.Split('|');
			int num = GlobalChannelRandom(Channel, 0, array.Length - 1);
			return Roll(array[num], Channel);
		}
		if (Dice.Contains("+"))
		{
			string[] array2 = Dice.Split(splitterPlus, 2);
			return Roll(array2[0], Channel) + Roll(array2[1], Channel);
		}
		if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array3 = Dice.Split(splitterMinus, 2);
			return Roll(array3[0], Channel) - Roll(array3[1], Channel);
		}
		if (Dice.Contains("x"))
		{
			string[] array4 = Dice.Split(splitterX, 2);
			return Roll(array4[0], Channel) * Roll(array4[1], Channel);
		}
		if (Dice.Contains("/"))
		{
			string[] array5 = Dice.Split(splitterSlash, 2);
			return Roll(array5[0], Channel) / Roll(array5[1], Channel);
		}
		if (Dice.Contains("d"))
		{
			string[] array6 = Dice.Split(splitterD, 2);
			int num2 = Convert.ToInt32(array6[0]);
			int high = Convert.ToInt32(array6[1]);
			int num3 = 0;
			for (int i = 0; i < num2; i++)
			{
				num3 += GlobalChannelRandom(Channel, 1, high);
			}
			return num3;
		}
		if (Dice.Contains("-"))
		{
			string[] array7 = Dice.Split(splitterMinus, 2);
			return GlobalChannelRandom(Channel, Roll(array7[0], Channel), Roll(array7[1], Channel));
		}
		return Convert.ToInt32(Dice);
	}

	public static int Roll(this Random R, string Dice)
	{
		if (string.IsNullOrEmpty(Dice))
		{
			return 0;
		}
		if (Dice[0] == '-')
		{
			return -R.Roll(Dice.Substring(1));
		}
		if (Dice.Contains("|"))
		{
			string[] array = Dice.Split('|');
			return R.Roll(array[R.Next(array.Length)]);
		}
		if (Dice.Contains("+"))
		{
			string[] array2 = Dice.Split(splitterPlus, 2);
			return R.Roll(array2[0]) + R.Roll(array2[1]);
		}
		if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array3 = Dice.Split(splitterMinus, 2);
			return R.Roll(array3[0]) - R.Roll(array3[1]);
		}
		if (Dice.Contains("x"))
		{
			string[] array4 = Dice.Split(splitterX, 2);
			return R.Roll(array4[0]) * R.Roll(array4[1]);
		}
		if (Dice.Contains("/"))
		{
			string[] array5 = Dice.Split(splitterSlash, 2);
			return R.Roll(array5[0]) / R.Roll(array5[1]);
		}
		if (Dice.Contains("d"))
		{
			string[] array6 = Dice.Split(splitterD, 2);
			int num = Convert.ToInt32(array6[0]);
			int num2 = Convert.ToInt32(array6[1]);
			int num3 = 0;
			for (int i = 0; i < num; i++)
			{
				num3 += R.Next(1, num2 + 1);
			}
			return num3;
		}
		if (Dice.Contains("-"))
		{
			string[] array7 = Dice.Split(splitterMinus, 2);
			return R.Next(R.Roll(array7[0]), R.Roll(array7[1]) + 1);
		}
		return Convert.ToInt32(Dice);
	}

	public static int RollMax(string Dice)
	{
		if (string.IsNullOrEmpty(Dice))
		{
			return 0;
		}
		TryExtractChannel(ref Dice);
		if (Dice[0] == '-')
		{
			return -RollMin(Dice.Substring(1));
		}
		if (Dice.Contains("|"))
		{
			int num = int.MinValue;
			bool flag = false;
			string[] array = Dice.Split('|');
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = RollMax(array[i]);
				if (num2 > num)
				{
					num = num2;
					flag = true;
				}
			}
			if (!flag)
			{
				return 0;
			}
			return num;
		}
		if (Dice.Contains("+"))
		{
			string[] array2 = Dice.Split(splitterPlus, 2);
			return RollMax(array2[0]) + RollMax(array2[1]);
		}
		if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array3 = Dice.Split(splitterMinus, 2);
			return RollMax(array3[0]) - RollMin(array3[1]);
		}
		if (Dice.Contains("x"))
		{
			string[] array4 = Dice.Split(splitterX, 2);
			return RollMax(array4[0]) * RollMax(array4[1]);
		}
		if (Dice.Contains("/"))
		{
			string[] array5 = Dice.Split(splitterSlash, 2);
			return RollMax(array5[0]) / RollMax(array5[1]);
		}
		if (Dice.Contains("d"))
		{
			string[] array6 = Dice.Split(splitterD, 2);
			int num3 = Convert.ToInt32(array6[0]);
			int num4 = Convert.ToInt32(array6[1]);
			return num3 * num4;
		}
		if (Dice.Contains("-"))
		{
			return RollMax(Dice.Split(splitterMinus, 2)[1]);
		}
		return Convert.ToInt32(Dice);
	}

	public static int RollMax(DieRoll Roll)
	{
		return Roll.Max();
	}

	public static int RollMin(string Dice)
	{
		if (string.IsNullOrEmpty(Dice))
		{
			return 0;
		}
		TryExtractChannel(ref Dice);
		if (Dice[0] == '-')
		{
			return -RollMax(Dice.Substring(1));
		}
		if (Dice.Contains("|"))
		{
			int num = int.MaxValue;
			bool flag = false;
			string[] array = Dice.Split('|');
			for (int i = 0; i < array.Length; i++)
			{
				int num2 = RollMin(array[i]);
				if (num2 < num)
				{
					num = num2;
					flag = true;
				}
			}
			if (!flag)
			{
				return 0;
			}
			return num;
		}
		if (Dice.Contains("+"))
		{
			string[] array2 = Dice.Split(splitterPlus, 2);
			return RollMin(array2[0]) + RollMin(array2[1]);
		}
		if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array3 = Dice.Split(splitterMinus, 2);
			return RollMin(array3[0]) - RollMax(array3[1]);
		}
		if (Dice.Contains("x"))
		{
			string[] array4 = Dice.Split(splitterX, 2);
			return RollMin(array4[0]) * RollMin(array4[1]);
		}
		if (Dice.Contains("/"))
		{
			string[] array5 = Dice.Split(splitterSlash, 2);
			return RollMin(array5[0]) / RollMin(array5[1]);
		}
		if (Dice.Contains("d"))
		{
			return Convert.ToInt32(Dice.Split(splitterD, 2)[0]);
		}
		if (Dice.Contains("-"))
		{
			return RollMin(Dice.Split(splitterMinus, 2)[0]);
		}
		return Convert.ToInt32(Dice);
	}

	public static int RollMin(DieRoll Roll)
	{
		return Roll.Min();
	}

	public static DieRoll GetCachedDieRoll(string Dice)
	{
		DieRoll value;
		if (DieRolls == null)
		{
			DieRolls = new Dictionary<string, DieRoll>(128);
			value = new DieRoll(Dice);
			DieRolls.Add(Dice, value);
		}
		else if (!DieRolls.TryGetValue(Dice, out value))
		{
			value = new DieRoll(Dice);
			DieRolls.Add(Dice, value);
		}
		return value;
	}

	public static int RollCached(string Dice)
	{
		return GetCachedDieRoll(Dice).Resolve();
	}

	public static int RollMinCached(string Dice)
	{
		return GetCachedDieRoll(Dice).Min();
	}

	public static int RollMaxCached(string Dice)
	{
		return GetCachedDieRoll(Dice).Max();
	}

	public static double RollAverageCached(string Dice)
	{
		return GetCachedDieRoll(Dice).Average();
	}

	public static int rollMentalAttackPenetrations(GameObject attacker, GameObject defender)
	{
		string text = "1d8";
		int num = attacker.StatMod("Ego");
		if (num >= 0)
		{
			text = text + "+" + num;
		}
		else if (num < 0)
		{
			text += num;
		}
		int combatMA = Stats.GetCombatMA(defender);
		return RollPenetratingSuccesses(text, combatMA);
	}

	public static void TryExtractChannel(ref string Dice, ref string Channel)
	{
		Match match = channelPattern.Match(Dice);
		if (match.Success)
		{
			Channel = match.Groups[1].Value;
			Dice = channelPattern.Replace(Dice, "");
		}
	}

	public static void TryExtractChannel(ref string Dice)
	{
		string Channel = null;
		TryExtractChannel(ref Dice, ref Channel);
	}

	public static int RollPenetratingSuccesses(string Dice, int TargetInclusive, int Bonus = 0, int Multiplier = 1, string Channel = null)
	{
		if (string.IsNullOrEmpty(Dice))
		{
			return 0;
		}
		TryExtractChannel(ref Dice, ref Channel);
		if (Dice.Contains("|"))
		{
			string[] array = Dice.Split('|');
			int num = GlobalChannelRandom(Channel, 0, array.Length - 1);
			return RollPenetratingSuccesses(array[num], TargetInclusive, Bonus, Multiplier, Channel);
		}
		if (Dice[0] == '-')
		{
			Multiplier *= -1;
			Dice = Dice.Substring(1);
		}
		if (Dice.Contains("+"))
		{
			string[] array2 = Dice.Split(splitterPlus, 2);
			return RollPenetratingSuccesses(array2[0], TargetInclusive, Bonus + Roll(array2[1], Channel), Multiplier, Channel);
		}
		if (Dice.Contains("d") && Dice.Contains("-"))
		{
			string[] array3 = Dice.Split(splitterMinus, 2);
			return RollPenetratingSuccesses(array3[0], TargetInclusive, Bonus - Roll(array3[1], Channel), Multiplier, Channel);
		}
		if (Dice.Contains("x"))
		{
			string[] array4 = Dice.Split(splitterX, 2);
			return RollPenetratingSuccesses(array4[0], TargetInclusive, Bonus, Multiplier * Roll(array4[1], Channel), Channel);
		}
		if (Dice.Contains("/"))
		{
			string[] array5 = Dice.Split(splitterSlash, 2);
			return RollPenetratingSuccesses(array5[0], TargetInclusive, Bonus, Multiplier / Roll(array5[1], Channel), Channel);
		}
		if (Dice.Contains("d"))
		{
			string[] array6 = Dice.Split(splitterD, 2);
			int num2 = Convert.ToInt32(array6[0]);
			int num3 = Convert.ToInt32(array6[1]);
			int num4 = 0;
			for (int i = 0; i < num2; i++)
			{
				int num5 = 0;
				int num6 = 0;
				bool flag = true;
				do
				{
					num6 = GlobalChannelRandom(Channel, 1, num3);
					num5 += num6;
					if (flag)
					{
						flag = false;
					}
					else
					{
						num5--;
					}
					if (num5 * Multiplier + Bonus >= TargetInclusive)
					{
						num4++;
						break;
					}
				}
				while (num6 == num3 && num6 != 1);
			}
			return num4;
		}
		if (Dice.Contains("-"))
		{
			string[] array7 = Dice.Split(splitterMinus, 2);
			if (GlobalChannelRandom(Channel, Roll(array7[0], Channel), Roll(array7[1], Channel)) * Multiplier + Bonus >= TargetInclusive)
			{
				return 1;
			}
		}
		else if (Roll(Dice, Channel) * Multiplier + Bonus >= TargetInclusive)
		{
			return 1;
		}
		return 0;
	}

	public static int GlobalChannelRandom(string Channel, int Low, int High)
	{
		if (Channel == null)
		{
			return Random(Low, High);
		}
		string text = "RandomSeed:" + Channel;
		int seed = ((!XRLCore.Core.Game.HasIntGameState(text)) ? Hash.String(XRLCore.Core.Game.GetWorldSeed() + text) : XRLCore.Core.Game.GetIntGameState(text));
		Random random = new Random(seed);
		int result = random.Next(Low, High + 1);
		XRLCore.Core.Game.SetIntGameState(text, random.Next());
		return result;
	}
}

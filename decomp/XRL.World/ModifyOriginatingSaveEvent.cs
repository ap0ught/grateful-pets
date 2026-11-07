using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ModifyOriginatingSaveEvent : ISaveEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ModifyOriginatingSaveEvent), null, CountPool, ResetPool);

	private static List<ModifyOriginatingSaveEvent> Pool;

	private static int PoolCounter;

	public ModifyOriginatingSaveEvent()
	{
		base.ID = ID;
	}

	public static int CountPool()
	{
		if (Pool != null)
		{
			return Pool.Count;
		}
		return 0;
	}

	public static void ResetPool()
	{
		while (PoolCounter > 0)
		{
			Pool[--PoolCounter].Reset();
		}
	}

	public static void ResetTo(ref ModifyOriginatingSaveEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ModifyOriginatingSaveEvent FromPool()
	{
		return MinEvent.FromPool(ref Pool, ref PoolCounter);
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		if (!base.Dispatch(Handler))
		{
			return false;
		}
		return Handler.HandleEvent(this);
	}

	public static ModifyOriginatingSaveEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual)
	{
		ModifyOriginatingSaveEvent modifyOriginatingSaveEvent = FromPool();
		modifyOriginatingSaveEvent.Attacker = Attacker;
		modifyOriginatingSaveEvent.Defender = Defender;
		modifyOriginatingSaveEvent.Source = Source;
		modifyOriginatingSaveEvent.Stat = Stat;
		modifyOriginatingSaveEvent.AttackerStat = AttackerStat;
		modifyOriginatingSaveEvent.Vs = Vs;
		modifyOriginatingSaveEvent.NaturalRoll = NaturalRoll;
		modifyOriginatingSaveEvent.Roll = Roll;
		modifyOriginatingSaveEvent.BaseDifficulty = BaseDifficulty;
		modifyOriginatingSaveEvent.Difficulty = Difficulty;
		modifyOriginatingSaveEvent.Actual = Actual;
		return modifyOriginatingSaveEvent;
	}

	public static bool Process(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		return ISaveEvent.Process(Attacker, "ModifyOriginatingSave", ID, ISaveEvent.CascadeLevel, FromPool, Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual);
	}
}

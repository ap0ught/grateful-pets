using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ModifyDefendingSaveEvent : ISaveEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ModifyDefendingSaveEvent), null, CountPool, ResetPool);

	private static List<ModifyDefendingSaveEvent> Pool;

	private static int PoolCounter;

	public ModifyDefendingSaveEvent()
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

	public static void ResetTo(ref ModifyDefendingSaveEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ModifyDefendingSaveEvent FromPool()
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

	public static ModifyDefendingSaveEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual)
	{
		ModifyDefendingSaveEvent modifyDefendingSaveEvent = FromPool();
		modifyDefendingSaveEvent.Attacker = Attacker;
		modifyDefendingSaveEvent.Defender = Defender;
		modifyDefendingSaveEvent.Source = Source;
		modifyDefendingSaveEvent.Stat = Stat;
		modifyDefendingSaveEvent.AttackerStat = AttackerStat;
		modifyDefendingSaveEvent.Vs = Vs;
		modifyDefendingSaveEvent.NaturalRoll = NaturalRoll;
		modifyDefendingSaveEvent.Roll = Roll;
		modifyDefendingSaveEvent.BaseDifficulty = BaseDifficulty;
		modifyDefendingSaveEvent.Difficulty = Difficulty;
		modifyDefendingSaveEvent.Actual = Actual;
		return modifyDefendingSaveEvent;
	}

	public static bool Process(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		return ISaveEvent.Process(Defender, "ModifyDefendingSave", ID, ISaveEvent.CascadeLevel, FromPool, Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual);
	}
}

using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cache = Cache.Pool)]
public class ModifyAttackingSaveEvent : ISaveEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(ModifyAttackingSaveEvent), null, CountPool, ResetPool);

	private static List<ModifyAttackingSaveEvent> Pool;

	private static int PoolCounter;

	public ModifyAttackingSaveEvent()
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

	public static void ResetTo(ref ModifyAttackingSaveEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static ModifyAttackingSaveEvent FromPool()
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

	public static ModifyAttackingSaveEvent FromPool(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, int Roll, int BaseDifficulty, int Difficulty, bool IgnoreNatural1, bool IgnoreNatural20, bool Actual)
	{
		ModifyAttackingSaveEvent modifyAttackingSaveEvent = FromPool();
		modifyAttackingSaveEvent.Attacker = Attacker;
		modifyAttackingSaveEvent.Defender = Defender;
		modifyAttackingSaveEvent.Source = Source;
		modifyAttackingSaveEvent.Stat = Stat;
		modifyAttackingSaveEvent.AttackerStat = AttackerStat;
		modifyAttackingSaveEvent.Vs = Vs;
		modifyAttackingSaveEvent.NaturalRoll = NaturalRoll;
		modifyAttackingSaveEvent.Roll = Roll;
		modifyAttackingSaveEvent.BaseDifficulty = BaseDifficulty;
		modifyAttackingSaveEvent.Difficulty = Difficulty;
		modifyAttackingSaveEvent.Actual = Actual;
		return modifyAttackingSaveEvent;
	}

	public static bool Process(GameObject Attacker, GameObject Defender, GameObject Source, string Stat, string AttackerStat, string Vs, int NaturalRoll, ref int Roll, int BaseDifficulty, ref int Difficulty, ref bool IgnoreNatural1, ref bool IgnoreNatural20, bool Actual)
	{
		return ISaveEvent.Process(Attacker, "ModifyAttackingSave", ID, ISaveEvent.CascadeLevel, FromPool, Attacker, Defender, Source, Stat, AttackerStat, Vs, NaturalRoll, ref Roll, BaseDifficulty, ref Difficulty, ref IgnoreNatural1, ref IgnoreNatural20, Actual);
	}
}

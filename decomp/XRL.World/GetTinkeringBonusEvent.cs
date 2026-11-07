using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Cascade = 3, Cache = Cache.Pool)]
public class GetTinkeringBonusEvent : IActOnItemEvent
{
	public new static readonly int ID = MinEvent.RegisterEvent(typeof(GetTinkeringBonusEvent), null, CountPool, ResetPool);

	public new static readonly int CascadeLevel = 3;

	private static List<GetTinkeringBonusEvent> Pool;

	private static int PoolCounter;

	public string Type;

	public int BaseRating;

	public int Bonus;

	public int SecondaryBonus;

	public int ToolboxBonus;

	public bool PsychometryApplied;

	public bool Interruptable;

	public bool ForSifrah;

	public GetTinkeringBonusEvent()
	{
		base.ID = ID;
	}

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
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

	public static void ResetTo(ref GetTinkeringBonusEvent E)
	{
		MinEvent.ResetTo(E, Pool, ref PoolCounter);
		E = null;
	}

	public static GetTinkeringBonusEvent FromPool()
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

	public override void Reset()
	{
		base.Reset();
		Type = null;
		BaseRating = 0;
		Bonus = 0;
		SecondaryBonus = 0;
		ToolboxBonus = 0;
		PsychometryApplied = false;
		Interruptable = false;
		ForSifrah = false;
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, ref int SecondaryBonus, ref bool Interrupt, ref bool PsychometryApplied, bool Interruptable = true, bool ForSifrah = false)
	{
		if (Actor.WantEvent(ID, CascadeLevel))
		{
			GetTinkeringBonusEvent getTinkeringBonusEvent = FromPool();
			getTinkeringBonusEvent.Actor = Actor;
			getTinkeringBonusEvent.Item = Item;
			getTinkeringBonusEvent.Type = Type;
			getTinkeringBonusEvent.BaseRating = BaseRating;
			getTinkeringBonusEvent.Bonus = Bonus;
			getTinkeringBonusEvent.SecondaryBonus = SecondaryBonus;
			getTinkeringBonusEvent.ToolboxBonus = 0;
			getTinkeringBonusEvent.PsychometryApplied = PsychometryApplied;
			getTinkeringBonusEvent.Interruptable = Interruptable;
			getTinkeringBonusEvent.ForSifrah = ForSifrah;
			if (!Actor.HandleEvent(getTinkeringBonusEvent))
			{
				Interrupt = true;
			}
			Bonus = getTinkeringBonusEvent.Bonus;
			PsychometryApplied = getTinkeringBonusEvent.PsychometryApplied;
			SecondaryBonus = getTinkeringBonusEvent.SecondaryBonus;
		}
		return Bonus;
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, ref bool Interrupt, ref bool PsychometryApplied, bool Interruptable = true, bool ForSifrah = false)
	{
		int SecondaryBonus = 0;
		return GetFor(Actor, Item, Type, BaseRating, Bonus, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable, ForSifrah);
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, ref bool Interrupt, bool Interruptable = true, bool ForSifrah = false)
	{
		int SecondaryBonus = 0;
		bool PsychometryApplied = false;
		return GetFor(Actor, Item, Type, BaseRating, Bonus, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable, ForSifrah);
	}

	public static int GetFor(GameObject Actor, GameObject Item, string Type, int BaseRating, int Bonus, bool Interruptable = true, bool ForSifrah = false)
	{
		int SecondaryBonus = 0;
		bool Interrupt = false;
		bool PsychometryApplied = false;
		return GetFor(Actor, Item, Type, BaseRating, Bonus, ref SecondaryBonus, ref Interrupt, ref PsychometryApplied, Interruptable, ForSifrah);
	}
}

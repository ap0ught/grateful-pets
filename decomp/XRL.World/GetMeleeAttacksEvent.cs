using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetMeleeAttacksEvent : PooledEvent<GetMeleeAttacksEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public List<MeleeAttack> Attacks = new List<MeleeAttack>();

	public string Properties;

	public int ChanceModifier;

	public double ChanceMultiplier = 1.0;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Attacks.Clear();
		Properties = null;
		ChanceModifier = 0;
		ChanceMultiplier = 1.0;
	}

	public bool AddAttack(int Chance = 0, int HitModifier = 0, int PenModifier = 0, object Source = null, string Type = null, string Properties = null, BodyPart BodyPart = null, Predicate<BodyPart> Filter = null, bool? Primary = null, bool Distinct = true, bool Replace = true)
	{
		if (Distinct && Source != null)
		{
			for (int i = 0; i < Attacks.Count; i++)
			{
				if (Attacks[i].Source == Source)
				{
					if (!Replace || Chance <= Attacks[i].Chance)
					{
						return false;
					}
					RemoveAttackAt(i);
					break;
				}
			}
		}
		MeleeAttack item = MeleeAttack.Get(Chance, HitModifier, PenModifier, Source, Type, Properties, null, BodyPart, Filter, Intrinsic: false, Primary);
		if (BodyPart != null)
		{
			Attacks.Insert(Attacks.FindLastIndex((MeleeAttack x) => x.BodyPart != null) + 1, item);
		}
		else if (Filter != null)
		{
			Attacks.Insert(Attacks.FindLastIndex((MeleeAttack x) => x.Filter != null || x.BodyPart != null) + 1, item);
		}
		else
		{
			Attacks.Add(item);
		}
		return true;
	}

	public bool RemoveAttack(MeleeAttack Attack)
	{
		if (Attacks.Remove(Attack))
		{
			MeleeAttack.Return(Attack);
			return true;
		}
		return false;
	}

	public void RemoveAttackAt(int Index)
	{
		MeleeAttack.Return(Attacks[Index]);
		Attacks.RemoveAt(Index);
	}

	public static GetMeleeAttacksEvent HandleFrom(GameObject Actor, string Properties = null, int ChanceModifier = 0, double ChanceMultiplier = 1.0)
	{
		GetMeleeAttacksEvent getMeleeAttacksEvent = PooledEvent<GetMeleeAttacksEvent>.FromPool();
		getMeleeAttacksEvent.Properties = Properties;
		getMeleeAttacksEvent.ChanceModifier = ChanceModifier;
		getMeleeAttacksEvent.ChanceMultiplier = ChanceMultiplier;
		Actor.HandleEvent(getMeleeAttacksEvent);
		return getMeleeAttacksEvent;
	}

	public static MeleeAttack GetBestFor(GameObject Actor, BodyPart BodyPart = null, string Properties = null, int ChanceModifier = 0, double ChanceMultiplier = 1.0)
	{
		GetMeleeAttacksEvent E = HandleFrom(Actor, Properties, ChanceModifier, ChanceMultiplier);
		int num = 0;
		int num2 = -1;
		for (int num3 = E.Attacks.Count - 1; num3 >= 0; num3--)
		{
			if (E.Attacks[num3].Chance > num && (BodyPart == null || E.Attacks[num3].IsValidFor(BodyPart)))
			{
				num = E.Attacks[num3].Chance;
				num2 = num3;
			}
		}
		MeleeAttack meleeAttack = ((num2 >= 0) ? E.Attacks[num2] : new MeleeAttack());
		meleeAttack.Chance += E.ChanceModifier;
		PooledEvent<GetMeleeAttacksEvent>.ResetTo(ref E);
		return meleeAttack;
	}
}

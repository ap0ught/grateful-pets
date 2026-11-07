using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IMeleeAttackEvent : MinEvent
{
	public GameObject Actor;

	public List<MeleeAttack> Attacks = new List<MeleeAttack>();

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Attacks.Clear();
	}

	public bool AddAttack(int Chance = 0, int HitModifier = 0, int PenModifier = 0, object Source = null, string Type = null, string Properties = null, GameObject Weapon = null, BodyPart BodyPart = null, Predicate<BodyPart> Filter = null, bool? Primary = null, bool Instrinsic = false, bool Distinct = true, bool Replace = true)
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

	public bool HasAttackByWeapon(GameObject Weapon)
	{
		int i = 0;
		for (int count = Attacks.Count; i < count; i++)
		{
			if (Attacks[i].Weapon == Weapon)
			{
				return true;
			}
		}
		return false;
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
}

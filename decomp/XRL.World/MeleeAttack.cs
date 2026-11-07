using System;
using System.Collections.Generic;
using XRL.World.Anatomy;

namespace XRL.World;

public class MeleeAttack : IDisposable
{
	/// <summary>The chance that this melee attack will be attempted.</summary>
	public int Chance;

	/// <summary>A to-hit modifier applied if the attack is attempted.</summary>
	public int HitModifier;

	/// <summary>A penetration modifier applied if the attack is attempted.</summary>
	public int PenModifier;

	/// <summary>The object or part that added this attack.</summary>
	public object Source;

	/// <summary>Comma delimited identifiers.</summary>
	public string Type;

	/// <summary>Comma delimited properties added to the attack.</summary>
	public string Properties;

	/// <summary>The weapon this attack is using.</summary>
	public GameObject Weapon;

	/// <summary>The body part this weapon is attack from.</summary>
	public BodyPart BodyPart;

	/// <summary>Filter applicable body parts for this attack.</summary>
	public Predicate<BodyPart> Filter;

	/// <summary>Whether this attack is a basic natural attack of the weapon, not added via skills or mutations.</summary>
	public bool Intrinsic;

	/// <summary>Whether this attack can be (or is being) executed from a primary (true), secondary (false), or any limb (null).</summary>
	public bool? Primary;

	private static Stack<MeleeAttack> Pool = new Stack<MeleeAttack>();

	public bool IsValidFor(BodyPart BodyPart)
	{
		if (this.BodyPart != null && BodyPart != this.BodyPart)
		{
			return false;
		}
		if (Filter != null && !Filter(BodyPart))
		{
			return false;
		}
		return true;
	}

	public void Reset()
	{
		Chance = 0;
		HitModifier = 0;
		PenModifier = 0;
		Type = null;
		Properties = null;
		BodyPart = null;
		Source = null;
		Filter = null;
		Primary = null;
	}

	private static MeleeAttack GetInternal()
	{
		if (Pool.Count <= 0)
		{
			return new MeleeAttack();
		}
		return Pool.Pop();
	}

	public static MeleeAttack Get(int Chance = 0, int HitModifier = 0, int PenModifier = 0, object Source = null, string Type = null, string Properties = null, GameObject Weapon = null, BodyPart BodyPart = null, Predicate<BodyPart> Filter = null, bool Intrinsic = false, bool? Primary = null)
	{
		MeleeAttack meleeAttack = GetInternal();
		meleeAttack.Chance = Chance;
		meleeAttack.HitModifier = HitModifier;
		meleeAttack.PenModifier = PenModifier;
		meleeAttack.Source = Source;
		meleeAttack.Type = Type;
		meleeAttack.Properties = Properties;
		meleeAttack.BodyPart = BodyPart;
		meleeAttack.Filter = Filter;
		meleeAttack.Primary = Primary;
		return meleeAttack;
	}

	public static void Return(MeleeAttack Attack)
	{
		Attack.Reset();
		Pool.Push(Attack);
	}

	public void Dispose()
	{
		Return(this);
	}
}

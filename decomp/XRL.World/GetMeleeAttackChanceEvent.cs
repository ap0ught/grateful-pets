using System;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetMeleeAttackChanceEvent : PooledEvent<GetMeleeAttackChanceEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Actor;

	public GameObject Weapon;

	public BodyPart BodyPart;

	public string Properties;

	public int BaseChance;

	public int Chance;

	public int Attempts = 1;

	public double Multiplier = 1.0;

	public bool Primary;

	public bool Intrinsic;

	public bool Inherited;

	public bool Consecutive;

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
		Weapon = null;
		BodyPart = null;
		Properties = null;
		BaseChance = 0;
		Chance = 0;
		Attempts = 1;
		Multiplier = 1.0;
		Primary = false;
		Intrinsic = false;
		Inherited = false;
		Consecutive = false;
	}

	public void HandleFor(GameObject Actor, GameObject Weapon, int BaseChance = -1, int Modifier = 0, double Multiplier = 1.0, string Properties = null, BodyPart BodyPart = null, BodyPart PrimaryBodyPart = null, bool Primary = false, bool Intrinsic = false, bool Consecutive = false)
	{
		Reset();
		this.Actor = Actor;
		this.Weapon = Weapon;
		this.BodyPart = BodyPart ?? (BodyPart = Weapon.EquippedOn());
		this.BaseChance = BaseChance;
		this.Multiplier = Multiplier;
		this.Properties = Properties;
		this.Primary = Primary;
		this.Intrinsic = Intrinsic;
		this.Consecutive = Consecutive;
		if (BodyPart.DefaultPrimary && !BodyPart.Primary)
		{
			if (PrimaryBodyPart == null)
			{
				Actor.Body.GetMainWeapon(out var _, out PrimaryBodyPart, null, NeedPrimary: true, FailDownFromPrimary: true);
			}
			if (PrimaryBodyPart != null && PrimaryBodyPart != BodyPart && PrimaryBodyPart.Primary)
			{
				this.BodyPart = PrimaryBodyPart;
				Inherited = true;
			}
		}
		if (BaseChance == -1)
		{
			this.BaseChance = (Primary ? 100 : Stats.GetSecondaryAttackChance(Actor));
		}
		Chance = this.BaseChance + Modifier;
		Actor.HandleEvent(this);
	}

	public static GetMeleeAttackChanceEvent HandleFrom(GameObject Actor, GameObject Weapon, int BaseChance = -1, int Modifier = 0, double Multiplier = 1.0, string Properties = null, BodyPart BodyPart = null, BodyPart PrimaryBodyPart = null, bool Primary = false, bool Intrinsic = false, bool Consecutive = false)
	{
		GetMeleeAttackChanceEvent getMeleeAttackChanceEvent = PooledEvent<GetMeleeAttackChanceEvent>.FromPool();
		getMeleeAttackChanceEvent.HandleFor(Actor, Weapon, BaseChance, Modifier, Multiplier, Properties, BodyPart, PrimaryBodyPart, Primary, Intrinsic, Consecutive);
		return getMeleeAttackChanceEvent;
	}

	public void SetFinalizedChance(int Chance)
	{
		this.Chance = Chance;
		Multiplier = 1.0;
	}

	public int GetFinalizedChance()
	{
		return (int)Math.Round((double)Chance * Multiplier);
	}

	public static int GetFor(GameObject Actor, GameObject Weapon, int BaseChance = -1, int Modifier = 0, double Multiplier = 1.0, string Properties = null, BodyPart BodyPart = null, BodyPart PrimaryBodyPart = null, bool Primary = false, bool Intrinsic = false, bool Consecutive = false)
	{
		GetMeleeAttackChanceEvent E = PooledEvent<GetMeleeAttackChanceEvent>.FromPool();
		E.HandleFor(Actor, Weapon, BaseChance, Modifier, Multiplier, Properties, BodyPart, PrimaryBodyPart, Primary, Intrinsic, Consecutive);
		int finalizedChance = E.GetFinalizedChance();
		PooledEvent<GetMeleeAttackChanceEvent>.ResetTo(ref E);
		return finalizedChance;
	}
}

using System;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class ModFatecaller : IModification
{
	public int Chance = 50;

	public ModFatecaller()
	{
	}

	public ModFatecaller(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "ProbabilityEngine";
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModFatecaller).Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!ParentObject.HasTag("Creature"))
		{
			int num = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModFatecaller Activation", Chance);
			E.Postfix.AppendRules(num + "% of the time, the Fates have their way.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("chance", 10);
			E.Add("might", 2);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		Registrar.Register("AttackerAfterDamage");
		Registrar.Register("DealingMissileDamage");
		Registrar.Register("WeaponMissileWeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" || E.ID == "AttackerAfterDamage" || E.ID == "DealingMissileDamage" || E.ID == "WeaponMissileWeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject gameObjectParameter3 = E.GetGameObjectParameter("Projectile");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			GameObject projectile = gameObjectParameter3;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModFatecaller Activation", Chance, subject, projectile).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				gameObjectParameter2.Rainbowsplatter();
				gameObjectParameter2.PlayWorldSound("sfx_characterTrigger_fatesHaveTheirWay");
				int num = Stat.Random(1, 41);
				if (num <= 5)
				{
					Axe_Dismember.Dismember(gameObjectParameter, gameObjectParameter2);
				}
				else if (num <= 10)
				{
					Axe_Decapitate.Decapitate(gameObjectParameter, gameObjectParameter2);
				}
				else if (num <= 15)
				{
					gameObjectParameter2.ApplyEffect(new Asleep(Stat.Random(1, 100)));
				}
				else if (num <= 20)
				{
					gameObjectParameter2.ApplyEffect(new Confused(Stat.Random(1, 100), Stat.Random(1, 20), Stat.Random(1, 20)));
				}
				else if (num <= 25)
				{
					gameObjectParameter2.ApplyEffect(new Stun(Stat.Random(1, 100), Stat.Random(1, 20)));
				}
				else if (num <= 30)
				{
					gameObjectParameter2.ApplyEffect(new Shaken(Stat.Random(1, 100), Stat.Random(1, 20)));
				}
				else if (num <= 35)
				{
					projectile = gameObjectParameter2;
					gameObjectParameter.Discharge(null, Stat.Random(1, 100), 0, "1d" + Stat.Random(1, 10), null, gameObjectParameter, ParentObject, projectile);
				}
				else if (num <= 40)
				{
					gameObjectParameter2.ApplyEffect(new Bleeding("1d" + Stat.Random(1, 10), Stat.Random(1, 100), gameObjectParameter));
				}
				else
				{
					gameObjectParameter2.CurrentCell.AddObject("Space-Time Vortex");
				}
			}
		}
		return base.FireEvent(E);
	}
}

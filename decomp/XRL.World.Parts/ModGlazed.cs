using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class ModGlazed : IModification
{
	public int Chance;

	public ModGlazed()
	{
	}

	public ModGlazed(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void TierConfigure()
	{
		Chance = 10 + Tier * 2;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<MeleeWeapon>();
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModGlazed).Chance != Chance)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!ParentObject.IsCreature)
		{
			int num = GetSpecialEffectChanceEvent.GetFor(ParentObject.Equipped ?? ParentObject.Implantee, ParentObject, "Modification ModGlazed Dismember", Chance);
			E.Postfix.AppendRules(num + "% chance to dismember on hit.");
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
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModGlazed Dismember", Chance, subject, projectile).in100() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				bool flag = 2.in1000();
				Axe_Dismember.Dismember(gameObjectParameter, gameObjectParameter2, null, null, ParentObject, null, "sfx_characterTrigger_dismember", flag, !flag);
			}
		}
		return base.FireEvent(E);
	}
}

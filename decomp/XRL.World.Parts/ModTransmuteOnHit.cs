using System;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ModTransmuteOnHit : IModification
{
	public const string MSG_TRANSMUTE_OBJECT = "=subject.T= =verb:were= transmuted into =object.an=.";

	public int ChancePerThousand = 1;

	public string Table = "Gemstones";

	public string DescriptionTerm = "a gemstone";

	public bool Animate;

	public ModTransmuteOnHit()
	{
	}

	public ModTransmuteOnHit(int Tier)
		: base(Tier)
	{
	}

	public ModTransmuteOnHit(int ChancePerThousand, string Table)
		: this()
	{
		this.ChancePerThousand = ChancePerThousand;
		this.Table = Table;
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MeleeWeapon>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification()
	{
		IncreaseDifficultyAndComplexity(1, 1);
	}

	public override bool SameAs(IPart p)
	{
		ModTransmuteOnHit modTransmuteOnHit = p as ModTransmuteOnHit;
		if (modTransmuteOnHit.ChancePerThousand != ChancePerThousand)
		{
			return false;
		}
		if (modTransmuteOnHit.Table != Table)
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
		if (!ParentObject.HasTag("Creature"))
		{
			E.Postfix.Append("\n{{rules|");
			if (ChancePerThousand < 10)
			{
				E.Postfix.Append("Small");
			}
			else
			{
				E.Postfix.Append(ChancePerThousand / 10).Append('%');
			}
			E.Postfix.Append(" chance to transmute an enemy into ").Append(DescriptionTerm).Append(" on hit.")
				.Append("}}");
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
			if (gameObjectParameter != null && gameObjectParameter2 != null && gameObjectParameter2.IsHostileTowards(gameObjectParameter))
			{
				GameObject parentObject = ParentObject;
				GameObject subject = gameObjectParameter2;
				if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Modification ModTransmuteOnHit Activation", ChancePerThousand, subject, null, ConstrainToPercentage: false, ConstrainToPermillage: true).in1000() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && gameObjectParameter2.GetCurrentCell() != null)
				{
					if (!Options.UseParticleVFX)
					{
						gameObjectParameter2.Splatter("&B!");
						gameObjectParameter2.Splatter("&b?");
					}
					Transmutation.TransmuteObject(gameObjectParameter2, gameObjectParameter, E.GetGameObjectParameter("Weapon"), null, PopulationManager.RollOneFrom(Table).Blueprint, "=subject.T= =verb:were= transmuted into =object.an=.", "Transmute", "sfx_transmute_gemstone", Animate);
				}
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using System.Collections.Generic;
using XRL.World.Effects;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, save targets to resist the effect are
/// increased by the standard power load bonus, i.e. 2 for the standard
/// overload power load of 400.
/// </remarks>
[Serializable]
public class ModMorphogenetic : IModification
{
	public const int DEFAULT_SAVE_BASE_DIFFICULTY = 10;

	public const float DEFAULT_SAVE_DAMAGE_DIFFICULTY_FACTOR = 0.2f;

	public const string DEFAULT_SAVE_ATTRIBUTE = "Willpower";

	public const string DEFAULT_SAVE_VS = "MorphicShock Daze";

	public const string DEFAULT_DAZE_DURATION = "2-3";

	public int SaveBaseDifficulty = 10;

	public float SaveDamageDifficultyFactor = 0.2f;

	public string SaveAttribute = "Willpower";

	public string SaveVs = "MorphicShock Daze";

	public string DazeDuration = "2-3";

	[NonSerialized]
	private Event eCanApplyMorphicShock = new Event("CanApplyMorphicShock");

	[NonSerialized]
	private Event eApplyMorphicShock = new Event("ApplyMorphicShock");

	public ModMorphogenetic()
	{
	}

	public ModMorphogenetic(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		ChargeUse = 200;
		WorksOnSelf = true;
		IsEMPSensitive = true;
		IsBootSensitive = true;
		IsPowerLoadSensitive = true;
		IsTechScannable = true;
		NameForStatus = "MorphogeneticResonator";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (!Object.HasPart<MeleeWeapon>() && !Object.HasPart<MissileWeapon>() && !Object.HasPart<ThrownWeapon>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(2, 2);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{m|morphogenetic}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription(), GetEventSensitiveAddStatusSummary(E));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("circuitry", 2);
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("LauncherProjectileHit");
		Registrar.Register("WeaponDealDamage");
		Registrar.Register("WeaponPseudoThrowHit");
		Registrar.Register("WeaponThrowHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponDealDamage" || E.ID == "LauncherProjectileHit" || E.ID == "WeaponThrowHit" || E.ID == "WeaponPseudoThrowHit")
		{
			int num = E.GetParameter<Damage>()?.Amount ?? E.GetIntParameter("Damage");
			if (num > 0)
			{
				GameObject defender = E.GetGameObjectParameter("Defender");
				if (defender != null && defender.Brain != null)
				{
					string species = defender.GetSpecies();
					Cell cell = defender.CurrentCell;
					if (!species.IsNullOrEmpty() && cell?.ParentZone != null && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
					{
						GameObject attacker = E.GetGameObjectParameter("Attacker");
						Event obj = Event.New("InitiateRealityDistortionLocal");
						obj.SetParameter("Object", attacker);
						obj.SetParameter("Device", ParentObject);
						obj.SetParameter("Operator", attacker);
						obj.SetParameter("Cell", cell);
						if (defender.FireEvent(obj, E))
						{
							List<GameObject> list = Event.NewGameObjectList();
							cell.ParentZone.FindObjects(list, (GameObject gameObject) => gameObject != defender && MorphicShockMatch(gameObject, species));
							if (list.Count > 0)
							{
								int num2 = MyPowerLoadLevel();
								int? powerLoadLevel = num2;
								if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
								{
									if (list.Count > 1)
									{
										list.Sort((GameObject a, GameObject b) => a.DistanceTo(attacker).CompareTo(b.DistanceTo(attacker)));
									}
									int num3 = 0;
									for (int count = list.Count; num3 < count; num3++)
									{
										GameObject Object = list[num3];
										if (GameObject.Validate(ref Object))
										{
											ApplyMorphicShock(Object, num, attacker, num2);
										}
									}
								}
							}
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Morphogenetic: When powered and used to perform a successful, damaging hit, this weapon attempts to daze all other creatures of the same species as your target on the local map. Compute power on the local lattice increases the strength of this effect.";
	}

	public string GetInstanceDescription()
	{
		return GetDescription(Tier);
	}

	public bool ApplyMorphicShock(GameObject Subject, int Damage, GameObject Owner, int PowerLoad = 100)
	{
		if (Subject.Brain == null)
		{
			return false;
		}
		if (!IComponent<GameObject>.CheckRealityDistortionAccessibility(Subject, null, Owner, ParentObject))
		{
			return false;
		}
		int num = SaveBaseDifficulty + IComponent<GameObject>.PowerLoadBonus(PowerLoad) + (int)((float)Damage * SaveDamageDifficultyFactor) + GetAvailableComputePowerEvent.GetFor(Owner) / 3;
		if (Subject.HasPart<Analgesia>())
		{
			num -= 5;
		}
		if (num <= 0)
		{
			return false;
		}
		eCanApplyMorphicShock.SetParameter("Attacker", Owner);
		eCanApplyMorphicShock.SetParameter("Defender", Subject);
		eCanApplyMorphicShock.SetParameter("Difficulty", num);
		if (!Subject.FireEvent(eCanApplyMorphicShock))
		{
			return false;
		}
		if (Subject.MakeSave(SaveAttribute, num, null, null, SaveVs, IgnoreNaturals: false, IgnoreNatural1: false, IgnoreNatural20: false, IgnoreGodmode: false, ParentObject))
		{
			return false;
		}
		eApplyMorphicShock.SetParameter("Attacker", Owner);
		eApplyMorphicShock.SetParameter("Defender", Subject);
		eApplyMorphicShock.SetParameter("Difficulty", num);
		if (!Subject.FireEvent(eApplyMorphicShock))
		{
			return false;
		}
		if (Subject.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("A weird" + (Subject.HasPart<Analgesia>() ? "" : ", painful") + " shock reverberates through you.");
		}
		if (!Subject.ApplyEffect(new Dazed(DazeDuration.RollCached())))
		{
			return false;
		}
		Subject.TelekinesisBlip();
		return true;
	}

	public static bool MorphicShockMatch(GameObject Subject, string Species)
	{
		if (Subject.Brain == null)
		{
			return false;
		}
		if (Species == "*")
		{
			return true;
		}
		string species = Subject.GetSpecies();
		if (species == "*")
		{
			return true;
		}
		return species == Species;
	}
}

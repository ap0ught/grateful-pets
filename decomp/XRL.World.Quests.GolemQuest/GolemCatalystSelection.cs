using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using XRL.Liquids;
using XRL.Wish;
using XRL.World.Capabilities;
using XRL.World.Parts;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemCatalystSelection : GolemMaterialSelection<string, string>
{
	public const string BASE_ID = "Catalyst";

	public const int DRAMS = 3;

	public string Liquid;

	[NonSerialized]
	private GameObject IconVolume;

	[NonSerialized]
	private List<string> _ValidMaterials = new List<string>(8);

	public override string Material
	{
		get
		{
			return Liquid;
		}
		set
		{
			Liquid = value;
		}
	}

	public override string ID => "Catalyst";

	public override string DisplayName => "catalyst";

	public override char Key => 'c';

	public override bool IsValid(string Material)
	{
		if (base.IsValid(Material) && GolemMaterialSelection<string, string>.Units.ContainsKey(Material))
		{
			return GetValidHolders().Any((GameObject x) => x.GetFreeDrams(Material) >= 3);
		}
		return false;
	}

	public override string GetNameFor(string Material)
	{
		return string.Format("{0} {1} of {2}", 3, "drams", LiquidVolume.GetLiquid(Material)?.Name ?? Material);
	}

	public override IRenderable GetIconFor(string Material)
	{
		if (IconVolume == null)
		{
			IconVolume = GameObjectFactory.Factory.CreateSampleObject("DeepFreshWaterPool");
		}
		LiquidVolume liquidVolume = IconVolume.LiquidVolume;
		liquidVolume.ComponentLiquids.Clear();
		liquidVolume.ComponentLiquids[Material] = 1000;
		liquidVolume.LastPaintMask = -1;
		liquidVolume.RecalculatePrimary();
		liquidVolume.Paint(0);
		return new Renderable(IconVolume.Render);
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(string Material)
	{
		if (!GolemMaterialSelection<string, string>.Units.TryGetValue(Material, out var value))
		{
			yield break;
		}
		using IEnumerator<GameObjectUnit> numr = value(Material).GetEnumerator();
		if (!numr.MoveNext())
		{
			yield break;
		}
		GameObjectUnit current = numr.Current;
		if (!numr.MoveNext())
		{
			yield return current;
			yield break;
		}
		GameObjectUnitAggregate gameObjectUnitAggregate = new GameObjectUnitAggregate();
		gameObjectUnitAggregate.Units.Add(current);
		gameObjectUnitAggregate.Units.Add(numr.Current);
		while (numr.MoveNext())
		{
			gameObjectUnitAggregate.Units.Add(numr.Current);
		}
		yield return gameObjectUnitAggregate;
	}

	public override List<string> GetValidMaterials()
	{
		_ValidMaterials.Clear();
		List<GameObject> validHolders = GetValidHolders();
		GetFreeDramsEvent getFreeDramsEvent = GetFreeDramsEvent.FromPool();
		foreach (KeyValuePair<string, UnitGenerator> unit in GolemMaterialSelection<string, string>.Units)
		{
			foreach (GameObject item in validHolders)
			{
				GameObject gameObject = (getFreeDramsEvent.Actor = item);
				getFreeDramsEvent.Drams = 0;
				getFreeDramsEvent.Liquid = unit.Key;
				gameObject.HandleEvent(getFreeDramsEvent);
				if (getFreeDramsEvent.Drams >= 3)
				{
					_ValidMaterials.Add(unit.Key);
					break;
				}
			}
		}
		return _ValidMaterials;
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		if (!LiquidVolume.isValidLiquid(Liquid))
		{
			return;
		}
		foreach (GameObject validHolder in GetValidHolders())
		{
			if (validHolder.GetFreeDrams(Liquid) >= 3)
			{
				validHolder.UseDrams(3, Liquid);
				break;
			}
		}
		Object.SetStringProperty("CatalystLiquid", Liquid);
		Object.SetStringProperty("BleedLiquid", "proteangunk-990," + Liquid + "-10");
	}

	[WishCommand("golemquest:catalyst", null)]
	private static void Wish()
	{
		ReceiveMaterials(The.Player);
	}

	public static void ReceiveMaterials(GameObject Object)
	{
		foreach (KeyValuePair<string, BaseLiquid> pair in LiquidVolume.Liquids)
		{
			Object.ReceiveObject(GameObjectFactory.Factory.CreateObject("Phial", (GameObject x) =>
			{
				LiquidVolume liquidVolume = x.LiquidVolume;
				liquidVolume.InitialLiquid = pair.Key + "-1000";
				liquidVolume.MaxVolume = 3;
				liquidVolume.StartVolume = "3";
			}));
		}
	}

	static GolemCatalystSelection()
	{
		GolemMaterialSelection<string, string>.Units = new Dictionary<string, UnitGenerator>
		{
			{ "water", UnitWater },
			{ "salt", UnitSalt },
			{ "asphalt", UnitAsphalt },
			{ "lava", UnitLava },
			{ "slime", UnitSlime },
			{ "oil", UnitOil },
			{ "blood", UnitBlood },
			{ "acid", UnitAcid },
			{ "honey", UnitHoney },
			{ "wine", UnitWine },
			{ "sludge", UnitSludge },
			{ "goo", UnitGoo },
			{ "putrid", UnitPutrescence },
			{ "gel", UnitGel },
			{ "ooze", UnitOoze },
			{ "cider", UnitCider },
			{ "convalessence", UnitConvalessence },
			{ "neutronflux", UnitNeutronFlux },
			{ "cloning", UnitCloning },
			{ "wax", UnitWax },
			{ "ink", UnitInk },
			{ "sap", UnitSap },
			{ "brainbrine", UnitBrainBrine },
			{ "algae", UnitAlgae },
			{ "sunslag", UnitSunSlag },
			{ "warmstatic", UnitWarmStatic }
		};
	}

	private static IEnumerable<GameObjectUnit> UnitWater(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Hitpoints",
			Value = 30,
			Percent = true
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSalt(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Willpower",
			Value = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitAsphalt(string Liquid)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Stuck",
			Value = SavingThrows.IMMUNITY
		};
		yield return new GameObjectPartUnit
		{
			Part = new StickOnHit
			{
				Chance = 100
			},
			Description = "Melee attacks cause enemies to get stuck"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitLava(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "HeatResistance",
			Value = 200
		};
		yield return new GameObjectPartUnit
		{
			Part = new AttackerElementalAmplifier
			{
				Attribute = "Heat",
				Amplification = 60
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSlime(string Liquid)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Slip",
			Value = SavingThrows.IMMUNITY
		};
		yield return new GameObjectPartUnit
		{
			Part = new ProneOnHit()
		};
	}

	private static IEnumerable<GameObjectUnit> UnitOil(string Liquid)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Slip",
			Value = SavingThrows.IMMUNITY
		};
		yield return new GameObjectPartUnit
		{
			Part = new AttackerElementalAmplifier
			{
				Attribute = "Heat",
				Amplification = 60
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitBlood(string Liquid)
	{
		yield return new GameObjectPartUnit
		{
			Part = new VampiricWeapon
			{
				Percent = "10",
				WorksThrown = false,
				WorksAsProjectile = false,
				WorksInMelee = false,
				WorksAsAttacker = true,
				RequiresLivingTarget = true
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitAcid(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "AcidResistance",
			Value = 200
		};
		yield return new GameObjectPartUnit
		{
			Part = new AttackerElementalAmplifier
			{
				Attribute = "Acid",
				Amplification = 75
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitHoney(string Liquid)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Regeneration",
			Level = 5
		};
		yield return new GameObjectPartUnit
		{
			Part = new StickOnHit
			{
				Chance = 100
			},
			Description = "Melee attacks cause enemies to get stuck"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitWine(string Liquid)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Confusion",
			Level = 10
		};
		yield return new GameObjectPartUnit
		{
			Part = new EffectResistance("Confusion"),
			Description = "Immune to confusion"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSludge(string Liquid)
	{
		yield return new GameObjectPartUnit
		{
			Part = new Galvanized
			{
				Inventory = false,
				Equipment = true
			}
		};
		yield return new GameObjectPartUnit
		{
			Part = new RustOnHit
			{
				Chance = 100,
				Amount = "2-4",
				PreferPartType = "Hand",
				AffectInventory = false
			},
			Description = "Melee attacks cause 2-4 equipped enemy items to rust, starting with held items"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitGoo(string Liquid)
	{
		yield return new GameObjectPartUnit
		{
			Part = new EffectResistance("Poison", "PoisonGasPoison"),
			Description = "Immune to poison"
		};
		yield return new GameObjectPartUnit
		{
			Part = new AttackerElementalAmplifier
			{
				Attribute = "Poison",
				Amplification = 100
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitPutrescence(string Liquid)
	{
		yield return new GameObjectPartUnit
		{
			Part = new SapOnPenetration
			{
				Chance = 100,
				Stat = "Toughness",
				Amount = "1",
				NaturalOnly = false
			},
			Description = "Melee attacks drain 1 Toughness"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitGel(string Liquid)
	{
		yield return new GameObjectUnitAggregate("+50 to all resists", new GameObjectAttributeUnit
		{
			Attribute = "HeatResistance",
			Value = 50
		}, new GameObjectAttributeUnit
		{
			Attribute = "ColdResistance",
			Value = 50
		}, new GameObjectAttributeUnit
		{
			Attribute = "ElectricResistance",
			Value = 50
		}, new GameObjectAttributeUnit
		{
			Attribute = "AcidResistance",
			Value = 50
		});
		yield return new GameObjectAttributeUnit
		{
			Attribute = "AV",
			Value = 3
		};
	}

	private static IEnumerable<GameObjectUnit> UnitOoze(string Liquid)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Disease",
			Value = SavingThrows.IMMUNITY
		};
		yield return new GameObjectMutationUnit
		{
			Name = "Corrosive Gas Generation",
			Level = 5
		};
	}

	private static IEnumerable<GameObjectUnit> UnitCider(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Speed",
			Value = 10
		};
		yield return new GameObjectMutationUnit
		{
			Name = "Confusion",
			Level = 5
		};
	}

	private static IEnumerable<GameObjectUnit> UnitConvalessence(string Liquid)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Regeneration",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitNeutronFlux(string Liquid)
	{
		yield return new GameObjectPartUnit
		{
			Part = new DetonateOnHit
			{
				Chance = 5,
				Force = 1200,
				Damage = "1d6"
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitCloning(string Liquid)
	{
		yield return new GameObjectUnitAggregate("Two golems, each with one-third HP", new GameObjectAttributeUnit
		{
			Attribute = "Hitpoints",
			Value = -66,
			Percent = true
		}, new GameObjectCloneUnit());
	}

	private static IEnumerable<GameObjectUnit> UnitWax(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "HeatResistance",
			Value = 50
		};
		yield return new GameObjectPartUnit
		{
			Part = new AttackerElementalAmplifier
			{
				Attribute = "Heat",
				Amplification = 25
			}
		};
		yield return new GameObjectPartUnit
		{
			Part = new StickOnHit
			{
				Chance = 100
			},
			Description = "Melee attacks cause enemies to get stuck"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitInk(string Liquid)
	{
		yield return new GameObjectSaveModifierUnit
		{
			Vs = "Slip",
			Value = SavingThrows.IMMUNITY
		};
		yield return new GameObjectAttributeUnit
		{
			Attribute = "SP",
			Value = 500
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSap(string Liquid)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Burgeoning",
			Level = 5
		};
		yield return new GameObjectPartUnit
		{
			Part = new StickOnHit
			{
				Chance = 100
			},
			Description = "Melee attacks cause enemies to get stuck"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitBrainBrine(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "MP",
			Value = 6
		};
	}

	private static IEnumerable<GameObjectUnit> UnitAlgae(string Liquid)
	{
		yield return new GameObjectMutationUnit
		{
			Name = "Burgeoning",
			Level = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSunSlag(string Liquid)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Speed",
			Value = 15
		};
	}

	private static IEnumerable<GameObjectUnit> UnitWarmStatic(string Liquid)
	{
		yield return new GameObjectGolemQuestRandomUnit
		{
			SelectionID = "Catalyst",
			Amount = 2
		};
	}
}

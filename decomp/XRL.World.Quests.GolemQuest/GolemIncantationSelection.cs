using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using Genkit;
using Qud.API;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
public class GolemIncantationSelection : GolemMaterialSelection<JournalAccomplishment, MuralCategory>
{
	public const string BASE_ID = "Incantation";

	public JournalAccomplishment _Material;

	public override JournalAccomplishment Material
	{
		get
		{
			return _Material;
		}
		set
		{
			_Material = value;
		}
	}

	public override string ID => "Incantation";

	public override string DisplayName => "incantation";

	public override char Key => 'i';

	public override bool IsValid(JournalAccomplishment Accomplishment)
	{
		if (base.IsValid(Accomplishment) && !Accomplishment.Text.IsNullOrEmpty())
		{
			return Accomplishment.MuralCategory != MuralCategory.Generic;
		}
		return false;
	}

	public override List<JournalAccomplishment> GetValidMaterials()
	{
		return JournalAPI.Accomplishments.Where(IsValid).ToList();
	}

	public override string GetNameFor(JournalAccomplishment Material)
	{
		return Material.Text;
	}

	public override IRenderable GetIconFor(JournalAccomplishment Material)
	{
		Random random = new Random((int)Hash.FNV1A32(Material.Text));
		Renderable renderable = new Renderable();
		renderable.ColorString = "&Y";
		Renderable renderable2 = renderable;
		renderable2.Tile = random.Next(4) switch
		{
			0 => "items/sw_unfurled_scroll1.bmp", 
			1 => "items/sw_unfurled_scroll2.bmp", 
			2 => "items/sw_scroll1.bmp", 
			_ => "items/sw_scroll2.bmp", 
		};
		renderable.TileColor = "&Y";
		do
		{
			renderable.DetailColor = Crayons.GetRandomColorAll(random)[0];
		}
		while (renderable.DetailColor == renderable.TileColor[1]);
		return renderable;
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(JournalAccomplishment Material)
	{
		if (!GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units.TryGetValue(Material.MuralCategory, out var value))
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

	[WishCommand("golemquest:incantation", null)]
	private static void Wish()
	{
		CreateAccomplishments();
	}

	public static void CreateAccomplishments()
	{
		List<MuralCategory> list = GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units.Keys.ToList();
		foreach (JournalAccomplishment accomplishment in JournalAPI.Accomplishments)
		{
			list.Remove(accomplishment.MuralCategory);
		}
		foreach (MuralCategory item in list)
		{
			JournalAPI.AddAccomplishment(GameText.GenerateMarkovMessageSentence(), "", null, null, "general", item, MuralWeight.Nil, null, -1L);
		}
	}

	static GolemIncantationSelection()
	{
		GolemMaterialSelection<JournalAccomplishment, MuralCategory>.Units = new Dictionary<MuralCategory, UnitGenerator>
		{
			{
				MuralCategory.IsBorn,
				UnitIsBorn
			},
			{
				MuralCategory.HasInspiringExperience,
				UnitLevelsSix
			},
			{
				MuralCategory.CommitsFolly,
				UnitLevelsSix
			},
			{
				MuralCategory.EnduresHardship,
				UnitLevelsSix
			},
			{
				MuralCategory.Treats,
				UnitTreats
			},
			{
				MuralCategory.Trysts,
				UnitTrysts
			},
			{
				MuralCategory.CreatesSomething,
				Unit4ArmorPieces
			},
			{
				MuralCategory.FindsObject,
				Unit4ArmorPieces
			},
			{
				MuralCategory.WieldsItemInBattle,
				Unit4ArmorPieces
			},
			{
				MuralCategory.WeirdThingHappens,
				UnitWeirdThingHappens
			},
			{
				MuralCategory.BodyExperienceBad,
				UnitRandomBodyParts
			},
			{
				MuralCategory.BodyExperienceGood,
				UnitRandomBodyParts
			},
			{
				MuralCategory.BodyExperienceNeutral,
				UnitRandomBodyParts
			},
			{
				MuralCategory.VisitsLocation,
				UnitVisitsLocation
			},
			{
				MuralCategory.DoesBureaucracy,
				UnitDoesBureaucracy
			},
			{
				MuralCategory.LearnsSecret,
				UnitLearnsSecret
			},
			{
				MuralCategory.DoesSomethingRad,
				UnitDoesSomethingRad
			},
			{
				MuralCategory.DoesSomethingHumble,
				UnitDoesSomethingHumble
			},
			{
				MuralCategory.DoesSomethingDestructive,
				UnitDoesSomethingDestructive
			},
			{
				MuralCategory.BecomesLoved,
				UnitAdjustingAura
			},
			{
				MuralCategory.MeetsWithCounselors,
				UnitAdjustingAura
			},
			{
				MuralCategory.Slays,
				UnitSlays
			},
			{
				MuralCategory.Resists,
				UnitResists
			},
			{
				MuralCategory.AppeasesBaetyl,
				UnitAppeasesBaetyl
			},
			{
				MuralCategory.CrownedSultan,
				UnitCrownedSultan
			},
			{
				MuralCategory.Dies,
				UnitDies
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitIsBorn(JournalAccomplishment Entry)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "AP",
			Value = 6
		};
	}

	private static IEnumerable<GameObjectUnit> UnitLevelsSix(JournalAccomplishment Entry)
	{
		yield return new GameObjectExperienceUnit
		{
			Levels = 6
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTreats(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new VehiclePairBonus
			{
				Stat = "Speed",
				Unpaired = 15
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitTrysts(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new VehiclePairBonus
			{
				Stat = "Speed",
				Paired = 15
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitRelicHigh(JournalAccomplishment Entry)
	{
		yield return new GameObjectRelicUnit
		{
			Tier = "7-8"
		};
	}

	private static IEnumerable<GameObjectUnit> Unit4ArmorPieces(JournalAccomplishment Entry)
	{
		yield return new GameObjectTieredArmorUnit
		{
			Tier = "4-7",
			Amount = 4,
			Gigantic = true,
			Equippable = true
		};
	}

	private static IEnumerable<GameObjectUnit> UnitWeirdThingHappens(JournalAccomplishment Entry)
	{
		yield return new GameObjectGolemQuestRandomUnit
		{
			SelectionID = "Incantation",
			Amount = 2
		};
	}

	private static IEnumerable<GameObjectUnit> UnitRandomBodyParts(JournalAccomplishment Entry)
	{
		GameObjectBodyPartUnit gameObjectBodyPartUnit = new GameObjectBodyPartUnit
		{
			Type = "Random",
			Manager = "Golem::Incantation",
			Metachromed = true
		};
		yield return new GameObjectUnitAggregate("3 random body parts", gameObjectBodyPartUnit, gameObjectBodyPartUnit, gameObjectBodyPartUnit);
	}

	private static IEnumerable<GameObjectUnit> UnitVisitsLocation(JournalAccomplishment Entry)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "MoveSpeed",
			Value = -100
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDoesBureaucracy(JournalAccomplishment Entry)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Intelligence",
			Value = 5
		};
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Willpower",
			Value = 5
		};
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Ego",
			Value = 5
		};
	}

	private static IEnumerable<GameObjectUnit> UnitLearnsSecret(JournalAccomplishment Entry)
	{
		yield return new GameObjectSecretUnit
		{
			Amount = 20
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDoesSomethingRad(JournalAccomplishment Entry)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Ego",
			Value = 10
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDoesSomethingHumble(JournalAccomplishment Entry)
	{
		yield return new GameObjectAttributeUnit
		{
			Attribute = "Ego",
			Value = -10
		};
		yield return new GameObjectExperienceUnit
		{
			Levels = 12
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDoesSomethingDestructive(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new DoubleEdge
			{
				Received = 10,
				Dealt = 20
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitAdjustingAura(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Description = "+5 quickness, +20% HP, and +3 to all attributes for nearby followers",
			Part = new AdjustingAura("&Cmotivated|Stat:Speed:5|Mult:Hitpoints:20|Stat:Strength:3|Stat:Agility:3|Stat:Toughness:3|Stat:Intelligence:3|Stat:Willpower:3|Stat:Ego:3")
		};
	}

	private static IEnumerable<GameObjectUnit> UnitSlays(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new CriticalThreshold
			{
				Value = 2
			}
		};
	}

	private static IEnumerable<GameObjectUnit> UnitResists(JournalAccomplishment Entry)
	{
		yield return new GameObjectUnitAggregate("+75 to all resists", new GameObjectAttributeUnit
		{
			Attribute = "HeatResistance",
			Value = 75
		}, new GameObjectAttributeUnit
		{
			Attribute = "ColdResistance",
			Value = 75
		}, new GameObjectAttributeUnit
		{
			Attribute = "ElectricResistance",
			Value = 75
		}, new GameObjectAttributeUnit
		{
			Attribute = "AcidResistance",
			Value = 75
		});
	}

	private static IEnumerable<GameObjectUnit> UnitAppeasesBaetyl(JournalAccomplishment Entry)
	{
		yield return new GameObjectBaetylUnit
		{
			Amount = "3",
			Tier = "7-8",
			Delay = true
		};
	}

	private static IEnumerable<GameObjectUnit> UnitCrownedSultan(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new LeaderShiftShare
			{
				RequiresPart = "SultanMask"
			},
			Description = "Share effects of worn sultan mask"
		};
	}

	private static IEnumerable<GameObjectUnit> UnitDies(JournalAccomplishment Entry)
	{
		yield return new GameObjectPartUnit
		{
			Part = new RestoreOnDeath
			{
				Amount = 1,
				Health = 50
			}
		};
	}
}

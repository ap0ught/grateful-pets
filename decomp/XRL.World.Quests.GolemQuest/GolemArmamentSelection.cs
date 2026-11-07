using System;
using System.Collections.Generic;
using Qud.API;
using XRL.Wish;
using XRL.World.Parts;
using XRL.World.Units;

namespace XRL.World.Quests.GolemQuest;

[Serializable]
[HasWishCommand]
public class GolemArmamentSelection : GolemGameObjectSelection
{
	public const string BASE_ID = "Armament";

	[NonSerialized]
	private static List<string> _Skills;

	public override string ID => "Armament";

	public override string DisplayName => "armament";

	public override char Key => 'r';

	public override bool IsCarried => true;

	public override int Consumed => 1;

	public static List<string> Skills
	{
		get
		{
			if (_Skills == null)
			{
				_Skills = new List<string>();
				foreach (GameObjectBlueprint zetachromeWeapon in ZetachromeWeapons)
				{
					string text = zetachromeWeapon.GetPartParameter<string>("MeleeWeapon", "Skill") ?? zetachromeWeapon.GetPartParameter<string>("MissileWeapon", "Skill");
					if (!text.IsNullOrEmpty() && !_Skills.Contains(text))
					{
						_Skills.Add(text);
					}
				}
				_Skills.TrimExcess();
			}
			return _Skills;
		}
	}

	private static IEnumerable<GameObjectBlueprint> ZetachromeWeapons
	{
		get
		{
			foreach (GameObjectBlueprint blueprint in GameObjectFactory.Factory.BlueprintList)
			{
				if ((blueprint.HasPart("MeleeWeapon") || blueprint.HasPart("MissileWeapon")) && blueprint.HasPart("Zetachrome") && !blueprint.HasTag("UndesirableWeapon"))
				{
					yield return blueprint;
				}
			}
		}
	}

	public override bool IsValid(GameObject Object)
	{
		if (base.IsValid(Object) && (Object.HasTag("MeleeWeapon") || Object.HasTag("MissileWeapon")) && !Object.HasTag("UndesirableWeapon"))
		{
			return Object.HasPart<Zetachrome>();
		}
		return false;
	}

	public override IEnumerable<GameObjectUnit> YieldEffectsOf(GameObject Object)
	{
		MeleeWeapon part = Object.GetPart<MeleeWeapon>();
		if (part != null && part.Skill != null)
		{
			yield return new GameObjectUnitAggregate(new GameObjectSkillUnit
			{
				Skill = part.Skill,
				Power = "*"
			}, new GameObjectMetachromeUnit
			{
				Skill = part.Skill
			});
		}
		MissileWeapon part2 = Object.GetPart<MissileWeapon>();
		if (part2 != null && part2.Skill != null)
		{
			yield return new GameObjectUnitAggregate(new GameObjectSkillUnit
			{
				Skill = part2.Skill,
				Power = "*"
			}, new GameObjectMetachromeUnit
			{
				Skill = part2.Skill
			});
		}
	}

	public override IEnumerable<GameObjectUnit> YieldAllEffects()
	{
		foreach (string skill in Skills)
		{
			yield return new GameObjectSkillUnit
			{
				Skill = skill,
				Power = "*"
			};
		}
		yield return new GameObjectMetachromeUnit
		{
			Skill = Skills.GetRandomElement()
		};
	}

	public override IEnumerable<GameObjectUnit> YieldRandomEffects()
	{
		string randomElement = Skills.GetRandomElement();
		yield return new GameObjectUnitAggregate(new GameObjectSkillUnit
		{
			Skill = randomElement,
			Power = "*"
		}, new GameObjectMetachromeUnit
		{
			Skill = randomElement
		});
	}

	public override void Apply(GameObject Object)
	{
		base.Apply(Object);
		string value = Material?.GetPart<MeleeWeapon>()?.Skill ?? Material?.GetPart<MissileWeapon>()?.Skill;
		if (!value.IsNullOrEmpty())
		{
			Object.SetStringProperty("ArmamentSkill", value);
		}
	}

	public string GetFirstSkill()
	{
		return GetFirstSkill(Material);
	}

	public string GetFirstSkill(GameObject Object)
	{
		if (Object != null)
		{
			string text = Object.GetPart<MeleeWeapon>()?.Skill;
			if (text == null)
			{
				MissileWeapon part = Material.GetPart<MissileWeapon>();
				if (part == null)
				{
					return null;
				}
				text = part.Skill;
			}
			return text;
		}
		return null;
	}

	[WishCommand("golemquest:armament", null)]
	private static void Wish()
	{
		ReceiveMaterials(The.Player);
	}

	public static void ReceiveMaterials(GameObject Object)
	{
		foreach (GameObjectBlueprint zetachromeWeapon in ZetachromeWeapons)
		{
			if (EncountersAPI.IsEligibleForDynamicEncounters(zetachromeWeapon))
			{
				GameObject gameObject = GameObjectFactory.Factory.CreateUnmodifiedObject(zetachromeWeapon);
				Object.ReceiveObject(gameObject);
			}
		}
	}
}

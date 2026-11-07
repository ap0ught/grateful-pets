using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Language;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.World.Quests.GolemQuest;

namespace XRL.World.Units;

[Serializable]
public class GameObjectMetachromeUnit : GameObjectUnit
{
	public string Skill;

	private static readonly string[] ExcludedTypes = new string[3] { "Missile Weapon", "Thrown Weapon", "Ammo" };

	public override void Apply(GameObject Object)
	{
		Body body = GolemQuestSystem.Get()?.Body.Material?.Body ?? Object.Body;
		Dictionary<string, List<BodyPart>> dictionary = new Dictionary<string, List<BodyPart>>();
		foreach (BodyPart item in Object.Body.LoopParts())
		{
			if (item.Type == null || item.Abstract || item.Extrinsic || ExcludedTypes.Contains(item.Type) || (item.VariantType != null && ExcludedTypes.Contains(item.VariantType)))
			{
				continue;
			}
			if (item.DefaultBehavior != null)
			{
				if (!IsDefaultWeapon(item.DefaultBehavior))
				{
					continue;
				}
				item.DefaultBehavior = null;
				item.DefaultBehaviorBlueprint = null;
			}
			if (IsCursed(item.Equipped))
			{
				if (IsDefaultWeapon(item.Equipped))
				{
					MeleeWeapon part = item.Equipped.GetPart<MeleeWeapon>();
					if (part != null)
					{
						part.Skill = Skill;
					}
				}
				continue;
			}
			if (IsDefaultWeapon(item.Equipped))
			{
				item.Unequip();
			}
			if (!dictionary.TryGetValue(item.Type, out var value))
			{
				value = (dictionary[item.Type] = new List<BodyPart>());
			}
			value.Add(item);
		}
		List<string> list2 = new List<string>();
		foreach (BodyPart item2 in body.LoopParts())
		{
			if (dictionary.ContainsKey(item2.Type) && !list2.Contains(item2.Type) && (IsDefaultWeapon(item2.Equipped) || IsDefaultWeapon(item2.DefaultBehavior)))
			{
				list2.Add(item2.Type);
			}
		}
		if (list2.IsNullOrEmpty())
		{
			string text = body.LoopParts().FirstOrDefault((BodyPart x) => x.Primary)?.Type;
			if (!text.IsNullOrEmpty() && dictionary.ContainsKey(text))
			{
				list2.Add(text);
			}
			else
			{
				text = body.GetPrimaryLimbType();
				if (dictionary.ContainsKey(text))
				{
					list2.Add(text);
				}
				else
				{
					text = Object.Body.GetPrimaryLimbType();
					list2.Add(dictionary.ContainsKey(text) ? text : dictionary.Keys.GetRandomElement());
				}
			}
		}
		foreach (string item3 in list2)
		{
			if (!dictionary.TryGetValue(item3, out var value2))
			{
				MetricsManager.LogError("No limbs by slot '" + item3 + "' found for " + Object.ShortDisplayNameStripped);
			}
			else
			{
				if (!TryGetBlueprintFor(item3, Skill, out var Blueprint))
				{
					continue;
				}
				foreach (BodyPart item4 in value2)
				{
					if (item4.DefaultBehavior == null)
					{
						SetDefaultBehavior(item4, Blueprint);
					}
				}
			}
		}
		Object.Body.RecalculateFirsts();
	}

	public static void SetDefaultBehavior(BodyPart Limb, GameObjectBlueprint Blueprint)
	{
		Limb.DefaultBehaviorBlueprint = Blueprint.Name;
		Limb.DefaultBehavior = GameObjectFactory.Factory.CreateUnmodifiedObject(Blueprint);
		MeleeWeapon part = Limb.DefaultBehavior.GetPart<MeleeWeapon>();
		if (part != null)
		{
			part.Slot = Limb.Type;
		}
	}

	public static bool TryGetBlueprintFor(string Type, string Skill, out GameObjectBlueprint Blueprint)
	{
		GameObjectFactory factory = GameObjectFactory.Factory;
		if (!factory.Blueprints.TryGetValue("Metachrome " + Type + " " + Skill, out Blueprint))
		{
			MetricsManager.LogError("No blueprint by id 'Metachrome " + Type + " " + Skill + "' found, defaulting to 'Metachrome " + Skill + "'");
			if (!factory.Blueprints.TryGetValue("Metachrome " + Skill, out Blueprint))
			{
				MetricsManager.LogError("No blueprint by id 'Metachrome " + Skill + "' found, skipping '" + Type + "' slot");
				return false;
			}
		}
		return true;
	}

	private bool IsCursed(GameObject Object)
	{
		if (Object != null)
		{
			return !Object.CanBeUnequipped();
		}
		return false;
	}

	private bool IsDefaultWeapon(GameObject Object)
	{
		if (Object != null && Object.IsNatural())
		{
			return Object.GetBlueprint().DescendsFrom("NaturalWeapon");
		}
		return false;
	}

	public override void Reset()
	{
		base.Reset();
		Skill = null;
	}

	public override bool CanInscribe()
	{
		return false;
	}

	public override string GetDescription(bool Inscription = false)
	{
		string text = "Metachrome " + Skill;
		if (!GameObjectFactory.Factory.Blueprints.TryGetValue(text, out var value))
		{
			return "[GameObjectMetachromeUnit Error: Unknown blueprint '" + text + "']";
		}
		return "Equipped with " + Grammar.Pluralize(value.DisplayName());
	}
}

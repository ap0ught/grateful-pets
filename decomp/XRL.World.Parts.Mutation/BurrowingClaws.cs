using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.UI;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class BurrowingClaws : BaseDefaultEquipmentMutation
{
	public static readonly string OBJECT_BLUEPRINT_NAME = "Burrowing Claws Claw";

	public string BodyPartType = "Hands";

	public Guid DigUpActivatedAbilityID = Guid.Empty;

	public Guid DigDownActivatedAbilityID = Guid.Empty;

	public Guid EnableActivatedAbilityID = Guid.Empty;

	public bool PathAsBurrower = true;

	[NonSerialized]
	protected GameObjectBlueprint _Blueprint;

	public GameObjectBlueprint Blueprint
	{
		get
		{
			if (_Blueprint == null)
			{
				_Blueprint = GameObjectFactory.Factory.GetBlueprint(GetBlueprintName());
			}
			return _Blueprint;
		}
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		int wallBonusPenetration = GetWallBonusPenetration(Level);
		if (wallBonusPenetration < 0)
		{
			stats.Set("WallPenetration", "-" + wallBonusPenetration, !stats.mode.Contains("ability"));
		}
		else
		{
			stats.Set("WallPenetration", "+" + wallBonusPenetration, !stats.mode.Contains("ability"));
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<AfterGameLoadedEvent>.ID && ID != PooledEvent<PartSupportEvent>.ID && ID != PooledEvent<PathAsBurrowerEvent>.ID && ID != PooledEvent<PreferDefaultBehaviorEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(EnableActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PathAsBurrowerEvent E)
	{
		if (PathAsBurrower && IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PartSupportEvent E)
	{
		if (E.Skip != this && E.Type == "Digging" && IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterGameLoadedEvent E)
	{
		NeedPartSupportEvent.Send(ParentObject, "Digging");
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CommandDigDown");
		Registrar.Register("CommandDigUp");
		Registrar.Register("CommandToggleBurrowingClaws");
		base.Register(Object, Registrar);
	}

	public string GetBlueprintName()
	{
		return Variant.Coalesce(OBJECT_BLUEPRINT_NAME);
	}

	public bool CheckDig()
	{
		if (ParentObject.AreHostilesNearby())
		{
			Popup.ShowFail("You can't excavate with hostiles nearby.");
			return false;
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CommandToggleBurrowingClaws")
		{
			ToggleMyActivatedAbility(EnableActivatedAbilityID);
			if (IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
			{
				ParentObject.RequirePart<Digging>();
			}
			else
			{
				NeedPartSupportEvent.Send(ParentObject, "Digging");
			}
			ParentObject.RemoveStringProperty("Burrowing");
		}
		else if (E.ID == "CommandDigDown")
		{
			if (CheckDig())
			{
				Cell cellFromDirection = ParentObject.CurrentCell.GetCellFromDirection("D", BuiltOnly: false);
				ParentObject.CurrentCell.AddObject("StairsDown");
				cellFromDirection.AddObject("StairsUp");
				ParentObject.PlayWorldSound("sfx_ability_mutation_burrowingClaws_burrow");
				DidX("dig", "a passage up", null, null, null, ParentObject);
			}
		}
		else if (E.ID == "CommandDigUp")
		{
			if (ParentObject.CurrentZone.IsOutside())
			{
				Popup.ShowFail("You can't excavate the sky!");
				return true;
			}
			if (CheckDig())
			{
				Cell cellFromDirection2 = ParentObject.CurrentCell.GetCellFromDirection("U", BuiltOnly: false);
				ParentObject.CurrentCell.AddObject("StairsUp");
				cellFromDirection2.AddObject("StairsDown");
				ParentObject?.PlayWorldSound("sfx_ability_mutation_burrowingClaws_burrow");
				DidX("dig", "a passage down", null, null, null, ParentObject);
			}
		}
		return base.FireEvent(E);
	}

	public string GetPenetration()
	{
		return GetPenetration(Level);
	}

	public static string GetPenetration(int Level)
	{
		return "1d6+" + 3 * Level;
	}

	public int GetWallBonusPenetration()
	{
		return GetWallBonusPenetration(Level);
	}

	public static int GetWallBonusPenetration(int Level)
	{
		return Level * 3;
	}

	public double GetWallBonusPercentage()
	{
		return GetWallBonusPercentage(Level, ParentObject);
	}

	public static double GetWallBonusPercentage(int Level, GameObject Mutant = null)
	{
		int num = 25;
		if (Mutant != null && Mutant.IsGiganticCreature)
		{
			num *= 2;
		}
		return num;
	}

	public int GetWallHitsRequired()
	{
		return GetWallHitsRequired(Level, ParentObject);
	}

	public static int GetWallHitsRequired(int Level, GameObject Mutant = null)
	{
		return Drill.GetWallHitsRequired(GetWallBonusPercentage(Level, Mutant));
	}

	public int GetAV(int Level)
	{
		if (Level < 5)
		{
			return 1;
		}
		if (Level < 9)
		{
			return 2;
		}
		return 3;
	}

	public override string GetDescription()
	{
		return Blueprint.GetTag("VariantDescription").Coalesce("You bear spade-like claws that can burrow through the earth.");
	}

	public override string GetLevelText(int Level)
	{
		string cachedDisplayNameStrippedTitleCase = Blueprint.CachedDisplayNameStrippedTitleCase;
		string value = Grammar.Pluralize(cachedDisplayNameStrippedTitleCase);
		int wallBonusPenetration = GetWallBonusPenetration(Level);
		StringBuilder stringBuilder = Event.NewStringBuilder().Append(cachedDisplayNameStrippedTitleCase).Append(" penetration vs. walls: {{rules|")
			.Append(wallBonusPenetration.Signed())
			.Append("}}\n");
		int wallHitsRequired = GetWallHitsRequired(Level, ParentObject);
		if (wallHitsRequired > 0)
		{
			stringBuilder.Append(value).Append(" destroy walls after ").Append(wallHitsRequired)
				.Append(" penetrating hits.\n");
		}
		if (Options.EnablePrereleaseContent)
		{
			stringBuilder.Append("Can dig passages up or down when outside of combat\n");
		}
		stringBuilder.Append(value).Append(" are also a ").Append(GetWeaponClass())
			.Append(" class natural weapon that deal {{rules|")
			.Append(GetClawsDamage(Level))
			.Append("}} base damage to non-walls.");
		return Event.FinalizeString(stringBuilder);
	}

	public string GetWeaponClass()
	{
		string partParameter = Blueprint.GetPartParameter("MeleeWeapon", "Skill", "ShortBlades");
		if (Skills.WeaponClassName.TryGetValue(partParameter, out var Value))
		{
			return Value;
		}
		return "short-blade";
	}

	public string GetClawsDamage(int Level)
	{
		ReadOnlySpan<char> value = default;
		DelimitedEnumeratorChar enumerator = Blueprint.GetTag("VariantDamage").DelimitedBy(',').GetEnumerator();
		while (enumerator.MoveNext())
		{
			ReadOnlySpan<char> current = enumerator.Current;
			int num = current.IndexOf(':');
			if (int.TryParse(current.Slice(0, num), out var result) && result <= Level)
			{
				value = current.Slice(num + 1);
			}
		}
		if (value.Length == 0)
		{
			return "1d2";
		}
		return new string(value);
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		GameObjectBlueprint blueprint = Blueprint;
		string partParameter = blueprint.GetPartParameter("MeleeWeapon", "Slot", "Hand");
		List<BodyPart> part = body.GetPart(partParameter);
		int level = Level;
		for (int i = 0; i < part.Count && i < 2; i++)
		{
			BodyPart bodyPart = part[i];
			if (bodyPart.DefaultBehavior == null || bodyPart.DefaultBehavior.GetBlueprint() != blueprint)
			{
				bodyPart.DefaultBehavior = GameObject.Create(blueprint);
				bodyPart.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "BurrowingClaws");
			}
			if (bodyPart.DefaultBehavior.TryGetPart<MeleeWeapon>(out var Part))
			{
				Part.BaseDamage = GetClawsDamage(level);
			}
			if (bodyPart.DefaultBehavior.TryGetPart<BurrowingClawsProperties>(out var Part2))
			{
				Part2.WallBonusPenetration = GetWallBonusPenetration(level);
				Part2.WallBonusPercentage = GetWallBonusPercentage(level);
			}
		}
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		_Blueprint = null;
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (Options.EnablePrereleaseContent)
		{
			DigUpActivatedAbilityID = AddMyActivatedAbility("Excavate up", "CommandDigUp", "Physical Mutations", null, "\u0018");
			DigDownActivatedAbilityID = AddMyActivatedAbility("Excavate down", "CommandDigDown", "Physical Mutations", null, "\u0019");
		}
		EnableActivatedAbilityID = AddMyActivatedAbility(GetDisplayName(), "CommandToggleBurrowingClaws", "Physical Mutations", null, "ë", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: true, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		if (IsMyActivatedAbilityToggledOn(EnableActivatedAbilityID))
		{
			GO.RequirePart<Digging>();
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		NeedPartSupportEvent.Send(GO, "Digging", this);
		RemoveMyActivatedAbility(ref DigUpActivatedAbilityID);
		RemoveMyActivatedAbility(ref DigDownActivatedAbilityID);
		RemoveMyActivatedAbility(ref EnableActivatedAbilityID);
		return base.Unmutate(GO);
	}
}

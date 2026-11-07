using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class HooksForFeet : BaseDefaultEquipmentMutation
{
	public GameObject HooksObject;

	[NonSerialized]
	protected GameObjectBlueprint _Blueprint;

	public string BlueprintName => Variant.Coalesce("Hooks");

	public GameObjectBlueprint Blueprint
	{
		get
		{
			if (_Blueprint == null)
			{
				_Blueprint = GameObjectFactory.Factory.GetBlueprint(BlueprintName);
			}
			return _Blueprint;
		}
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override string GetDescription()
	{
		string tag = Blueprint.GetTag("VariantDescription");
		if (tag.IsNullOrEmpty())
		{
			return "You have " + Blueprint.DisplayName() + " for feet.\n\nYou cannot wear shoes.";
		}
		return tag;
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		_Blueprint = null;
	}

	public override void OnRegenerateDefaultEquipment(Body Body)
	{
		if (!GameObject.Validate(ref HooksObject))
		{
			HooksObject = GameObject.Create(Blueprint);
		}
		string type = Blueprint.GetPartParameter<string>("Armor", "WornOn") ?? Blueprint.GetPartParameter("MeleeWeapon", "Slot", "Feet");
		BodyPart bodyPart = RequireRegisteredSlot(Body, type);
		if (bodyPart != null && bodyPart.Equipped != HooksObject && (bodyPart.Equipped == null || bodyPart.ForceUnequip(Silent: true)) && !ParentObject.ForceEquipObject(HooksObject, bodyPart, Silent: true, 0))
		{
			MetricsManager.LogError("HooksForFeet force equip on " + (bodyPart?.Name ?? "NULL") + " failed");
		}
		base.OnRegenerateDefaultEquipment(Body);
	}

	public override bool Unmutate(GameObject GO)
	{
		CleanUpMutationEquipment(GO, ref HooksObject);
		return base.Unmutate(GO);
	}
}

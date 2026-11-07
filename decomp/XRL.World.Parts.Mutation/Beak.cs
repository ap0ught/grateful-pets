using System;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Beak : BaseDefaultEquipmentMutation
{
	public GameObject BeakObject;

	public string BodyPartType = "Face";

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Beak obj = base.DeepCopy(Parent, MapInv) as Beak;
		obj.BeakObject = null;
		return obj;
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}

	public override string GetDescription()
	{
		if (!Variant.IsNullOrEmpty())
		{
			return "Your face bears a sightly " + GetVariantName().ToLowerInvariant() + ".\n\n+1 Ego\nYou occasionally peck at your opponents.\n+200 reputation with {{w|birds}}";
		}
		return "Your face bears a sightly beak.\n\n+1 Ego\nYou occasionally peck at your opponents.\n+200 reputation with {{w|birds}}";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	public override void OnRegenerateDefaultEquipment(Body body)
	{
		if (!TryGetRegisteredSlot(body, BodyPartType, out var Part))
		{
			Part = body.GetFirstPart(BodyPartType);
			if (Part != null)
			{
				RegisterSlot(BodyPartType, Part);
			}
		}
		if (Part != null)
		{
			BeakObject = GameObjectFactory.Factory.CreateObject(Variant ?? "Beak");
			MeleeWeapon part = BeakObject.GetPart<MeleeWeapon>();
			Armor part2 = BeakObject.GetPart<Armor>();
			part.Skill = "ShortBlades";
			part.BaseDamage = "1";
			part.Slot = Part.Type;
			part2.WornOn = Part.Type;
			part2.AV = 0;
			Part.DefaultBehavior = BeakObject;
			Part.DefaultBehavior.SetStringProperty("TemporaryDefaultBehavior", "Beak");
			BeakObject.SetStringProperty("HitSound", "Sounds/Abilities/sfx_ability_mutation_beak_peck");
			ResetDisplayName();
		}
		base.OnRegenerateDefaultEquipment(body);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		StatShifter.SetStatShift(ParentObject, "Ego", 1, baseValue: true);
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		StatShifter.RemoveStatShifts(ParentObject);
		CleanUpMutationEquipment(GO, ref BeakObject);
		return base.Unmutate(GO);
	}
}

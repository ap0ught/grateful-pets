using System;
using XRL.Language;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Horns : BaseMutation
{
	public GameObject HornsObject;

	public override bool GeneratesEquipment()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<RegenerateDefaultEquipmentEvent>.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("might", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
	{
		RegrowHorns();
		return base.HandleEvent(E);
	}

	public override string GetDescription()
	{
		if (Variant == null)
		{
			return "Horns jut out of your head.";
		}
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Variant);
		string propertyOrTag = blueprint.GetPropertyOrTag("Gender");
		string cachedDisplayNameStripped = blueprint.CachedDisplayNameStripped;
		if (propertyOrTag == "plural")
		{
			return Grammar.InitCap(cachedDisplayNameStripped) + " jut out of your head.";
		}
		return Grammar.A(cachedDisplayNameStripped, Capitalize: true) + " juts out of your head.";
	}

	public int GetAV(int Level)
	{
		return 1 + (Level - 1) / 3;
	}

	public string GetBaseDamage(int Level)
	{
		return "2d" + (3 + Level / 3);
	}

	public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
	{
		Horns obj = base.DeepCopy(Parent, MapInv) as Horns;
		obj.HornsObject = null;
		return obj;
	}

	public override string GetLevelText(int Level)
	{
		string baseDamage = GetBaseDamage(Level);
		int aV = GetAV(Level);
		string text = "20% chance on melee attack to gore your opponent\n";
		text = text + "Damage increment: {{rules|" + baseDamage + "}}\n";
		text = text + "To-hit bonus: {{rules|" + HornsProperties.GetToHitBonus(Level) + "}}\n";
		if (Level == base.Level)
		{
			text += "Goring attacks may cause bleeding\n";
		}
		else
		{
			text = ((Level % 4 != 1) ? (text + "{{rules|Increased bleeding save difficulty}}\n") : (text + "{{rules|Increased bleeding save difficulty and intensity}}\n"));
		}
		string text2 = "plural";
		string word;
		if (Variant == null)
		{
			word = "horns";
		}
		else
		{
			GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(Variant);
			text2 = blueprint.GetPropertyOrTag("Gender");
			word = blueprint.CachedDisplayNameStripped;
		}
		text = ((!(text2 == "plural")) ? (text + Grammar.InitCap(word) + " is a short-blade class natural weapon.\n") : (text + Grammar.InitCap(word) + " are a short-blade class natural weapon.\n"));
		text = text + "+{{rules|" + aV + " AV}}\n";
		text += "Cannot wear helmets\n";
		return text + "+100 reputation with {{w|antelopes}} and {{w|goatfolk}}";
	}

	public void RegrowHorns(bool Force = false)
	{
		Body body = ParentObject.Body;
		if (body == null || Variant.IsNullOrEmpty())
		{
			return;
		}
		string partParameter = GameObjectFactory.Factory.GetBlueprint(Variant).GetPartParameter("MeleeWeapon", "Slot", "Head");
		BodyPart bodyPart = HornsObject?.DefaultOrEquippedPart();
		if (bodyPart == null || bodyPart.Type != partParameter)
		{
			bodyPart = body.GetFirstPart(partParameter);
		}
		if (bodyPart != null && (Force || !HornsObject.IsValid() || !(HornsObject.Blueprint == Variant) || HornsObject.DefaultOrEquippedPart() != bodyPart))
		{
			if (HornsObject != null && (!HornsObject.IsValid() || HornsObject.Blueprint != Variant))
			{
				GameObject.Release(ref HornsObject);
			}
			if (HornsObject == null)
			{
				HornsObject = GameObject.Create(Variant);
			}
			MeleeWeapon part = HornsObject.GetPart<MeleeWeapon>();
			part.Slot = bodyPart.Type;
			part.MaxStrengthBonus = 100;
			part.BaseDamage = GetBaseDamage(Level);
			Armor part2 = HornsObject.GetPart<Armor>();
			part2.WornOn = bodyPart.Type;
			part2.AV = GetAV(Level);
			ParentObject.ForceEquipObject(HornsObject, bodyPart, Silent: true, 0);
			ResetDisplayName();
		}
	}

	public override bool ChangeLevel(int NewLevel)
	{
		RegrowHorns(Force: true);
		return base.ChangeLevel(NewLevel);
	}

	public override void SetVariant(string Variant)
	{
		base.SetVariant(Variant);
		if (HornsObject != null && HornsObject.Blueprint != Variant)
		{
			RegrowHorns(Force: true);
		}
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		if (Variant.IsNullOrEmpty())
		{
			Variant = GetVariants().GetRandomElement();
			ResetDisplayName();
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		CleanUpMutationEquipment(GO, ref HornsObject);
		return base.Unmutate(GO);
	}
}

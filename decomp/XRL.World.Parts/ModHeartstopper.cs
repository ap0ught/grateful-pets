using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class ModHeartstopper : IModification
{
	public ModHeartstopper()
	{
	}

	public ModHeartstopper(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart<CardiacArrestOnHit>())
		{
			return false;
		}
		GameObject Projectile = null;
		string Blueprint = null;
		GetMissileWeaponProjectileEvent.GetFor(Object, ref Projectile, ref Blueprint);
		if (Projectile != null && Projectile.HasPart<CardiacArrestOnHit>())
		{
			return false;
		}
		if (!Blueprint.IsNullOrEmpty())
		{
			GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(Blueprint);
			if (blueprintIfExists != null && blueprintIfExists.HasPart("CardiacArrestOnHit"))
			{
				return false;
			}
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		CardiacArrestOnHit cardiacArrestOnHit = Object.RequirePart<CardiacArrestOnHit>();
		cardiacArrestOnHit.Chance = GetBaseChance();
		cardiacArrestOnHit.SaveTarget = GetSaveTarget();
		cardiacArrestOnHit.SaveAttribute = GetSaveAttribute();
		cardiacArrestOnHit.ChargeUse = 200;
		cardiacArrestOnHit.IsBootSensitive = true;
		cardiacArrestOnHit.IsEMPSensitive = true;
		cardiacArrestOnHit.IsPowerSwitchSensitive = true;
		cardiacArrestOnHit.IsPowerLoadSensitive = true;
		cardiacArrestOnHit.IsTechScannable = true;
		cardiacArrestOnHit.NameForStatus = "CardiacDisruptor";
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexityIfComplex(2, 2);
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
			E.AddAdjective("{{lovesickness|heartstopper}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("circuitry", 1);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public static string GetDescription(int Tier)
	{
		return "Heartstopper: When powered, this weapon has a chance to put opponents into cardiac arrest.";
	}

	public string GetInstanceDescription()
	{
		CardiacArrestOnHit part = ParentObject.GetPart<CardiacArrestOnHit>();
		if (part == null)
		{
			return GetDescription(Tier);
		}
		return "Heartstopper: When powered, this weapon has " + Grammar.A(part.GetActivationChance(ParentObject.Holder)) + "% chance to put opponents into cardiac arrest" + (part.SaveAttribute.IsNullOrEmpty() ? "" : (" if they fail a difficulty " + part.GetSaveTarget() + " " + part.SaveAttribute + " save")) + ".";
	}

	public int GetBaseChance()
	{
		return 9 + Tier;
	}

	public int GetSaveTarget()
	{
		return 18 + Tier * 2;
	}

	public string GetSaveAttribute()
	{
		return "Toughness";
	}
}

using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class ModSpiked : IModification
{
	public ModSpiked()
	{
	}

	public ModSpiked(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "PenetrationModule";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart<Shield>())
		{
			return true;
		}
		if (!IModification.CheckWornSlot(Object, "Hands"))
		{
			return false;
		}
		if (Object.HasPart<BleedingOnHit>())
		{
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (!Object.HasPart<Shield>())
		{
			BleedingOnHit bleedingOnHit = new BleedingOnHit();
			bleedingOnHit.Amount = "1d3";
			bleedingOnHit.SaveTarget = 20 + Tier * 2;
			bleedingOnHit.WorksOnEquipper = true;
			bleedingOnHit.RequireDamageAttribute = "Unarmed";
			bleedingOnHit.Stack = true;
			Object.AddPart(bleedingOnHit);
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShieldSlamDamageEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == ShieldSlamPerformedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShieldSlamDamageEvent E)
	{
		if (E.Shield == ParentObject)
		{
			E.Damage += E.StrengthMod;
			if (E.Attributes == "Bludgeoning")
			{
				E.Attributes = "Stabbing";
			}
			else if (E.Attributes.HasDelimitedSubstring(' ', "Bludgeoning"))
			{
				string[] array = E.Attributes.Split(' ');
				array[Array.IndexOf(array, "Bludgeoning")] = "Stabbing";
				E.Attributes = string.Join(' ', array);
			}
			else
			{
				E.Attributes = E.Attributes.AddDelimitedSubstring(' ', "Stabbing");
			}
			if (E.ExtraDesc != null)
			{
				E.ExtraDesc += "+bleeding";
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ShieldSlamPerformedEvent E)
	{
		if (E.Shield == ParentObject && GameObject.Validate(E.Target) && !E.Target.IsNowhere())
		{
			E.Target.ApplyEffect(new Bleeding("1d2", 20 + E.ShieldAV, ParentObject));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{spiked|spiked}}", 5);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetInstanceDescription());
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Spiked: This item causes additional bleeding damage.";
	}

	public static string GetDescription(int Tier, GameObject obj)
	{
		if (obj.HasPart<Shield>())
		{
			return "Spiked: This item adds bonus damage to Shield Slam equal to your Strength modifier and causes your target to bleed.";
		}
		return "Spiked: Unarmed attacks performed while this item is equipped cause bleeding.";
	}

	public string GetInstanceDescription()
	{
		return GetDescription(Tier, ParentObject);
	}
}

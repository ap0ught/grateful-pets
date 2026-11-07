using System;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
public class ModJacked : IModification
{
	public ModJacked()
	{
	}

	public ModJacked(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		if (Object.HasPart<IntegratedPowerSystems>())
		{
			return false;
		}
		if (!CheckUsesChargeWhileEquippedEvent.Check(Object))
		{
			return false;
		}
		try
		{
			string usesSlots = Object.UsesSlots;
			if (!string.IsNullOrEmpty(usesSlots))
			{
				bool flag = false;
				foreach (string item in usesSlots.CachedCommaExpansion())
				{
					if (Anatomies.GetBodyPartTypeOrFail(item).Contact != false)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			else
			{
				Armor part = Object.GetPart<Armor>();
				if (part != null && part.WornOn != "*" && Anatomies.GetBodyPartTypeOrFail(part.WornOn).Contact == false)
				{
					return false;
				}
				Shield part2 = Object.GetPart<Shield>();
				if (part2 != null && part2.WornOn != "*" && Anatomies.GetBodyPartTypeOrFail(part2.WornOn).Contact == false)
				{
					return false;
				}
				MissileWeapon part3 = Object.GetPart<MissileWeapon>();
				if (part3 != null && part3.SlotType != "*" && Anatomies.GetBodyPartTypeOrFail(part3.SlotType).Contact == false)
				{
					return false;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("ModJacked contact check", x);
			return false;
		}
		return true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (!Object.HasPart<IntegratedPowerSystems>())
		{
			IntegratedPowerSystems integratedPowerSystems = new IntegratedPowerSystems();
			integratedPowerSystems.RequiresEvent = "HasPowerConnectors";
			Object.AddPart(integratedPowerSystems);
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{c|jacked}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Jacked: When equipped by a robot, cyborg, or mutant with the ability to generate electricity or access to grid power, this item can draw power.";
	}

	public string GetPhysicalDescription()
	{
		return "";
	}
}

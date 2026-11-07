using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class ModNav : IModification
{
	public ModNav()
	{
	}

	public ModNav(int Tier)
		: base(Tier)
	{
	}

	public override void ApplyModification(GameObject Object)
	{
		NavigationBonus navigationBonus = Object.GetPart<NavigationBonus>();
		int num = 20 + Tier * 3;
		int num2 = 15 + Tier * 2;
		int num3 = 15 + Tier * 2;
		if (navigationBonus == null)
		{
			navigationBonus = new NavigationBonus();
			navigationBonus.PercentBonus = num.ToString();
			navigationBonus.EncounterPercentBonus = num2.ToString();
			navigationBonus.SpeedPercentBonus = num3.ToString();
			navigationBonus.ChargeUse = 1;
			navigationBonus.ComputePowerFactor = 1f;
			navigationBonus.SingleApplicationKey = "NavMapApplied";
			navigationBonus.ShowInShortDescription = true;
			Object.AddPart(navigationBonus);
		}
		else
		{
			if (string.IsNullOrEmpty(navigationBonus.PercentBonus))
			{
				navigationBonus.PercentBonus = num.ToString();
			}
			else if (Stat.RollMin(navigationBonus.PercentBonus) == Stat.RollMax(navigationBonus.PercentBonus))
			{
				navigationBonus.PercentBonus = (Stat.Roll(navigationBonus.PercentBonus) + num).ToString();
			}
			else
			{
				navigationBonus.PercentBonus += num.Signed();
			}
			if (string.IsNullOrEmpty(navigationBonus.EncounterPercentBonus))
			{
				navigationBonus.EncounterPercentBonus = num2.ToString();
			}
			else if (Stat.RollMin(navigationBonus.EncounterPercentBonus) == Stat.RollMax(navigationBonus.EncounterPercentBonus))
			{
				navigationBonus.EncounterPercentBonus = (Stat.Roll(navigationBonus.EncounterPercentBonus) + num2).ToString();
			}
			else
			{
				navigationBonus.EncounterPercentBonus += num2.Signed();
			}
			if (string.IsNullOrEmpty(navigationBonus.SpeedPercentBonus))
			{
				navigationBonus.SpeedPercentBonus = num3.ToString();
			}
			else if (Stat.RollMin(navigationBonus.SpeedPercentBonus) == Stat.RollMax(navigationBonus.SpeedPercentBonus))
			{
				navigationBonus.SpeedPercentBonus = (Stat.Roll(navigationBonus.SpeedPercentBonus) + num3).ToString();
			}
			else
			{
				navigationBonus.SpeedPercentBonus += num3.Signed();
			}
			if (navigationBonus.ComputePowerFactor < 1f)
			{
				navigationBonus.ComputePowerFactor = 1f;
			}
			if (string.IsNullOrEmpty(navigationBonus.SingleApplicationKey))
			{
				navigationBonus.SingleApplicationKey = "NavMapApplied";
			}
			navigationBonus.ChargeUse++;
		}
		navigationBonus.IsEMPSensitive = true;
		navigationBonus.IsBootSensitive = true;
		int num4 = Math.Max((Tier >= 5) ? (5 - Tier / 2) : Tier, 2);
		BootSequence part = Object.GetPart<BootSequence>();
		if (part == null)
		{
			part = new BootSequence();
			part.BootTime = num4;
			part.ReadoutInName = true;
			part.ReadoutInDescription = true;
			Object.AddPart(part);
		}
		else
		{
			if (part.BootTime < num4)
			{
				part.BootTime = num4;
			}
			part.ReadoutInName = true;
			part.ReadoutInDescription = true;
		}
		Object.RequirePart<EnergyCellSocket>();
		IncreaseDifficultyAndComplexity(1, 2);
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
			E.AddAdjective("{{r|nav}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Base.Compound(GetPhysicalDescription());
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("travel", 10);
		}
		return base.HandleEvent(E);
	}

	public string GetPhysicalDescription()
	{
		return "";
	}

	public static string GetDescription(int Tier)
	{
		return "Nav: When powered and booted up, this item enhances navigation.";
	}
}

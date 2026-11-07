using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ModSereneVisage : IModification
{
	public ModSereneVisage()
	{
	}

	public ModSereneVisage(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "SereneVisage";
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Head", "Face", null, null, AllowGeneric: true, AllowMagnetized: true, AllowPartial: false);
	}

	public override void ApplyModification(GameObject Object)
	{
		string text = Object.UsesSlots ?? Object.GetPart<Armor>()?.WornOn;
		if (text.IsNullOrEmpty() || text == "Head" || text == "Face")
		{
			text = "Head,Face";
		}
		else if (text.Contains(","))
		{
			if (!text.HasDelimitedSubstring(',', "Head"))
			{
				text += ",Head";
			}
			if (!text.HasDelimitedSubstring(',', "Face"))
			{
				text += ",Face";
			}
		}
		else
		{
			text += ",Head,Face";
		}
		Object.UsesSlots = text;
		Object.GetPart<Armor>().Willpower++;
		string propertyOrTag = Object.GetPropertyOrTag("Mods");
		if (!propertyOrTag.IsNullOrEmpty())
		{
			if (propertyOrTag != "None")
			{
				List<string> list = new List<string>(propertyOrTag.Split(','));
				if (!list.Contains("HeadwearMods"))
				{
					list.Add("HeadwearMods");
				}
				if (!list.Contains("MaskMods"))
				{
					list.Add("MaskMods");
				}
				Object.SetStringProperty("Mods", string.Join(",", list.ToArray()));
			}
		}
		else
		{
			Object.SetStringProperty("Mods", "HeadwearMods,MaskMods");
		}
		IncreaseDifficultyIfComplex(1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.ModIntProperty("Serene", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.ModIntProperty("Serene", -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{Y|serene}} visage");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Serene visage: This item grants bonus Willpower, reduces the cooldown of Meditate by 40 rounds, and is worn on both the head and face. It requires both the head and face to wear.";
	}
}

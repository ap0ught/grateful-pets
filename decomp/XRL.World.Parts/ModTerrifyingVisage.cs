using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class ModTerrifyingVisage : IModification
{
	public ModTerrifyingVisage()
	{
	}

	public ModTerrifyingVisage(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
		NameForStatus = "TerrifyingVisage";
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
		IncreaseDifficultyIfComplex(2);
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
		E.Actor.ModIntProperty("Horrifying", 1);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.ModIntProperty("Horrifying", -1, RemoveIfZero: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddWithClause("{{K|terrifying}} visage");
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
		return "Terrifying visage: This item reduces the cooldowns of Berate, Intimidate, and Menacing Stare by 10 rounds. It requires both the head and face to wear.";
	}
}

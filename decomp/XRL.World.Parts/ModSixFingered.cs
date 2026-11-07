using System;

namespace XRL.World.Parts;

[Serializable]
public class ModSixFingered : IModification
{
	public ModSixFingered()
	{
	}

	public ModSixFingered(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		if (Object.HasPart<Armor>())
		{
			Object.GetPart<Armor>().Agility++;
		}
		else
		{
			EquipStatBoost.AppendBoostOnEquip(Object, "Agility:1");
		}
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == PooledEvent<ShouldDescribeStatBonusEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ShouldDescribeStatBonusEvent E)
	{
		if (E.Component is Armor && E.Stat == "Agility")
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("{{G|six-fingered}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier, ParentObject));
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Six-Fingered: +1 Agility";
	}

	public static string GetDescription(int Tier, GameObject obj)
	{
		int value = Math.Max(obj.GetPart<Armor>()?.Agility ?? 1, 1);
		return "Six-Fingered: " + value.Signed() + " Agility";
	}
}

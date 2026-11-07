using System;

namespace XRL.World.Parts;

[Serializable]
public class ModReinforced : IModification
{
	public ModReinforced()
	{
	}

	public ModReinforced(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return IModification.CheckWornSlot(Object, "Body", "Back");
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<Armor>().AV++;
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
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
		if (E.Understood() && !E.Object.HasProperName && !E.Object.HasPart<ModOverbuilt>())
		{
			E.AddAdjective("reinforced");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!E.Object.HasPart<ModOverbuilt>())
		{
			E.Postfix.AppendRules(GetDescription(Tier));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("might", 1);
		}
		return base.HandleEvent(E);
	}

	public static string GetDescription(int Tier)
	{
		return "Reinforced: +1 AV";
	}
}

using System;

namespace XRL.World.Parts;

/// This part is not used in the base game.
[Serializable]
public class ModOverbuilt : IModification
{
	[NonSerialized]
	private bool HaveAddedWeight;

	[NonSerialized]
	private int AddedWeight;

	public ModOverbuilt()
	{
	}

	public ModOverbuilt(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override bool ModificationApplicable(GameObject Object)
	{
		return Object.HasPart<ModReinforced>();
	}

	public override void ApplyModification(GameObject Object)
	{
		Object.GetPart<Armor>().AV += 2;
		Object.GetPart<Armor>().DV -= 2;
		Object.GetPart<Armor>().SpeedPenalty += 10;
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetIntrinsicWeightEvent.ID && ID != PooledEvent<GetItemElementsEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Understood() && !E.Object.HasProperName)
		{
			E.AddAdjective("overbuilt");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetDescription(Tier));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetIntrinsicWeightEvent E)
	{
		E.Weight += GetAddedWeight();
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
		return "Overbuilt: +3 AV, -2 DV, -10 move speed, x2 weight";
	}

	public int GetAddedWeight()
	{
		if (!HaveAddedWeight)
		{
			try
			{
				AddedWeight = ParentObject.GetBlueprint().GetPartParameter("Physics", "Weight", 0);
			}
			catch
			{
			}
			if (AddedWeight < 2)
			{
				AddedWeight = 2;
			}
			HaveAddedWeight = true;
		}
		return AddedWeight;
	}
}

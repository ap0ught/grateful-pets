using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ModScaled : IModification
{
	public int Amount = 250;

	public ModScaled()
	{
	}

	public ModScaled(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		AddsRep.AddModifier(Object, "Unshelled Reptiles:" + Amount + ":hidden");
		IncreaseDifficultyAndComplexityIfComplex(1, 1);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModScaled).Amount != Amount)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction += 30;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (!E.Understood() || !E.Object.HasProperName)
		{
			E.AddAdjective("{{scaled|scaled}}");
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
		return "Scaled: This item grants the wearer reputation with unshelled reptiles.";
	}

	public string GetInstanceDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Scaled: This item grants the wearer ");
		if (Amount > 0)
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(Amount).Append(" reputation with unshelled reptiles.");
		return stringBuilder.ToString();
	}
}

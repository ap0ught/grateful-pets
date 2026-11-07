using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class ModSnailEncrusted : IModification
{
	public int Amount = 250;

	public ModSnailEncrusted()
	{
	}

	public ModSnailEncrusted(int Tier)
		: base(Tier)
	{
	}

	public override void Configure()
	{
		WorksOnSelf = true;
	}

	public override void ApplyModification(GameObject Object)
	{
		AddsRep.AddModifier(Object, "Mollusks:" + Amount + ":hidden");
		IncreaseDifficultyIfComplex(1);
	}

	public override bool SameAs(IPart p)
	{
		if ((p as ModSnailEncrusted).Amount != Amount)
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
			E.AddAdjective("{{snail-encrusted|snail-encrusted}}");
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
		return "Snail-Encrusted: This item is crawling with tiny snails and grants the wearer reputation with mollusks.";
	}

	public string GetInstanceDescription()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Snail-Encrusted: This item is crawling with tiny snails and grants the wearer ");
		if (Amount > 0)
		{
			stringBuilder.Append('+');
		}
		stringBuilder.Append(Amount).Append(" reputation with mollusks.");
		return stringBuilder.ToString();
	}
}

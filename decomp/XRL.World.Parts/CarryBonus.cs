using System;
using System.Text;

namespace XRL.World.Parts;

[Serializable]
public class CarryBonus : IActivePart
{
	public string Style = "Flat";

	public int Amount;

	public bool BonusApplied;

	public CarryBonus()
	{
		WorksOnEquipper = true;
	}

	public CarryBonus(int Amount, string Style = "Flat")
		: this()
	{
		this.Amount = Amount;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetMaxCarriedWeightEvent.ID && ID != GetShortDescriptionEvent.ID && (!WorksOnEquipper || ID != EquippedEvent.ID) && (!WorksOnImplantee || ID != ImplantedEvent.ID) && (!WorksOnEquipper || ID != UnequippedEvent.ID))
		{
			if (WorksOnImplantee)
			{
				return ID == UnimplantedEvent.ID;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetMaxCarriedWeightEvent E)
	{
		if (BonusApplied || (WorksOnSelf && E.Object == ParentObject))
		{
			if (Style == "Flat")
			{
				E.Weight += Amount;
			}
			else
			{
				E.AdjustWeight((double)(100 + Amount) / 100.0);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Amount > 0)
		{
			E.Postfix.AppendRules(AppendEffect);
		}
		return base.HandleEvent(E);
	}

	public void AppendEffect(StringBuilder SB)
	{
		SB.AppendSigned(Amount).Append((Style == "Flat") ? " lbs. carry capacity" : "% carry capacity");
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		BonusApplied = ParentObject.IsEquippedProperly(E.Part);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		BonusApplied = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ImplantedEvent E)
	{
		BonusApplied = ParentObject.IsEquippedProperly(E.Part);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnimplantedEvent E)
	{
		BonusApplied = false;
		return base.HandleEvent(E);
	}
}

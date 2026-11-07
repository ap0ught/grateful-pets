using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class HealMedication : IPart
{
	public string MutantHeal = "2d6";

	public string TrueManHeal = "4d6";

	public string Message = "The tube hisses against your skin, and a warm feeling washes over your body.";

	public override bool SameAs(IPart p)
	{
		HealMedication healMedication = p as HealMedication;
		if (healMedication.MutantHeal != MutantHeal)
		{
			return false;
		}
		if (healMedication.TrueManHeal != TrueManHeal)
		{
			return false;
		}
		if (healMedication.Message != Message)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Apply")
		{
			int amount = (E.Actor.IsTrueKin() ? TrueManHeal : MutantHeal).RollCached();
			E.Actor.Heal(amount, Message: false, FloatText: true, RandomMinimum: true);
			if (E.Actor.IsPlayer())
			{
				Popup.Show(Message);
			}
			ParentObject.Destroy();
		}
		return base.HandleEvent(E);
	}
}

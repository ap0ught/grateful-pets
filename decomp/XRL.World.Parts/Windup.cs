using System;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Windup : IPoweredPart
{
	public int ChargeRate = 1;

	public string ActionName = "Wind";

	public string ActionLabel = "&Ww&yind";

	public string ActionVerb = "wind";

	public string ActionKey = "w";

	public bool LastWindDidAnything;

	public Windup()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		Windup windup = p as Windup;
		if (windup.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (windup.ActionName != ActionName)
		{
			return false;
		}
		if (windup.ActionLabel != ActionLabel)
		{
			return false;
		}
		if (windup.ActionVerb != ActionVerb)
		{
			return false;
		}
		if (windup.ActionKey != ActionKey)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetPassiveAbilityListEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		if (Stat.Random(1, LastWindDidAnything ? 3 : 100) == 1)
		{
			E.Add("WindUp", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (WorksFor(IComponent<GameObject>.ThePlayer))
		{
			E.AddAction(ActionName, ActionLabel, "WindUp", null, ActionKey[0], FireOnActor: false, 0, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "WindUp")
		{
			if (IsDisabled(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You try to " + ActionVerb + " " + ParentObject.t() + ", but " + ParentObject.itis + " unresponsive.");
				}
				else if (E.Actor.IsVisible())
				{
					IComponent<GameObject>.AddPlayerMessage(E.Actor.Does("try") + " to " + ActionVerb + " " + ParentObject.an() + ", but " + ParentObject.itis + " unresponsive.");
				}
			}
			else
			{
				if (E.Actor.IsPlayer())
				{
					Popup.Show("You " + ActionVerb + " " + ParentObject.t() + ".");
				}
				else if (IComponent<GameObject>.Visible(E.Actor))
				{
					IComponent<GameObject>.AddPlayerMessage(E.Actor.Does(ActionVerb) + " " + ParentObject.an() + ".");
				}
				LastWindDidAnything = false;
				if (ParentObject.ChargeAvailable(ChargeRate, 0L) > 0)
				{
					LastWindDidAnything = true;
					ConsumeCharge();
				}
				E.Actor.UseEnergy(1000);
				E.RequestInterfaceExit();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

using System;
using UnityEngine;

namespace XRL.World.Parts;

[Serializable]
public class ActionChargeCost : IPart
{
	public int Percentage = 100;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<UseEnergyEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(UseEnergyEvent E)
	{
		if (!E.Passive)
		{
			int charge = Mathf.RoundToInt((float)E.Amount * ((float)Percentage / 100f));
			ParentObject.UseCharge(charge, LiveOnly: false, 0L);
		}
		return base.HandleEvent(E);
	}
}

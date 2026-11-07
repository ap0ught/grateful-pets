using System;
using System.Collections.Generic;
using System.Text;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class IntegratedPowerSystems : IPoweredPart
{
	public string RequiresPart;

	public string RequiresTagOrProperty;

	public string RequiresEvent;

	public string ChargeDisplayStyle;

	public string AltChargeDisplayStyle;

	public string AltChargeDisplayProperty = Scanning.GetScanPropertyName(Scanning.Scan.Tech);

	public string Description = "integrated power systems";

	public IntegratedPowerSystems()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnEquipper = true;
	}

	public override bool WorksForEveryone()
	{
		if (!RequiresPart.IsNullOrEmpty())
		{
			return false;
		}
		if (!RequiresTagOrProperty.IsNullOrEmpty())
		{
			return false;
		}
		if (!RequiresEvent.IsNullOrEmpty())
		{
			return false;
		}
		return base.WorksForEveryone();
	}

	public override bool WorksFor(GameObject obj)
	{
		if (!RequiresPart.IsNullOrEmpty() && (obj == null || !obj.HasPart(RequiresPart)))
		{
			return false;
		}
		if (!RequiresTagOrProperty.IsNullOrEmpty() && (obj == null || !obj.HasTagOrProperty(RequiresTagOrProperty)))
		{
			return false;
		}
		if (!RequiresEvent.IsNullOrEmpty() && (obj == null || obj.FireEvent(RequiresEvent)))
		{
			return false;
		}
		return base.WorksFor(obj);
	}

	public bool HasCharge(int Amount)
	{
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				if (activePartSubject.TestCharge(Amount, LiveOnly: false, 0L))
				{
					return true;
				}
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject.TestCharge(Amount, LiveOnly: false, 0L))
			{
				return true;
			}
		}
		return false;
	}

	public int GetCharge()
	{
		int num = 0;
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				num += activePartSubject.QueryCharge(LiveOnly: false, 0L);
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			num += activePartFirstSubject.QueryCharge(LiveOnly: false, 0L);
		}
		return num;
	}

	public void UseCharge(int Amount)
	{
		if (Amount <= 0)
		{
			return;
		}
		if (ActivePartHasMultipleSubjects())
		{
			List<GameObject> activePartSubjects = GetActivePartSubjects();
			if (activePartSubjects.Count == 0)
			{
				return;
			}
			bool flag = false;
			int i = 0;
			for (int count = activePartSubjects.Count; i < count; i++)
			{
				if (UsingChargeEvent.Wanted(activePartSubjects[i]))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				UsingChargeEvent usingChargeEvent = UsingChargeEvent.FromPool();
				usingChargeEvent.Source = ParentObject;
				usingChargeEvent.Amount = Amount;
				int j = 0;
				for (int count2 = activePartSubjects.Count; j < count2; j++)
				{
					UsingChargeEvent.Process(activePartSubjects[j], usingChargeEvent);
				}
			}
			bool flag2 = false;
			int k = 0;
			for (int count3 = activePartSubjects.Count; k < count3; k++)
			{
				if (UseChargeEvent.Wanted(activePartSubjects[k]))
				{
					flag2 = true;
					break;
				}
			}
			int amount = 0;
			if (flag2)
			{
				UseChargeEvent useChargeEvent = UseChargeEvent.FromPool();
				useChargeEvent.Source = ParentObject;
				useChargeEvent.Amount = Amount;
				int l = 0;
				for (int count4 = activePartSubjects.Count; l < count4; l++)
				{
					if (!UseChargeEvent.Process(activePartSubjects[l], useChargeEvent))
					{
						return;
					}
				}
				amount = Amount - useChargeEvent.Amount;
			}
			bool flag3 = false;
			int m = 0;
			for (int count5 = activePartSubjects.Count; m < count5; m++)
			{
				if (ChargeUsedEvent.Wanted(activePartSubjects[m]))
				{
					flag3 = true;
					break;
				}
			}
			if (flag3)
			{
				ChargeUsedEvent chargeUsedEvent = ChargeUsedEvent.FromPool();
				chargeUsedEvent.Source = ParentObject;
				chargeUsedEvent.DesiredAmount = Amount;
				chargeUsedEvent.Amount = amount;
				int n = 0;
				for (int count6 = activePartSubjects.Count; n < count6; n++)
				{
					ChargeUsedEvent.Process(activePartSubjects[n], chargeUsedEvent);
				}
			}
		}
		else
		{
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (UsingChargeEvent.Wanted(activePartFirstSubject))
			{
				UsingChargeEvent usingChargeEvent2 = UsingChargeEvent.FromPool();
				usingChargeEvent2.Source = ParentObject;
				usingChargeEvent2.Amount = Amount;
				UsingChargeEvent.Process(activePartFirstSubject, usingChargeEvent2);
			}
			int amount2 = 0;
			if (UseChargeEvent.Wanted(activePartFirstSubject))
			{
				UseChargeEvent useChargeEvent2 = UseChargeEvent.FromPool();
				useChargeEvent2.Source = ParentObject;
				useChargeEvent2.Amount = Amount;
				UseChargeEvent.Process(activePartFirstSubject, useChargeEvent2);
				amount2 = Amount - useChargeEvent2.Amount;
			}
			if (ChargeUsedEvent.Wanted(activePartFirstSubject))
			{
				ChargeUsedEvent chargeUsedEvent2 = ChargeUsedEvent.FromPool();
				chargeUsedEvent2.Source = ParentObject;
				chargeUsedEvent2.DesiredAmount = Amount;
				chargeUsedEvent2.Amount = amount2;
				ChargeUsedEvent.Process(activePartFirstSubject, chargeUsedEvent2);
			}
		}
	}

	public void AddCharge(int Amount)
	{
		if (Amount <= 0)
		{
			return;
		}
		if (ActivePartHasMultipleSubjects())
		{
			foreach (GameObject activePartSubject in GetActivePartSubjects())
			{
				int num = activePartSubject.ChargeAvailable(Amount, 0L);
				if (num > 0)
				{
					Amount -= num;
					if (Amount <= 0)
					{
						break;
					}
				}
			}
			return;
		}
		GameObject activePartFirstSubject = GetActivePartFirstSubject();
		activePartFirstSubject.ChargeAvailable(Amount, 0L);
	}

	public int GetChargeLevel()
	{
		return EnergyStorage.GetChargeLevel(GetCharge(), 0);
	}

	public bool UseAltChargeDisplayStyle()
	{
		if (IComponent<GameObject>.ThePlayer == null)
		{
			return false;
		}
		if (AltChargeDisplayProperty.IsNullOrEmpty())
		{
			return false;
		}
		if (IComponent<GameObject>.ThePlayer.GetIntProperty(AltChargeDisplayProperty) <= 0)
		{
			return false;
		}
		return true;
	}

	public string ChargeStatus()
	{
		string text = (UseAltChargeDisplayStyle() ? AltChargeDisplayStyle : ChargeDisplayStyle);
		if (text.IsNullOrEmpty())
		{
			return null;
		}
		if (IsDisabled(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return null;
		}
		return EnergyStorage.GetChargeStatus(GetCharge(), 0, text);
	}

	public override bool SameAs(IPart p)
	{
		IntegratedPowerSystems integratedPowerSystems = p as IntegratedPowerSystems;
		if (integratedPowerSystems.RequiresPart != RequiresPart)
		{
			return false;
		}
		if (integratedPowerSystems.RequiresTagOrProperty != RequiresTagOrProperty)
		{
			return false;
		}
		if (integratedPowerSystems.RequiresEvent != RequiresEvent)
		{
			return false;
		}
		if (integratedPowerSystems.ChargeDisplayStyle != ChargeDisplayStyle)
		{
			return false;
		}
		if (integratedPowerSystems.AltChargeDisplayStyle != AltChargeDisplayStyle)
		{
			return false;
		}
		if (integratedPowerSystems.AltChargeDisplayProperty != AltChargeDisplayProperty)
		{
			return false;
		}
		if (integratedPowerSystems.Description != Description)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != ChargeAvailableEvent.ID && ID != FinishChargeAvailableEvent.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != QueryChargeEvent.ID && ID != QueryChargeStorageEvent.ID && ID != PooledEvent<QueryDrawEvent>.ID && ID != TestChargeEvent.ID)
		{
			return ID == UseChargeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				List<GameObject> activePartSubjects = GetActivePartSubjects();
				int i = 0;
				for (int count = activePartSubjects.Count; i < count; i++)
				{
					if (!ChargeAvailableEvent.Process(activePartSubjects[i], E))
					{
						return false;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (!ChargeAvailableEvent.Process(activePartFirstSubject, E))
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FinishChargeAvailableEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				List<GameObject> activePartSubjects = GetActivePartSubjects();
				int i = 0;
				for (int count = activePartSubjects.Count; i < count; i++)
				{
					if (!FinishChargeAvailableEvent.Process(activePartSubjects[i], E))
					{
						return false;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (!FinishChargeAvailableEvent.Process(activePartFirstSubject, E))
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				List<GameObject> activePartSubjects = GetActivePartSubjects();
				int i = 0;
				for (int count = activePartSubjects.Count; i < count; i++)
				{
					if (!QueryChargeEvent.Process(activePartSubjects[i], E))
					{
						return false;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (!QueryChargeEvent.Process(activePartFirstSubject, E))
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				List<GameObject> activePartSubjects = GetActivePartSubjects();
				int i = 0;
				for (int count = activePartSubjects.Count; i < count; i++)
				{
					if (!TestChargeEvent.Process(activePartSubjects[i], E))
					{
						return false;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (!TestChargeEvent.Process(activePartFirstSubject, E))
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			if (ActivePartHasMultipleSubjects())
			{
				List<GameObject> activePartSubjects = GetActivePartSubjects();
				int i = 0;
				for (int count = activePartSubjects.Count; i < count; i++)
				{
					if (!UseChargeEvent.Process(activePartSubjects[i], E))
					{
						return false;
					}
				}
			}
			else
			{
				GameObject activePartFirstSubject = GetActivePartFirstSubject();
				if (!UseChargeEvent.Process(activePartFirstSubject, E))
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Context != "Tinkering" && E.Understood())
		{
			string text = ChargeStatus();
			if (text != null)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("{{y|(").Append(Description).Append(": ")
					.Append(text)
					.Append(")}}");
				E.AddTag(stringBuilder.ToString());
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryDrawEvent E)
	{
		GameObject gameObject = E.Object;
		if (gameObject == ParentObject.Equipped && IsObjectActive(gameObject))
		{
			try
			{
				E.Object = ParentObject;
				ParentObject.HandleEvent(E);
			}
			finally
			{
				E.Object = gameObject;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeStorageEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L) && AnyActivePartSubjectWantsEvent(E.GetID(), E.GetCascadeLevel()))
		{
			ActivePartSubjectsHandleEvent(E);
		}
		return base.HandleEvent(E);
	}

	private bool IsObjectActive(GameObject obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj.TryGetPart<BootSequence>(out var Part) && Part.BootTimeLeft > 0)
		{
			return false;
		}
		if (obj.TryGetPart<PowerSwitch>(out var Part2) && !Part2.Active)
		{
			return false;
		}
		if (obj.TryGetPart<ArtificialIntelligence>(out var Part3) && !Part3.IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return true;
	}
}

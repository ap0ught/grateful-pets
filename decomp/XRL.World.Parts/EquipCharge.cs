using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class EquipCharge : IPoweredPart
{
	public int ChargeRate = 1;

	public string EquipperProperty;

	public bool Describe;

	public string PropertyConditionDescription;

	public bool AlternateChargeRateInCombat;

	public int CombatChargeRate = 2;

	public EquipCharge()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnHolder = true;
		WorksOnWearer = true;
	}

	public override bool SameAs(IPart p)
	{
		EquipCharge equipCharge = p as EquipCharge;
		if (equipCharge.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (equipCharge.EquipperProperty != EquipperProperty)
		{
			return false;
		}
		if (equipCharge.Describe != Describe)
		{
			return false;
		}
		if (equipCharge.PropertyConditionDescription != PropertyConditionDescription)
		{
			return false;
		}
		if (equipCharge.AlternateChargeRateInCombat != AlternateChargeRateInCombat)
		{
			return false;
		}
		if (equipCharge.CombatChargeRate != CombatChargeRate)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetPassiveItemListEvent.ID && ID != GetShortDescriptionEvent.ID && ID != QueryChargeProductionEvent.ID && ID != QueryEquippableListEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Describe)
		{
			StringBuilder postfix = E.Postfix;
			postfix.Append("\n{{rules|Regains charge when ");
			List<string> list = new List<string>();
			if (WorksOnWearer)
			{
				list.Add("worn");
			}
			if (WorksOnHolder)
			{
				list.Add("held in hand");
			}
			if (WorksOnCarrier)
			{
				list.Add("carried");
			}
			if (WorksOnImplantee)
			{
				list.Add("implanted");
			}
			if (WorksOnCellContents || WorksOnAdjacentCellContents)
			{
				list.Add("objects are nearby");
			}
			if (WorksOnEnclosed)
			{
				list.Add("enclosing something");
			}
			if (WorksOnInventory)
			{
				list.Add("containing inventory");
			}
			if (WorksOnSelf)
			{
				list.Add("existing");
			}
			if (list.Count > 0)
			{
				postfix.Append(Grammar.MakeOrList(list));
			}
			else
			{
				postfix.Append("used in some indeterminate fashion");
			}
			if (EquipperProperty != null)
			{
				if (PropertyConditionDescription != null)
				{
					postfix.Append(' ').Append(PropertyConditionDescription);
				}
				else
				{
					postfix.Append(" by someone ").Append(EquipperProperty);
				}
			}
			if (AlternateChargeRateInCombat)
			{
				if (ChargeRate <= 0 && CombatChargeRate > 0)
				{
					postfix.Append(" in combat");
				}
				else if (ChargeRate > 0 && CombatChargeRate <= 0)
				{
					postfix.Append(" not in combat");
				}
				else if (ChargeRate < CombatChargeRate)
				{
					if (ChargeRate < CombatChargeRate / 2)
					{
						postfix.Append(", much more quickly while in combat");
					}
					else
					{
						postfix.Append(", more quickly while in combat");
					}
				}
				else if (ChargeRate > CombatChargeRate)
				{
					if (ChargeRate > CombatChargeRate * 2)
					{
						postfix.Append(", much less quickly while in combat");
					}
					else
					{
						postfix.Append(", less quickly while in combat");
					}
				}
			}
			postfix.Append(".}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeProductionEvent E)
	{
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			int num = ((AlternateChargeRateInCombat && !ForeachActivePartSubjectWhile(CheckCombat)) ? CombatChargeRate : ChargeRate);
			if (!EquipperProperty.IsNullOrEmpty())
			{
				int intProperty = ParentObject.Equipped.GetIntProperty(EquipperProperty);
				if (num > intProperty)
				{
					num = intProperty;
				}
			}
			if (ChargeUse > 0)
			{
				num -= ChargeUse;
			}
			if (num > 0)
			{
				E.Amount += num * E.Multiple;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryEquippableListEvent E)
	{
		if (E.RequireDesirable && E.Item == ParentObject && ParentObject.QueryChargeStorage() <= 0)
		{
			E.List.Remove(ParentObject);
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetPassiveItemListEvent E)
	{
		if (ParentObject.QueryChargeStorage() <= 0)
		{
			E.Add("CommandUnequipObject", 1, null, Inv: false, Self: false, ParentObject);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		if (E.Actor != null && !E.Actor.IsPlayer() && E.Actor.Brain != null && ParentObject.QueryChargeStorage() > 0)
		{
			E.Actor.Brain.DoReequip = true;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		PerformCharging(Amount, CombatPossible: true);
	}

	private void PerformCharging(int Turns = 1, bool CombatPossible = false)
	{
		if (!IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		int num = ((CombatPossible && AlternateChargeRateInCombat && !ForeachActivePartSubjectWhile(CheckCombat)) ? CombatChargeRate : ChargeRate);
		if (!EquipperProperty.IsNullOrEmpty())
		{
			int intProperty = ParentObject.Equipped.GetIntProperty(EquipperProperty);
			if (num > intProperty)
			{
				num = intProperty;
			}
		}
		if (ChargeUse > 0)
		{
			num -= ChargeUse;
		}
		if (num > 0)
		{
			ParentObject.ChargeAvailable(num, 0L, Turns);
		}
	}

	public static bool CheckCombat(GameObject obj)
	{
		if (obj.OnWorldMap() || !obj.AreHostilesNearby())
		{
			return true;
		}
		return false;
	}
}

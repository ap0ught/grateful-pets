using System;
using System.Collections.Generic;
using XRL.Language;

namespace XRL.World.Parts;

/// <remarks>
/// overload behavior: if <see cref="!:IsPowerLoadSensitive" /> is true,
/// which it is by default, a chance to take effect that is below 100
/// will be increased by ((power load - 100) / 30), i.e. 10 for the
/// standard overload power load of 400.
/// </remarks>
[Serializable]
public class PartsGas : IPoweredPart
{
	public int Radius = 1;

	public int Chance = 100;

	public bool UsesChargePerTurn;

	public bool UsesChargePerEachEffect;

	public bool UsesChargePerAnyEffect;

	public bool UsesCircularRadius;

	public bool ShowInShortDescription = true;

	public PartsGas()
	{
		NameForStatus = "GasDispersal";
	}

	public override bool SameAs(IPart p)
	{
		PartsGas partsGas = p as PartsGas;
		if (partsGas.Radius != Radius)
		{
			return false;
		}
		if (partsGas.Chance != Chance)
		{
			return false;
		}
		if (partsGas.UsesChargePerTurn != UsesChargePerTurn)
		{
			return false;
		}
		if (partsGas.UsesChargePerEachEffect != UsesChargePerEachEffect)
		{
			return false;
		}
		if (partsGas.UsesChargePerAnyEffect != UsesChargePerAnyEffect)
		{
			return false;
		}
		if (partsGas.UsesCircularRadius != UsesCircularRadius)
		{
			return false;
		}
		if (partsGas.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return ShowInShortDescription;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription)
		{
			E.Postfix.Append("\n{{rules|");
			int effectiveChance = GetEffectiveChance();
			if (effectiveChance >= 100)
			{
				E.Postfix.Append("Repels");
			}
			else
			{
				E.Postfix.Append(effectiveChance).Append("% chance per turn to repel");
			}
			E.Postfix.Append(" gases ");
			if (Radius == 1)
			{
				E.Postfix.Append("near ");
			}
			else if (UsesCircularRadius)
			{
				E.Postfix.Append("within ").Append(Grammar.A(Radius)).Append("-square radius of ");
			}
			else
			{
				E.Postfix.Append("within ").Append(Radius).Append(" squares of ");
			}
			E.Postfix.Append(GetOperationalScopeDescription()).Append('.');
			AddStatusSummary(E.Postfix);
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		int num = MyPowerLoadLevel();
		bool usesChargePerTurn = UsesChargePerTurn;
		int? powerLoadLevel = num;
		if (IsReady(usesChargePerTurn, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
		{
			PerformGasRepulsion(null, num);
		}
		base.TurnTick(TimeTick, Amount);
	}

	public void PerformGasRepulsion(Cell FromCell = null, int PowerLoad = int.MinValue)
	{
		if (FromCell == null)
		{
			FromCell = ParentObject.GetCurrentCell();
		}
		if (FromCell == null || FromCell.OnWorldMap())
		{
			return;
		}
		List<Cell> localAdjacentCells = FromCell.GetLocalAdjacentCells(Radius);
		localAdjacentCells.Remove(FromCell);
		localAdjacentCells.Insert(0, FromCell);
		List<GameObject> list = Event.NewGameObjectList();
		bool flag = !UsesChargePerAnyEffect;
		if (PowerLoad == int.MinValue)
		{
			PowerLoad = MyPowerLoadLevel();
		}
		int effectiveChance = GetEffectiveChance(PowerLoad);
		int i = 0;
		for (int count = localAdjacentCells.Count; i < count; i++)
		{
			Cell cell = localAdjacentCells[i];
			list.Clear();
			int j = 0;
			for (int count2 = cell.Objects.Count; j < count2; j++)
			{
				if (!cell.Objects[j].HasPart<Gas>() || !effectiveChance.in100())
				{
					continue;
				}
				if (!flag)
				{
					int? powerLoadLevel = PowerLoad;
					if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
					{
						return;
					}
					flag = true;
				}
				list.Add(cell.Objects[j]);
			}
			if (list.Count <= 0)
			{
				continue;
			}
			string directionFromCell = FromCell.GetDirectionFromCell(cell);
			Cell cell2 = cell.GetCellFromDirection(directionFromCell);
			if (cell2 == null || localAdjacentCells.Contains(cell2))
			{
				cell2 = cell.getClosestPassableCellExcept(localAdjacentCells);
			}
			if (cell2 == null || localAdjacentCells.Contains(cell2))
			{
				continue;
			}
			int k = 0;
			for (int count3 = list.Count; k < count3; k++)
			{
				if (UsesChargePerEachEffect)
				{
					int? powerLoadLevel = PowerLoad;
					if (!IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L, powerLoadLevel))
					{
						continue;
					}
				}
				list[k].DirectMoveTo(cell2, 0, Forced: true);
			}
		}
	}

	public int GetEffectiveChance()
	{
		int num = Chance;
		if (num < 100)
		{
			num += MyPowerLoadBonus(int.MinValue, 100, 30);
		}
		return num;
	}

	public int GetEffectiveChance(int PowerLoad)
	{
		int num = Chance;
		if (num < 100)
		{
			num += MyPowerLoadBonus(PowerLoad, 100, 30);
		}
		return num;
	}
}

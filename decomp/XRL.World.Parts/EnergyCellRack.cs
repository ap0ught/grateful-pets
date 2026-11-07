using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class EnergyCellRack : IPoweredPart
{
	public string SlotType = "EnergyCell";

	public bool PreventOther = true;

	public EnergyCellRack()
	{
		ChargeUse = 0;
		IsBootSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		EnergyCellRack energyCellRack = p as EnergyCellRack;
		if (energyCellRack.SlotType != SlotType)
		{
			return false;
		}
		if (energyCellRack.PreventOther != PreventOther)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != PooledEvent<CanAcceptObjectEvent>.ID || !PreventOther) && ID != SingletonEvent<EndTurnEvent>.ID && ID != PooledEvent<GetContentsEvent>.ID && ID != PooledEvent<GetDisplayNameEvent>.ID && ID != GetExtrinsicValueEvent.ID && ID != GetExtrinsicWeightEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID && ID != QueryChargeEvent.ID && ID != TestChargeEvent.ID)
		{
			return ID == UseChargeEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		long gridMask = E.GridMask;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory?.Objects != null)
			{
				int i = 0;
				for (int count = inventory.Objects.Count; i < count; i++)
				{
					GameObject gameObject = inventory.Objects[i];
					int j = 0;
					for (int count2 = gameObject.PartsList.Count; j < count2; j++)
					{
						if (gameObject.PartsList[j] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
						{
							int num = gameObject.QueryCharge(LiveOnly: false, IncludeTransient: E.IncludeTransient, IncludeBiological: E.IncludeBiological, GridMask: E.GridMask);
							if (num > 0)
							{
								E.Amount += num;
							}
							break;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		long gridMask = E.GridMask;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory?.Objects != null)
			{
				int i = 0;
				for (int count = inventory.Objects.Count; i < count; i++)
				{
					GameObject gameObject = inventory.Objects[i];
					int j = 0;
					for (int count2 = gameObject.PartsList.Count; j < count2; j++)
					{
						if (gameObject.PartsList[j] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
						{
							int num = Math.Min(E.Amount, gameObject.QueryCharge(LiveOnly: false, IncludeTransient: E.IncludeTransient, IncludeBiological: E.IncludeBiological, GridMask: E.GridMask) - ChargeUse * E.Multiple);
							if (num <= 0)
							{
								break;
							}
							E.Amount -= num;
							if (E.Amount > 0)
							{
								break;
							}
							return false;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		long gridMask = E.GridMask;
		if (IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
		{
			Inventory inventory = ParentObject.Inventory;
			if (inventory?.Objects != null)
			{
				int i = 0;
				for (int count = inventory.Objects.Count; i < count; i++)
				{
					GameObject gameObject = inventory.Objects[i];
					int j = 0;
					for (int count2 = gameObject.PartsList.Count; j < count2; j++)
					{
						if (gameObject.PartsList[j] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
						{
							int num = ChargeUse * E.Multiple;
							int num2 = Math.Min(E.Amount, gameObject.QueryCharge(LiveOnly: false, IncludeTransient: E.IncludeTransient, IncludeBiological: E.IncludeBiological, GridMask: E.GridMask) - num);
							if (num2 <= 0)
							{
								break;
							}
							gameObject.UseCharge(num2 + num, LiveOnly: false, E.GridMask);
							E.Amount -= num2;
							if (E.Amount > 0)
							{
								break;
							}
							return false;
						}
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanAcceptObjectEvent E)
	{
		if (PreventOther)
		{
			bool flag = false;
			int i = 0;
			for (int count = E.Object.PartsList.Count; i < count; i++)
			{
				if (E.Object.PartsList[i] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDisplayNameEvent E)
	{
		if (E.Context != "Tinkering" && E.Understood())
		{
			int cellCount = GetCellCount();
			switch (cellCount)
			{
			case 0:
				E.AddTag("{{y|[{{K|no cells}}]}}", -5);
				break;
			case 1:
				E.AddTag("{{y|[1 cell]}}", -5);
				break;
			default:
				E.AddTag("{{y|[" + cellCount + " cells]}}");
				break;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public List<GameObject> GetCells()
	{
		List<GameObject> list = Event.NewGameObjectList();
		Inventory inventory = ParentObject.Inventory;
		if (inventory?.Objects != null)
		{
			int i = 0;
			for (int count = inventory.Objects.Count; i < count; i++)
			{
				GameObject gameObject = inventory.Objects[i];
				int j = 0;
				for (int count2 = gameObject.PartsList.Count; j < count2; j++)
				{
					if (gameObject.PartsList[j] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
					{
						list.Add(gameObject);
					}
				}
			}
		}
		return list;
	}

	public int GetCellCount()
	{
		int num = 0;
		Inventory inventory = ParentObject.Inventory;
		if (inventory?.Objects != null)
		{
			int i = 0;
			for (int count = inventory.Objects.Count; i < count; i++)
			{
				GameObject gameObject = inventory.Objects[i];
				int j = 0;
				for (int count2 = gameObject.PartsList.Count; j < count2; j++)
				{
					if (gameObject.PartsList[j] is IEnergyCell energyCell && energyCell.SlotType == SlotType)
					{
						num += gameObject.Count;
					}
				}
			}
		}
		return num;
	}
}

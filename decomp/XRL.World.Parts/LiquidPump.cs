using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LiquidPump : IPoweredPart
{
	public int Tick;

	public int Rate = 1000;

	public string Liquid;

	public string VariableRate;

	public string PerTick;

	public string FromDirection;

	public string ToDirection;

	public bool PreferCollectors;

	public bool PureOnFloor;

	public bool FillSelfOnly;

	public bool StickyFromDirection;

	public bool StickyToDirection;

	[NonSerialized]
	private static LiquidVolume pumped = new LiquidVolume();

	public LiquidPump()
	{
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		LiquidPump liquidPump = p as LiquidPump;
		if (liquidPump.Rate != Rate)
		{
			return false;
		}
		if (liquidPump.Liquid != Liquid)
		{
			return false;
		}
		if (liquidPump.VariableRate != VariableRate)
		{
			return false;
		}
		if (liquidPump.PerTick != PerTick)
		{
			return false;
		}
		if (liquidPump.FromDirection != FromDirection)
		{
			return false;
		}
		if (liquidPump.ToDirection != ToDirection)
		{
			return false;
		}
		if (liquidPump.PreferCollectors != PreferCollectors)
		{
			return false;
		}
		if (liquidPump.PureOnFloor != PureOnFloor)
		{
			return false;
		}
		if (liquidPump.FillSelfOnly != FillSelfOnly)
		{
			return false;
		}
		if (liquidPump.StickyFromDirection != StickyFromDirection)
		{
			return false;
		}
		if (liquidPump.StickyToDirection != StickyToDirection)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AllowLiquidCollectionEvent.ID || string.IsNullOrEmpty(Liquid)) && (ID != GetPreferredLiquidEvent.ID || string.IsNullOrEmpty(Liquid)) && ID != ObjectCreatedEvent.ID)
		{
			if (ID == WantsLiquidCollectionEvent.ID)
			{
				return !string.IsNullOrEmpty(Liquid);
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AllowLiquidCollectionEvent E)
	{
		if (!IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetPreferredLiquidEvent E)
	{
		if (E.Liquid == null)
		{
			E.Liquid = Liquid;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(WantsLiquidCollectionEvent E)
	{
		if (IsLiquidCollectionCompatible(E.Liquid))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		if (!string.IsNullOrEmpty(VariableRate))
		{
			Rate = VariableRate.RollCached();
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!IsNeeded() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Amount, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		if (string.IsNullOrEmpty(PerTick))
		{
			Tick += Amount;
		}
		else
		{
			DieRoll cachedDieRoll = PerTick.GetCachedDieRoll();
			for (int i = 0; i < Amount; i++)
			{
				Tick += cachedDieRoll.Resolve();
			}
		}
		while (Tick >= Rate)
		{
			Tick -= Rate;
			if (!DistributeLiquid())
			{
				Tick = 0;
				break;
			}
			if (!string.IsNullOrEmpty(VariableRate))
			{
				Rate = VariableRate.RollCached();
			}
		}
	}

	private LiquidVolume GetPumpableLiquidVolume(Cell C, int Phase)
	{
		if (C.IsSolid(ForFluid: true, Phase))
		{
			return null;
		}
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			LiquidVolume liquidVolume = C.Objects[i].LiquidVolume;
			if (liquidVolume != null && liquidVolume.MaxVolume == -1 && (string.IsNullOrEmpty(Liquid) || liquidVolume.IsPureLiquid(Liquid)))
			{
				return liquidVolume;
			}
		}
		return null;
	}

	private LiquidVolume GetPumpableLiquidVolume(Cell C)
	{
		return GetPumpableLiquidVolume(C, ParentObject.GetPhase());
	}

	private bool HasPumpableLiquid(Cell C, int Phase)
	{
		return GetPumpableLiquidVolume(C, Phase) != null;
	}

	private bool HasPumpableLiquid(Cell C)
	{
		return HasPumpableLiquid(C, ParentObject.GetPhase());
	}

	private LiquidVolume GetLiquidVolumeToPumpFrom(ref Cell CC, int Phase)
	{
		if (CC == null)
		{
			CC = ParentObject.GetCurrentCell();
			if (CC == null)
			{
				return null;
			}
		}
		if (!string.IsNullOrEmpty(FromDirection))
		{
			Cell cellFromDirection = CC.GetCellFromDirection(FromDirection);
			if (cellFromDirection != null)
			{
				LiquidVolume pumpableLiquidVolume = GetPumpableLiquidVolume(cellFromDirection, Phase);
				if (pumpableLiquidVolume != null)
				{
					return pumpableLiquidVolume;
				}
			}
		}
		else
		{
			List<Cell> localAdjacentCells = CC.GetLocalAdjacentCells();
			List<Cell> list = Event.NewCellList();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				if (HasPumpableLiquid(localAdjacentCells[i], Phase))
				{
					list.Add(localAdjacentCells[i]);
				}
			}
			Cell randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				LiquidVolume pumpableLiquidVolume2 = GetPumpableLiquidVolume(randomElement, Phase);
				if (pumpableLiquidVolume2 != null)
				{
					if (StickyFromDirection)
					{
						FromDirection = CC.GetDirectionFromCell(randomElement);
					}
					return pumpableLiquidVolume2;
				}
			}
		}
		return null;
	}

	private LiquidVolume GetLiquidVolumeToPumpFrom(Cell CC, int Phase)
	{
		return GetLiquidVolumeToPumpFrom(ref CC, Phase);
	}

	private bool HasSomeplaceToPumpFrom(ref Cell CC, int Phase)
	{
		if (CC == null)
		{
			CC = ParentObject.GetCurrentCell();
			if (CC == null)
			{
				return false;
			}
		}
		if (!string.IsNullOrEmpty(FromDirection))
		{
			Cell cellFromDirection = CC.GetCellFromDirection(FromDirection);
			if (cellFromDirection != null && HasPumpableLiquid(cellFromDirection, Phase))
			{
				return true;
			}
		}
		else
		{
			List<Cell> localAdjacentCells = CC.GetLocalAdjacentCells();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				if (HasPumpableLiquid(localAdjacentCells[i], Phase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasSomeplaceToPumpFrom(Cell CC, int Phase)
	{
		return HasSomeplaceToPumpFrom(ref CC, Phase);
	}

	private bool HasSomeplaceToPumpFrom(Cell CC)
	{
		return HasSomeplaceToPumpFrom(ref CC, ParentObject.GetPhase());
	}

	private bool HasSomeplaceToPumpFrom()
	{
		Cell cC = null;
		return HasSomeplaceToPumpFrom(cC);
	}

	private bool CanPumpTo(Cell C, int Phase)
	{
		if (C == null)
		{
			return false;
		}
		bool flag = false;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.ConsiderSolid(ForFluid: true, Phase))
			{
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null && liquidVolume.Collector && !liquidVolume.EffectivelySealed())
				{
					return true;
				}
				flag = true;
			}
		}
		return !flag;
	}

	private bool CanPumpTo(Cell C)
	{
		return CanPumpTo(C, ParentObject.GetPhase());
	}

	private LiquidVolume GetDestinationLiquidVolume(Cell C, int Phase)
	{
		bool flag = false;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.ConsiderSolid(ForFluid: true, Phase))
			{
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null && liquidVolume.Collector && !liquidVolume.EffectivelySealed())
				{
					return liquidVolume;
				}
				flag = true;
			}
		}
		if (flag)
		{
			return null;
		}
		int j = 0;
		for (int count2 = C.Objects.Count; j < count2; j++)
		{
			LiquidVolume liquidVolume2 = C.Objects[j].LiquidVolume;
			if (liquidVolume2 != null && liquidVolume2.Collector && !liquidVolume2.EffectivelySealed())
			{
				return liquidVolume2;
			}
		}
		int k = 0;
		for (int count3 = C.Objects.Count; k < count3; k++)
		{
			LiquidVolume liquidVolume3 = C.Objects[k].LiquidVolume;
			if (liquidVolume3 != null && liquidVolume3.MaxVolume == -1)
			{
				return liquidVolume3;
			}
		}
		return null;
	}

	private LiquidVolume GetDestinationLiquidVolume(Cell C)
	{
		return GetDestinationLiquidVolume(C, ParentObject.GetPhase());
	}

	private LiquidVolume GetDestinationCollectorLiquidVolume(Cell C, int Phase)
	{
		bool flag = false;
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.ConsiderSolid(ForFluid: true, Phase))
			{
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				if (liquidVolume != null && liquidVolume.Collector && !liquidVolume.EffectivelySealed())
				{
					return liquidVolume;
				}
				flag = true;
			}
		}
		if (flag)
		{
			return null;
		}
		int j = 0;
		for (int count2 = C.Objects.Count; j < count2; j++)
		{
			LiquidVolume liquidVolume2 = C.Objects[j].LiquidVolume;
			if (liquidVolume2 != null && liquidVolume2.Collector && !liquidVolume2.EffectivelySealed())
			{
				return liquidVolume2;
			}
		}
		return null;
	}

	private LiquidVolume GetDestinationCollectorLiquidVolume(Cell C)
	{
		return GetDestinationCollectorLiquidVolume(C, ParentObject.GetPhase());
	}

	private LiquidVolume GetLiquidVolumeToPumpTo(ref Cell CC, int Phase)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && (liquidVolume.MaxVolume == -1 || liquidVolume.Volume < liquidVolume.MaxVolume) && (string.IsNullOrEmpty(Liquid) || liquidVolume.IsPureLiquid(Liquid)))
		{
			return liquidVolume;
		}
		if (FillSelfOnly)
		{
			return null;
		}
		if (CC == null)
		{
			CC = ParentObject.GetCurrentCell();
			if (CC == null)
			{
				return null;
			}
		}
		if (!string.IsNullOrEmpty(ToDirection))
		{
			Cell cellFromDirection = CC.GetCellFromDirection(ToDirection);
			if (cellFromDirection != null)
			{
				LiquidVolume destinationLiquidVolume = GetDestinationLiquidVolume(cellFromDirection, Phase);
				if (destinationLiquidVolume != null)
				{
					return destinationLiquidVolume;
				}
			}
		}
		else
		{
			List<Cell> localAdjacentCells = CC.GetLocalAdjacentCells();
			if (PreferCollectors)
			{
				List<Cell> list = Event.NewCellList();
				int i = 0;
				for (int count = localAdjacentCells.Count; i < count; i++)
				{
					if (GetDestinationCollectorLiquidVolume(localAdjacentCells[i], Phase) != null)
					{
						list.Add(localAdjacentCells[i]);
					}
				}
				Cell randomElement = list.GetRandomElement();
				if (randomElement != null)
				{
					LiquidVolume destinationCollectorLiquidVolume = GetDestinationCollectorLiquidVolume(randomElement);
					if (destinationCollectorLiquidVolume != null)
					{
						if (StickyToDirection)
						{
							ToDirection = CC.GetDirectionFromCell(randomElement);
						}
						return destinationCollectorLiquidVolume;
					}
				}
			}
			List<Cell> list2 = Event.NewCellList();
			int j = 0;
			for (int count2 = localAdjacentCells.Count; j < count2; j++)
			{
				if (CanPumpTo(localAdjacentCells[j], Phase))
				{
					list2.Add(localAdjacentCells[j]);
				}
			}
			Cell randomElement2 = list2.GetRandomElement();
			if (randomElement2 != null)
			{
				if (StickyToDirection)
				{
					ToDirection = CC.GetDirectionFromCell(randomElement2);
				}
				return GetDestinationLiquidVolume(randomElement2, Phase);
			}
		}
		return null;
	}

	private LiquidVolume GetLiquidVolumeToPumpTo(Cell CC, int Phase)
	{
		return GetLiquidVolumeToPumpTo(ref CC, Phase);
	}

	private Cell GetCellToPumpTo(ref Cell CC, int Phase)
	{
		if (FillSelfOnly)
		{
			return null;
		}
		if (CC == null)
		{
			CC = ParentObject.GetCurrentCell();
			if (CC == null)
			{
				return null;
			}
		}
		if (!string.IsNullOrEmpty(ToDirection))
		{
			Cell cellFromDirection = CC.GetCellFromDirection(ToDirection);
			if (cellFromDirection != null)
			{
				return cellFromDirection;
			}
		}
		else
		{
			List<Cell> localAdjacentCells = CC.GetLocalAdjacentCells();
			List<Cell> list = Event.NewCellList();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				if (!localAdjacentCells[i].IsSolid(ForFluid: true, Phase))
				{
					list.Add(localAdjacentCells[i]);
				}
			}
			Cell randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				if (StickyToDirection)
				{
					ToDirection = CC.GetDirectionFromCell(randomElement);
				}
				return randomElement;
			}
		}
		return null;
	}

	private Cell GetCellToPumpTo(Cell CC, int Phase)
	{
		return GetCellToPumpTo(ref CC, Phase);
	}

	private bool HasSomeplaceToPumpTo(ref Cell CC, int Phase)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && (liquidVolume.MaxVolume == -1 || liquidVolume.Volume < liquidVolume.MaxVolume) && (string.IsNullOrEmpty(Liquid) || liquidVolume.IsPureLiquid(Liquid)))
		{
			return true;
		}
		if (FillSelfOnly)
		{
			return false;
		}
		if (CC == null)
		{
			CC = ParentObject.GetCurrentCell();
			if (CC == null)
			{
				return false;
			}
		}
		if (!string.IsNullOrEmpty(ToDirection))
		{
			Cell cellFromDirection = CC.GetCellFromDirection(ToDirection);
			if (CanPumpTo(cellFromDirection, Phase))
			{
				return true;
			}
		}
		else
		{
			List<Cell> localAdjacentCells = CC.GetLocalAdjacentCells();
			int i = 0;
			for (int count = localAdjacentCells.Count; i < count; i++)
			{
				if (CanPumpTo(localAdjacentCells[i], Phase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasSomeplaceToPumpTo(Cell CC, int Phase)
	{
		return HasSomeplaceToPumpTo(ref CC, Phase);
	}

	private bool HasSomeplaceToPumpTo(Cell CC)
	{
		return HasSomeplaceToPumpTo(ref CC, ParentObject.GetPhase());
	}

	private bool HasSomeplaceToPumpTo()
	{
		Cell cC = null;
		return HasSomeplaceToPumpTo(cC);
	}

	public bool IsNeeded()
	{
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null)
		{
			return false;
		}
		if (cell.OnWorldMap())
		{
			return false;
		}
		if (!HasSomeplaceToPumpTo(cell))
		{
			return false;
		}
		if (!HasSomeplaceToPumpFrom(cell))
		{
			return false;
		}
		return true;
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (string.IsNullOrEmpty(Liquid) || Liquid == LiquidType)
		{
			return true;
		}
		return false;
	}

	public bool DistributeLiquid()
	{
		Cell cC = ParentObject.GetCurrentCell();
		int phase = ParentObject.GetPhase();
		LiquidVolume liquidVolumeToPumpFrom = GetLiquidVolumeToPumpFrom(cC, phase);
		if (liquidVolumeToPumpFrom == null)
		{
			return false;
		}
		LiquidVolume liquidVolumeToPumpTo = GetLiquidVolumeToPumpTo(cC, phase);
		Cell cell = ((liquidVolumeToPumpTo == null) ? GetCellToPumpTo(cC, phase) : null);
		if (liquidVolumeToPumpTo == null && cell == null)
		{
			return false;
		}
		if (!liquidVolumeToPumpFrom.UseDram())
		{
			return false;
		}
		if (!pumped.ComponentLiquids.ContainsKey(Liquid) || pumped.ComponentLiquids.Count > 1)
		{
			if (pumped.ComponentLiquids.Count > 0)
			{
				pumped.ComponentLiquids.Clear();
			}
			pumped.ComponentLiquids.Add(Liquid, 1000);
		}
		pumped.MaxVolume = 1;
		pumped.Volume = 1;
		if (liquidVolumeToPumpTo != null)
		{
			liquidVolumeToPumpTo.MixWith(pumped);
			return true;
		}
		if (cell != null)
		{
			GameObject gameObject = GameObject.Create("Water");
			LiquidVolume liquidVolume = gameObject.LiquidVolume;
			liquidVolume.Volume = pumped.Volume;
			if (PureOnFloor || string.IsNullOrEmpty(cell.GroundLiquid))
			{
				liquidVolume.InitialLiquid = Liquid + "-1000";
			}
			else
			{
				liquidVolume.InitialLiquid = cell.GroundLiquid;
				liquidVolume.MixWith(pumped);
			}
			cell.AddObject(gameObject);
			return true;
		}
		return false;
	}
}

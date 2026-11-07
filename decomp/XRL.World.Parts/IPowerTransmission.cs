using System;
using System.Collections.Generic;
using System.Text;
using XRL.Core;
using XRL.Language;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public abstract class IPowerTransmission : IPoweredPart
{
	public int ChargeRate = 100;

	public int ChanceBreakConnectedOnDestroy = 50;

	public int ChanceBreakConnectedOnMove = 50;

	public int ChanceBreakOnMove = 100;

	public string Substance;

	public string Activity;

	public string Constituent;

	public string Assembly;

	public string Unit;

	public string DependsOnLiquid;

	public double UnitFactor = 0.1;

	public double WrongLiquidFactor;

	public bool IsProducer;

	public bool IsConsumer;

	public bool IsConduit;

	public bool DamageDegrades = true;

	public bool Readout;

	public bool DischargeLiquidWhenBrokenAndPowered;

	public bool DischargeLiquidWhenBrokenAndUnpowered;

	public bool MingleLiquidsWhenPowered;

	public bool MingleLiquidsWhenUnpowered;

	public bool SparkWhenBrokenAndPowered;

	public bool TileEffects;

	public string TileBaseFromTag;

	public string TileAppendWhenPowered;

	public string TileAppendWhenUnpowered;

	public string TileAppendWhenBroken;

	public string TileAppendWhenUnbroken;

	public string TileAppendWhenBrokenAndPowered;

	public string TileAppendWhenUnbrokenAndPowered;

	public string TileAppendWhenBrokenAndUnpowered;

	public string TileAppendWhenUnbrokenAndUnpowered;

	public bool TileAnimateWhenPowered;

	public bool TileAnimateWhenUnpowered;

	public bool TileAnimateSuppressWhenBroken;

	public bool TileAnimateSuppressWhenUnbroken;

	public int TileAnimatePoweredFrames;

	public int TileAnimateUnpoweredFrames;

	public bool TileAnimateDesyncFrame;

	public int FrameOffset = -1;

	public bool SparkingActive;

	public string[] PaintedTileParts;

	[NonSerialized]
	private static long GridBitBase = 1L;

	[NonSerialized]
	private long GridBit;

	[NonSerialized]
	private List<GameObject> Grid;

	[NonSerialized]
	private int GridCapacity;

	[NonSerialized]
	private List<GameObject> Producers;

	[NonSerialized]
	private List<GameObject> Consumers;

	[NonSerialized]
	private List<long> ChargeActivity;

	private static string[] TileSplitter = new string[1];

	[NonSerialized]
	private long AdjacentChargeTurn;

	[NonSerialized]
	private int AdjacentCharge;

	private static StringBuilder TileBuilder = new StringBuilder(64);

	private static List<Cell> Visited = new List<Cell>();

	private static List<Cell> VisitNow = new List<Cell>();

	private static List<Cell> VisitNext = new List<Cell>();

	private static long lastSparkTurn = -1L;

	public IPowerTransmission()
	{
		ChargeUse = 0;
		WorksOnSelf = true;
	}

	public abstract string GetPowerTransmissionType();

	public override bool SameAs(IPart p)
	{
		IPowerTransmission powerTransmission = p as IPowerTransmission;
		if (powerTransmission.GetPowerTransmissionType() != GetPowerTransmissionType())
		{
			return false;
		}
		if (powerTransmission.ChargeRate != ChargeRate)
		{
			return false;
		}
		if (powerTransmission.ChanceBreakConnectedOnDestroy != ChanceBreakConnectedOnDestroy)
		{
			return false;
		}
		if (powerTransmission.ChanceBreakConnectedOnMove != ChanceBreakConnectedOnMove)
		{
			return false;
		}
		if (powerTransmission.ChanceBreakOnMove != ChanceBreakOnMove)
		{
			return false;
		}
		if (powerTransmission.Substance != Substance)
		{
			return false;
		}
		if (powerTransmission.Activity != Activity)
		{
			return false;
		}
		if (powerTransmission.Constituent != Constituent)
		{
			return false;
		}
		if (powerTransmission.Assembly != Assembly)
		{
			return false;
		}
		if (powerTransmission.Unit != Unit)
		{
			return false;
		}
		if (powerTransmission.UnitFactor != UnitFactor)
		{
			return false;
		}
		if (powerTransmission.DamageDegrades != DamageDegrades)
		{
			return false;
		}
		if (powerTransmission.Readout != Readout)
		{
			return false;
		}
		if (powerTransmission.IsProducer != IsProducer)
		{
			return false;
		}
		if (powerTransmission.IsConsumer != IsConsumer)
		{
			return false;
		}
		if (powerTransmission.IsConduit != IsConduit)
		{
			return false;
		}
		if (powerTransmission.DischargeLiquidWhenBrokenAndPowered != DischargeLiquidWhenBrokenAndPowered)
		{
			return false;
		}
		if (powerTransmission.DischargeLiquidWhenBrokenAndUnpowered != DischargeLiquidWhenBrokenAndUnpowered)
		{
			return false;
		}
		if (powerTransmission.MingleLiquidsWhenPowered != MingleLiquidsWhenPowered)
		{
			return false;
		}
		if (powerTransmission.MingleLiquidsWhenUnpowered != MingleLiquidsWhenUnpowered)
		{
			return false;
		}
		if (powerTransmission.SparkWhenBrokenAndPowered != SparkWhenBrokenAndPowered)
		{
			return false;
		}
		if (powerTransmission.TileEffects != TileEffects)
		{
			return false;
		}
		if (powerTransmission.TileBaseFromTag != TileBaseFromTag)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenPowered != TileAppendWhenPowered)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenUnpowered != TileAppendWhenUnpowered)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenBroken != TileAppendWhenBroken)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenUnbroken != TileAppendWhenUnbroken)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenBrokenAndPowered != TileAppendWhenBrokenAndPowered)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenUnbrokenAndPowered != TileAppendWhenUnbrokenAndPowered)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenBrokenAndUnpowered != TileAppendWhenBrokenAndUnpowered)
		{
			return false;
		}
		if (powerTransmission.TileAppendWhenUnbrokenAndUnpowered != TileAppendWhenUnbrokenAndUnpowered)
		{
			return false;
		}
		if (powerTransmission.TileAnimateWhenPowered != TileAnimateWhenPowered)
		{
			return false;
		}
		if (powerTransmission.TileAnimateWhenUnpowered != TileAnimateWhenUnpowered)
		{
			return false;
		}
		if (powerTransmission.TileAnimateSuppressWhenBroken != TileAnimateSuppressWhenBroken)
		{
			return false;
		}
		if (powerTransmission.TileAnimateSuppressWhenUnbroken != TileAnimateSuppressWhenUnbroken)
		{
			return false;
		}
		if (powerTransmission.TileAnimatePoweredFrames != TileAnimatePoweredFrames)
		{
			return false;
		}
		if (powerTransmission.TileAnimateUnpoweredFrames != TileAnimateUnpoweredFrames)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && (ID != AllowLiquidCollectionEvent.ID || DependsOnLiquid.IsNullOrEmpty()) && ID != PooledEvent<AnimateEvent>.ID && (ID != ChargeAvailableEvent.ID || !IsProducer) && ID != EnteredCellEvent.ID && ID != EffectAppliedEvent.ID && ID != EffectRemovedEvent.ID && (ID != FinishChargeAvailableEvent.ID || !IsProducer) && ID != SingletonEvent<GetDebugInternalsEvent>.ID && (ID != GetPreferredLiquidEvent.ID || DependsOnLiquid.IsNullOrEmpty()) && ID != GetShortDescriptionEvent.ID && ID != LeavingCellEvent.ID && (ID != PooledEvent<LiquidMixedEvent>.ID || DependsOnLiquid.IsNullOrEmpty()) && ID != OnDestroyObjectEvent.ID && (ID != PowerSwitchFlippedEvent.ID || !IsPowerSwitchSensitive) && (ID != SingletonEvent<RepaintedEvent>.ID || !TileEffects || TileBaseFromTag.IsNullOrEmpty()) && (ID != PooledEvent<StatChangeEvent>.ID || !DamageDegrades) && (ID != QueryChargeEvent.ID || !IsConsumer) && (ID != TestChargeEvent.ID || !IsConsumer) && (ID != UseChargeEvent.ID || !IsConsumer))
		{
			if (ID == WantsLiquidCollectionEvent.ID)
			{
				return !DependsOnLiquid.IsNullOrEmpty();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(AnimateEvent E)
	{
		ChanceBreakOnMove = 0;
		ChanceBreakConnectedOnDestroy = 0;
		ChanceBreakConnectedOnMove = 0;
		MingleLiquidsWhenPowered = false;
		MingleLiquidsWhenUnpowered = false;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(QueryChargeEvent E)
	{
		if ((E.GridMask & GridBit) == 0L && IsConsumer)
		{
			long gridMask = E.GridMask | GridBit;
			if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
			{
				int num = GetCharge(E.IncludeTransient, E.IncludeBiological, E.GridMask) - ChargeUse * E.Multiple;
				if (num > 0)
				{
					E.Amount += num;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TestChargeEvent E)
	{
		if ((E.GridMask & GridBit) == 0L && IsConsumer)
		{
			long gridMask = E.GridMask | GridBit;
			if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
			{
				int num = Math.Min(E.Amount, GetCharge(E.IncludeTransient, E.IncludeBiological, E.GridMask) - ChargeUse * E.Multiple);
				if (num > 0)
				{
					E.Amount -= num;
					if (E.Amount <= 0)
					{
						return false;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UseChargeEvent E)
	{
		if (E.Pass == 1 && (E.GridMask & GridBit) == 0L && IsConsumer)
		{
			long gridMask = E.GridMask | GridBit;
			if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
			{
				int num = ChargeUse * E.Multiple;
				int num2 = Math.Min(E.Amount, GetCharge(E.IncludeTransient, E.IncludeBiological, E.GridMask) - num);
				if (num2 > 0)
				{
					UseCharge(num2 + num, E.GridMask);
					E.Amount -= num2;
					if (E.Amount <= 0)
					{
						return false;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ChargeAvailableEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(FinishChargeAvailableEvent E)
	{
		Process(E);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LiquidMixedEvent E)
	{
		DependsOnLiquid.IsNullOrEmpty();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RepaintedEvent E)
	{
		if (TileEffects && !TileBaseFromTag.IsNullOrEmpty())
		{
			string tag = ParentObject.GetTag(TileBaseFromTag);
			if (tag != null)
			{
				TileSplitter[0] = tag;
				PaintedTileParts = ParentObject.Render.Tile.Split(TileSplitter, 2, StringSplitOptions.None);
			}
			else
			{
				PaintedTileParts = null;
			}
			SyncTile();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ResetGrid();
		Cell cell = ParentObject.CurrentCell;
		Zone zone = cell?.ParentZone;
		if (zone != null && zone.Built)
		{
			ZoneManager.PaintWalls(zone, cell.X - 1, cell.Y - 1, cell.X + 1, cell.Y + 1);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(LeavingCellEvent E)
	{
		ResetGrid();
		if (ChanceBreakOnMove.in100())
		{
			ParentObject.ApplyEffect(new Broken());
		}
		if (ChanceBreakConnectedOnMove > 0)
		{
			ScanForBreakageOnMove();
		}
		return base.HandleEvent(E);
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
		if (E.Liquid == null && !DependsOnLiquid.IsNullOrEmpty())
		{
			E.Liquid = DependsOnLiquid;
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

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (E.Effect is IBusted)
		{
			if (The.Game != null)
			{
				ResetGrid();
			}
			if (The.Game != null)
			{
				CheckSparking();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EffectRemovedEvent E)
	{
		if (E.Effect is IBusted)
		{
			ResetGrid();
			CheckSparking();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(StatChangeEvent E)
	{
		if (E.Name == "Hitpoints" && DamageDegrades)
		{
			ResetGrid();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		if (IsPowerSwitchSensitive)
		{
			ResetGrid();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Power available", GetCharge(IncludeTransient: true, IncludeBiological: true, 0L));
		E.AddEntry(this, "Total production", GetTotalProduction());
		E.AddEntry(this, "Total availability", GetTotalAvailability(0L));
		E.AddEntry(this, "Total draw", GetTotalDraw());
		E.AddEntry(this, "Local capacity", GetEffectiveChargeRate());
		E.AddEntry(this, "Grid capacity", GridCapacity + " " + ((GridCapacity == 1) ? Unit : Grammar.Pluralize(Unit)));
		E.AddEntry(this, "Grid bit", $"0x{GridBit:X}");
		E.AddEntry(this, "Any charge activity", AnyChargeActivity(0L));
		E.AddEntry(this, "Should be sparking", ShouldBeSparking());
		E.AddEntry(this, "Any adjacent charge activity", AnyAdjacentChargeActivity());
		E.AddEntry(this, "Adjacent charge", GetAdjacentCharge(IncludeTransient: true, IncludeBiological: true, 0L));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(GetBehaviorDescription());
		if (Readout || (!DescribeStatusForProperty.IsNullOrEmpty() && IComponent<GameObject>.ThePlayer != null && IComponent<GameObject>.ThePlayer.GetIntProperty(DescribeStatusForProperty) > 0))
		{
			int charge = GetCharge(IncludeTransient: true, IncludeBiological: true, 0L);
			int totalProduction = GetTotalProduction();
			int totalAvailability = GetTotalAvailability(0L);
			int totalDraw = GetTotalDraw();
			string value = "\n{{rules|";
			string text = ((double)charge * UnitFactor).ToString("F").TrimEnd('0').TrimEnd('.');
			E.Postfix.Append(value).Append("Power available: ").Append(text)
				.Append(' ')
				.Append((text == "1") ? Unit : Grammar.Pluralize(Unit));
			string text2 = ((double)totalProduction * UnitFactor).ToString("F").TrimEnd('0').TrimEnd('.');
			E.Postfix.Append(value).Append("Total ").Append(Assembly)
				.Append(" production: ")
				.Append(text2)
				.Append(' ')
				.Append((text2 == "1") ? Unit : Grammar.Pluralize(Unit));
			string text3 = ((double)totalAvailability * UnitFactor).ToString("F").TrimEnd('0').TrimEnd('.');
			E.Postfix.Append(value).Append("Total ").Append(Assembly)
				.Append(" availability: ")
				.Append(text3)
				.Append(' ')
				.Append((text3 == "1") ? Unit : Grammar.Pluralize(Unit));
			string text4 = ((double)totalDraw * UnitFactor).ToString("F").TrimEnd('0').TrimEnd('.');
			E.Postfix.Append(value).Append("Total ").Append(Assembly)
				.Append(" draw: ")
				.Append(text4)
				.Append(' ')
				.Append((text4 == "1") ? Unit : Grammar.Pluralize(Unit));
			string text5 = ((double)GetEffectiveChargeRate() * UnitFactor).ToString("F").TrimEnd('0').TrimEnd('.');
			E.Postfix.Append(value).Append("Local ").Append(Assembly)
				.Append(" capacity: ")
				.Append(text5)
				.Append(' ')
				.Append((text5 == "1") ? Unit : Grammar.Pluralize(Unit));
			E.Postfix.Append("}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		ResetGrid();
		if (ChanceBreakConnectedOnDestroy > 0)
		{
			ScanForBreakageOnDestroy();
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (TileEffects)
		{
			SyncTile();
		}
		if (SparkingActive)
		{
			MaybeEmitCosmeticSpark();
		}
		return true;
	}

	public void HadChargeActivity(long TurnNumber)
	{
		if (ChargeActivity != null)
		{
			ChargeActivity[0] = TurnNumber;
		}
	}

	public void HadChargeActivity()
	{
		HadChargeActivity(XRLCore.CurrentTurn);
	}

	public bool AnyChargeActivity(long GridMask = 0L, bool IgnoreReady = false)
	{
		if (ChargeActivity != null && ChargeActivity[0] >= XRLCore.CurrentTurn - 1)
		{
			return true;
		}
		return AnyCharge(GridMask, IgnoreReady);
	}

	public bool AnyAdjacentChargeActivity()
	{
		if (AnyChargeActivity(0L))
		{
			return true;
		}
		if (AnyChargeActivity(ParentObject.CurrentCell))
		{
			return true;
		}
		List<Cell> list = ParentObject.CurrentCell?.GetLocalAdjacentCells();
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				if (AnyChargeActivity(list[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool AnyChargeActivity(Cell C)
	{
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				GameObject gameObject = C.Objects[i];
				if (gameObject != ParentObject)
				{
					IPowerTransmission correspondingPart = GetCorrespondingPart(gameObject);
					if (correspondingPart != null && correspondingPart.AnyChargeActivity(0L))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool AnyCharge(long GridMask = 0L, bool IgnoreReady = false)
	{
		if ((GridMask & GridBit) == 0L)
		{
			if (!IgnoreReady)
			{
				long gridMask = GridMask | GridBit;
				if (!IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
				{
					goto IL_0084;
				}
			}
			CheckGrid();
			if (Producers != null)
			{
				int i = 0;
				for (int count = Producers.Count; i < count; i++)
				{
					if (Producers[i].TestCharge(1, LiveOnly: false, GridMask | GridBit))
					{
						return true;
					}
				}
			}
		}
		goto IL_0084;
		IL_0084:
		return false;
	}

	public bool HasCharge(int Amount, bool IncludeTransient = true, bool IncludeBiological = true, bool IgnoreReady = false)
	{
		return GetCharge(IncludeTransient, IncludeBiological, 0L, IgnoreReady) >= Amount;
	}

	public int GetTotalAvailability(long GridMask = 0L, bool IgnoreReady = false)
	{
		int num = 0;
		if ((GridMask & GridBit) == 0L)
		{
			long gridMask = GridMask | GridBit;
			if (IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
			{
				CheckGrid();
				if (Producers != null)
				{
					int i = 0;
					for (int count = Producers.Count; i < count; i++)
					{
						num += Producers[i].QueryCharge(LiveOnly: false, GridMask | GridBit);
					}
				}
			}
		}
		return num;
	}

	public int GetTotalProduction()
	{
		int num = 0;
		CheckGrid();
		if (Producers != null)
		{
			for (int i = 0; i < Producers.Count; i++)
			{
				num += Producers[i].QueryChargeProduction();
			}
		}
		return num;
	}

	public int GetCharge(bool IncludeTransient = true, bool IncludeBiological = true, long GridMask = 0L, bool IgnoreReady = false)
	{
		int num = 0;
		if ((GridMask & GridBit) == 0L)
		{
			if (!IgnoreReady)
			{
				long gridMask = GridMask | GridBit;
				if (!IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask))
				{
					goto IL_00bb;
				}
			}
			CheckGrid();
			if (Producers != null)
			{
				int num2 = 0;
				int i = 0;
				for (int count = Producers.Count; i < count; i++)
				{
					int num3 = GridCapacity - num2;
					if (num3 > num)
					{
						int val = num3 - num;
						int val2 = Producers[i].QueryCharge(LiveOnly: false, GridMask | GridBit, IncludeTransient, IncludeBiological);
						int num4 = Math.Min(val, val2);
						if (num4 > 0)
						{
							num += num4;
							num2 += num4;
						}
					}
				}
			}
		}
		goto IL_00bb;
		IL_00bb:
		return num;
	}

	public int GetAdjacentCharge(bool IncludeTransient = true, bool IncludeBiological = true, long GridMask = 0L, bool IgnoreReady = false, int MinIfActivity = 0)
	{
		int num = GetCharge(IncludeTransient, IncludeBiological, GridMask, IgnoreReady);
		int charge = GetCharge(ParentObject.CurrentCell, IncludeTransient, IncludeBiological, GridMask, IgnoreReady);
		if (charge > num)
		{
			num = charge;
		}
		List<Cell> list = ParentObject.CurrentCell?.GetLocalAdjacentCells();
		if (list != null)
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				int charge2 = GetCharge(list[i], IncludeTransient, IncludeBiological, GridMask, IgnoreReady);
				if (charge2 > num)
				{
					num = charge2;
				}
			}
		}
		if (num < MinIfActivity && (num > 0 || AnyAdjacentChargeActivity()))
		{
			num = MinIfActivity;
		}
		return num;
	}

	private int GetCharge(Cell C, bool IncludeTransient = true, bool IncludeBiological = true, long GridMask = 0L, bool IgnoreReady = false)
	{
		int num = 0;
		if (C != null)
		{
			int i = 0;
			for (int count = C.Objects.Count; i < count; i++)
			{
				GameObject gameObject = C.Objects[i];
				if (gameObject == ParentObject)
				{
					continue;
				}
				IPowerTransmission correspondingPart = GetCorrespondingPart(gameObject);
				if (correspondingPart != null)
				{
					int charge = correspondingPart.GetCharge(IncludeTransient, IncludeBiological, GridMask, IgnoreReady);
					if (charge > num)
					{
						num = charge;
					}
				}
			}
		}
		return num;
	}

	public int GetTotalDraw()
	{
		CheckGrid();
		return QueryDrawEvent.GetFor(Consumers);
	}

	public void UseCharge(int Amount, long GridMask = 0L)
	{
		if ((GridMask & GridBit) != 0L)
		{
			return;
		}
		CheckGrid();
		if (Producers == null)
		{
			return;
		}
		int num = 0;
		int i = 0;
		for (int count = Producers.Count; i < count; i++)
		{
			GameObject gameObject = Producers[i];
			int num2 = GridCapacity - num;
			if (num2 <= 0)
			{
				continue;
			}
			int val = gameObject.QueryCharge(LiveOnly: false, GridMask | GridBit);
			int num3 = Math.Min(Math.Min(num2, val), Amount);
			if (num3 > 0)
			{
				gameObject.UseCharge(num3, LiveOnly: false, GridMask | GridBit);
				Amount -= num3;
				num += num3;
				HadChargeActivity();
				if (Amount <= 0)
				{
					break;
				}
			}
		}
	}

	public double GetPerformance()
	{
		if (!DamageDegrades && DependsOnLiquid.IsNullOrEmpty())
		{
			return 1.0;
		}
		double num = 1.0;
		if (DamageDegrades)
		{
			num *= ParentObject.Health();
		}
		if (!DependsOnLiquid.IsNullOrEmpty())
		{
			LiquidVolume liquidVolume = ParentObject.LiquidVolume;
			if (liquidVolume != null && liquidVolume.Volume > 0)
			{
				int num2 = 0;
				int num3 = 0;
				double? num4 = null;
				double? num5 = null;
				if (liquidVolume.ComponentLiquids.ContainsKey(DependsOnLiquid))
				{
					if (liquidVolume.ComponentLiquids.Count == 1)
					{
						num2 = liquidVolume.Volume;
					}
					else
					{
						num4 = liquidVolume.Volume * liquidVolume.ComponentLiquids[DependsOnLiquid] / 1000;
						num5 = (double)liquidVolume.Volume - num4;
					}
				}
				else
				{
					num3 = liquidVolume.Volume;
				}
				if (num4.HasValue || num2 < liquidVolume.MaxVolume)
				{
					if (!num4.HasValue)
					{
						num4 = num2;
					}
					double num6 = num4.Value / (double)liquidVolume.MaxVolume;
					if (WrongLiquidFactor > 0.0)
					{
						if (!num5.HasValue)
						{
							num5 = num3;
						}
						num6 += num5.Value * WrongLiquidFactor / (double)liquidVolume.MaxVolume;
					}
					num *= num6;
				}
			}
			else
			{
				num = 0.0;
			}
		}
		return num;
	}

	public int GetEffectiveChargeRate()
	{
		int num = ChargeRate;
		double performance = GetPerformance();
		if (performance != 1.0)
		{
			num = (int)Math.Round((double)num * performance);
		}
		return num;
	}

	public void MaybeEmitCosmeticSpark()
	{
		if (AdjacentChargeTurn < XRLCore.CurrentTurn)
		{
			AdjacentChargeTurn = XRLCore.CurrentTurn;
			AdjacentCharge = GetAdjacentCharge(IncludeTransient: true, IncludeBiological: true, 0L, IgnoreReady: false, 100);
		}
		if (Stat.RandomCosmetic(1, 120) <= Math.Min(1 + AdjacentCharge / 200, 12))
		{
			EmitCosmeticSpark();
		}
	}

	public void EmitCosmeticSpark()
	{
		for (int i = 0; i < 2; i++)
		{
			ParentObject.ParticleText("&Y" + (char)Stat.RandomCosmetic(191, 198), 0.2f, 20);
		}
		for (int j = 0; j < 2; j++)
		{
			ParentObject.ParticleText("&W\u000f", 0.02f, 10);
		}
		PlayWorldSound("sfx_spark", 0.35f, 0.35f);
	}

	public void SyncTile()
	{
		if (!TileEffects || PaintedTileParts == null || TileBaseFromTag == null)
		{
			return;
		}
		TileBuilder.Length = 0;
		TileBuilder.Append(PaintedTileParts[0]);
		TileBuilder.Append(ParentObject.GetTag(TileBaseFromTag));
		bool flag = false;
		bool flag2 = AnyChargeActivity(0L);
		if (flag2)
		{
			if (!TileAppendWhenBrokenAndPowered.IsNullOrEmpty() || !TileAppendWhenUnbrokenAndPowered.IsNullOrEmpty())
			{
				if (IsBroken())
				{
					if (!TileAppendWhenBrokenAndPowered.IsNullOrEmpty())
					{
						TileBuilder.Append(TileAppendWhenBrokenAndPowered);
						flag = true;
					}
				}
				else if (!TileAppendWhenUnbrokenAndPowered.IsNullOrEmpty())
				{
					TileBuilder.Append(TileAppendWhenUnbrokenAndPowered);
					flag = true;
				}
			}
			if (!flag && !TileAppendWhenPowered.IsNullOrEmpty())
			{
				TileBuilder.Append(TileAppendWhenPowered);
			}
		}
		else
		{
			if (!TileAppendWhenBrokenAndUnpowered.IsNullOrEmpty() || !TileAppendWhenUnbrokenAndUnpowered.IsNullOrEmpty())
			{
				if (IsBroken())
				{
					if (!TileAppendWhenBrokenAndUnpowered.IsNullOrEmpty())
					{
						TileBuilder.Append(TileAppendWhenBrokenAndUnpowered);
						flag = true;
					}
				}
				else if (!TileAppendWhenUnbrokenAndUnpowered.IsNullOrEmpty())
				{
					TileBuilder.Append(TileAppendWhenUnbrokenAndUnpowered);
					flag = true;
				}
			}
			if (!flag && !TileAppendWhenUnpowered.IsNullOrEmpty())
			{
				TileBuilder.Append(TileAppendWhenUnpowered);
			}
		}
		if (!flag && (!TileAppendWhenBroken.IsNullOrEmpty() || !TileAppendWhenUnbroken.IsNullOrEmpty()))
		{
			if (IsBroken())
			{
				if (!TileAppendWhenBroken.IsNullOrEmpty())
				{
					TileBuilder.Append(TileAppendWhenBroken);
				}
			}
			else if (!TileAppendWhenUnbroken.IsNullOrEmpty())
			{
				TileBuilder.Append(TileAppendWhenUnbroken);
			}
		}
		if ((flag2 ? TileAnimateWhenPowered : TileAnimateWhenUnpowered) && (!TileAnimateSuppressWhenBroken || !IsBroken()) && (!TileAnimateSuppressWhenUnbroken || IsBroken()))
		{
			int num = (flag2 ? TileAnimatePoweredFrames : TileAnimateUnpoweredFrames);
			if (num > 0)
			{
				long num2 = XRLCore.CurrentFrameLong;
				if (TileAnimateDesyncFrame)
				{
					if (FrameOffset == -1)
					{
						FrameOffset = Stat.RandomCosmetic(0, 1000);
					}
					num2 = (num2 + FrameOffset) % 1000;
				}
				int num3 = 1000 / num;
				int value = (int)Math.Min(1 + num2 / num3, num);
				TileBuilder.Append('_').Append(value);
			}
		}
		if (PaintedTileParts.Length >= 2)
		{
			TileBuilder.Append(PaintedTileParts[1]);
		}
		if (!TileBuilder.CompareTo(ParentObject.Render.Tile))
		{
			ParentObject.Render.Tile = TileBuilder.ToString();
		}
	}

	private IPowerTransmission GetCorrespondingPart(GameObject obj)
	{
		string powerTransmissionType = GetPowerTransmissionType();
		int i = 0;
		for (int count = obj.PartsList.Count; i < count; i++)
		{
			if ((obj.PartsList[i] as IPowerTransmission)?.GetPowerTransmissionType() == powerTransmissionType)
			{
				return obj.PartsList[i] as IPowerTransmission;
			}
		}
		return null;
	}

	private bool TypeMatches(IPowerTransmission part)
	{
		return part.GetPowerTransmissionType() == GetPowerTransmissionType();
	}

	private bool TypeMatches(GameObject obj)
	{
		return GetCorrespondingPart(obj) != null;
	}

	public void FindGrid()
	{
		if (ParentObject.CurrentCell == null || ParentObject.CurrentCell?.ParentZone?.IsActive() != true || ParentObject.CurrentCell?.ParentZone?.IsWorldMap() == true)
		{
			return;
		}
		if (Grid == null)
		{
			Grid = new List<GameObject> { ParentObject };
		}
		if (ChargeActivity == null)
		{
			ChargeActivity = new List<long> { 0L };
		}
		if (Producers == null)
		{
			Producers = new List<GameObject>();
		}
		if (Consumers == null)
		{
			Consumers = new List<GameObject>();
		}
		Visited.Clear();
		VisitNext.Clear();
		VisitNext.Add(ParentObject.CurrentCell);
		GridCapacity = GetEffectiveChargeRate();
		while (VisitNext.Count > 0)
		{
			VisitNow.Clear();
			VisitNow.AddRange(VisitNext);
			VisitNext.Clear();
			int i = 0;
			for (int count = VisitNow.Count; i < count; i++)
			{
				Cell cell = VisitNow[i];
				Visited.Add(cell);
				int j = 0;
				for (int count2 = cell.Objects.Count; j < count2; j++)
				{
					GameObject gameObject = cell.Objects[j];
					IPowerTransmission correspondingPart = GetCorrespondingPart(gameObject);
					if (correspondingPart == null)
					{
						continue;
					}
					long gridBit = GridBit;
					if (!correspondingPart.IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridBit))
					{
						continue;
					}
					int effectiveChargeRate = correspondingPart.GetEffectiveChargeRate();
					if (effectiveChargeRate <= 0)
					{
						continue;
					}
					if (gameObject != ParentObject)
					{
						Grid.Add(gameObject);
						if (GridBit == 0L)
						{
							GridBit = GridBitBase;
							GridBitBase <<= 1;
							if (GridBitBase == 0L)
							{
								GridBitBase = 1L;
							}
						}
						correspondingPart.GridBit = GridBit;
						correspondingPart.Grid = Grid;
						correspondingPart.ChargeActivity = ChargeActivity;
						correspondingPart.Producers = Producers;
						correspondingPart.Consumers = Consumers;
					}
					if (GridCapacity > effectiveChargeRate)
					{
						GridCapacity = effectiveChargeRate;
					}
					if (correspondingPart.IsProducer && !Producers.Contains(gameObject))
					{
						Producers.Add(gameObject);
					}
					if (correspondingPart.IsConsumer && !Consumers.Contains(gameObject))
					{
						Consumers.Add(gameObject);
					}
					Cell cell2 = gameObject.CurrentCell;
					if (cell2 == null)
					{
						continue;
					}
					int k = 0;
					for (int num = Cell.DirectionListCardinalOnly.Length; k < num; k++)
					{
						Cell localCellFromDirection = cell2.GetLocalCellFromDirection(Cell.DirectionListCardinalOnly[k]);
						if (localCellFromDirection != null && !Visited.Contains(localCellFromDirection) && !VisitNext.Contains(localCellFromDirection) && !VisitNow.Contains(localCellFromDirection) && localCellFromDirection.HasObject(TypeMatches))
						{
							VisitNext.Add(localCellFromDirection);
						}
					}
				}
			}
		}
		int l = 0;
		for (int count3 = Grid.Count; l < count3; l++)
		{
			GameObject obj = Grid[l];
			IPowerTransmission correspondingPart2 = GetCorrespondingPart(obj);
			if (correspondingPart2 != null)
			{
				correspondingPart2.GridCapacity = GridCapacity;
			}
		}
	}

	public void CheckGrid()
	{
		if (GridBit == 0L)
		{
			FindGrid();
		}
	}

	public void ResetGrid(bool Local = false, bool Initial = true)
	{
		if (ParentObject.GetLongProperty("LastReset", 0L) == The.Game.Turns)
		{
			return;
		}
		ParentObject.SetLongProperty("LastReset", The.Game.Turns);
		if (!Local)
		{
			if (Grid != null)
			{
				int i = 0;
				for (int count = Grid.Count; i < count; i++)
				{
					GameObject obj = Grid[i];
					IPowerTransmission correspondingPart = GetCorrespondingPart(obj);
					if (correspondingPart != null && correspondingPart.GridBit == GridBit)
					{
						correspondingPart.ResetGrid(Local: true);
					}
				}
			}
			ResetGrid(ParentObject.CurrentCell);
			List<Cell> list = ParentObject.CurrentCell?.GetLocalAdjacentCells();
			if (list != null)
			{
				int j = 0;
				for (int count2 = list.Count; j < count2; j++)
				{
					ResetGrid(list[j]);
				}
			}
		}
		GridBit = 0L;
		GridCapacity = 0;
		ChargeActivity = null;
		Producers = null;
		Consumers = null;
		if (Initial)
		{
			PowerUpdatedEvent.Send(ParentObject.CurrentZone, ParentObject);
		}
	}

	private void ResetGrid(Cell C)
	{
		if (C == null)
		{
			return;
		}
		int i = 0;
		for (int count = C.Objects.Count; i < count; i++)
		{
			GameObject gameObject = C.Objects[i];
			if (gameObject.GetLongProperty("LastReset", 0L) != The.Game.Turns && gameObject != ParentObject)
			{
				IPowerTransmission correspondingPart = GetCorrespondingPart(gameObject);
				if (correspondingPart != null && correspondingPart.GridBit != 0L)
				{
					correspondingPart.ResetGrid(Local: false, Initial: false);
				}
			}
		}
	}

	private void ScanForBreakageOnDestroy(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && currentZone.Built)
		{
			IPowerTransmission correspondingPart = GetCorrespondingPart(obj);
			if (correspondingPart != null && !correspondingPart.IsBroken() && ChanceBreakConnectedOnDestroy.in100())
			{
				obj.ApplyEffect(new Broken());
			}
		}
	}

	private void ScanForBreakageOnDestroy(Cell C)
	{
		C.ForeachObject((Action<GameObject>)ScanForBreakageOnDestroy);
	}

	private void ScanForBreakageOnDestroyFrom(Cell C)
	{
		ScanForBreakageOnDestroy(C);
		C.ForeachCardinalAdjacentCell((Action<Cell>)ScanForBreakageOnDestroy);
	}

	private void ScanForBreakageOnDestroy()
	{
		if (ParentObject.CurrentCell != null)
		{
			ScanForBreakageOnDestroyFrom(ParentObject.CurrentCell);
		}
	}

	private void ScanForBreakageOnMove(GameObject obj)
	{
		if (obj == ParentObject)
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && currentZone.Built)
		{
			IPowerTransmission correspondingPart = GetCorrespondingPart(obj);
			if (correspondingPart != null && !correspondingPart.IsBroken() && ChanceBreakConnectedOnMove.in100())
			{
				obj.ApplyEffect(new Broken());
			}
		}
	}

	private void ScanForBreakageOnMove(Cell C)
	{
		C.ForeachObject((Action<GameObject>)ScanForBreakageOnMove);
	}

	private void ScanForBreakageOnMoveFrom(Cell C)
	{
		ScanForBreakageOnMove(C);
		C.ForeachCardinalAdjacentCell((Action<Cell>)ScanForBreakageOnMove);
	}

	private void ScanForBreakageOnMove()
	{
		if (ParentObject.CurrentCell != null)
		{
			ScanForBreakageOnMoveFrom(ParentObject.CurrentCell);
		}
	}

	public bool ShouldBeSparking()
	{
		if (!SparkWhenBrokenAndPowered)
		{
			return false;
		}
		if (!IsBroken())
		{
			return false;
		}
		if (!AnyAdjacentChargeActivity())
		{
			return false;
		}
		return true;
	}

	public bool SyncSparking()
	{
		if (SparkingActive)
		{
			if (!ShouldBeSparking())
			{
				SparkingActive = false;
			}
		}
		else if (ShouldBeSparking())
		{
			SparkingActive = true;
		}
		return SparkingActive;
	}

	public void Spark()
	{
		if (ParentObject.CurrentCell == null)
		{
			return;
		}
		Cell randomLocalAdjacentCell = ParentObject.CurrentCell.GetRandomLocalAdjacentCell();
		if (randomLocalAdjacentCell != null)
		{
			int adjacentCharge = GetAdjacentCharge(IncludeTransient: true, IncludeBiological: true, 0L, IgnoreReady: false, 100);
			int voltage = Stat.Random(1, Math.Min(1 + adjacentCharge / 10, 5));
			string text = "1d" + Math.Min(4 + adjacentCharge / 50, 20);
			if (adjacentCharge >= 200)
			{
				text += Math.Min(adjacentCharge / 200, 10).Signed();
			}
			CheckGrid();
			ParentObject.Discharge(randomLocalAdjacentCell, voltage, 0, text, null, ParentObject, ParentObject, null, null, null, Grid);
		}
		ParentObject.ParticleBlip("&K*", 10, 0L);
	}

	public void MaybeSpark()
	{
		if (ParentObject.CurrentCell != null && lastSparkTurn != XRLCore.CurrentFrameLong)
		{
			int adjacentCharge = GetAdjacentCharge(IncludeTransient: true, IncludeBiological: true, 0L);
			if (adjacentCharge > 1000 || Stat.Random(1, 1000) < adjacentCharge)
			{
				lastSparkTurn = XRLCore.CurrentFrameLong;
				Spark();
			}
		}
	}

	public void CheckSparking()
	{
		if (SyncSparking())
		{
			MaybeSpark();
		}
	}

	public bool ShouldDischargeLiquid()
	{
		if (!DischargeLiquidWhenBrokenAndPowered && !DischargeLiquidWhenBrokenAndUnpowered)
		{
			return false;
		}
		if (DischargeLiquidWhenBrokenAndPowered && DischargeLiquidWhenBrokenAndUnpowered)
		{
			return IsBroken();
		}
		if (AnyChargeActivity(0L) ? DischargeLiquidWhenBrokenAndPowered : DischargeLiquidWhenBrokenAndUnpowered)
		{
			return IsBroken();
		}
		return false;
	}

	private int DischargeLiquidForceFrom(IPowerTransmission pt, LiquidVolume lv)
	{
		return Math.Min(Math.Max(GetCharge(IncludeTransient: true, IncludeBiological: true, 0L, IgnoreReady: true), Math.Max(lv.Volume / 8, 1)), lv.Volume);
	}

	private int DischargeLiquidForceFrom(GameObject obj)
	{
		IPowerTransmission correspondingPart = GetCorrespondingPart(obj);
		if (correspondingPart == null)
		{
			return 0;
		}
		LiquidVolume liquidVolume = obj.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume <= 1)
		{
			return 0;
		}
		return DischargeLiquidForceFrom(correspondingPart, liquidVolume);
	}

	private int DischargeLiquidForceFrom(Cell C)
	{
		if (C == null)
		{
			return 0;
		}
		GameObject firstObject = C.GetFirstObject(TypeMatches);
		if (firstObject == null)
		{
			return 0;
		}
		int num = DischargeLiquidForceFrom(firstObject);
		foreach (GameObject @object in C.Objects)
		{
			if (@object != firstObject)
			{
				int num2 = DischargeLiquidForceFrom(firstObject);
				if (num2 > num)
				{
					num = num2;
				}
			}
		}
		return num;
	}

	private int DischargeLiquidForceFrom(string Direction)
	{
		return DischargeLiquidForceFrom(ParentObject.CurrentCell.GetCellFromDirection(Direction));
	}

	private int Randomize(int num)
	{
		if (num <= 1)
		{
			return num;
		}
		return Stat.Random(1, num);
	}

	private int CalmDown(int num)
	{
		if (num <= 0)
		{
			return 0;
		}
		if (num <= 2)
		{
			return num - 1;
		}
		return Stat.Random(1, num - 1);
	}

	public void DischargeLiquid()
	{
		if (ParentObject.CurrentCell == null)
		{
			return;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume <= 1)
		{
			return;
		}
		int num = Randomize(DischargeLiquidForceFrom("S"));
		int num2 = Randomize(DischargeLiquidForceFrom("N"));
		int num3 = Randomize(DischargeLiquidForceFrom("W"));
		int num4 = Randomize(DischargeLiquidForceFrom("E"));
		int num5 = Randomize(DischargeLiquidForceFrom(this, liquidVolume));
		while (num + num2 + num3 + num4 + num5 > liquidVolume.Volume)
		{
			num = CalmDown(num);
			num2 = CalmDown(num2);
			num3 = CalmDown(num3);
			num4 = CalmDown(num4);
			if (num + num2 + num3 + num4 + num5 > liquidVolume.Volume)
			{
				num5 = CalmDown(num5);
			}
		}
		if (num > 0)
		{
			liquidVolume.PourIntoCell(ParentObject, ParentObject.CurrentCell.GetCellFromDirection("N"), num);
		}
		if (num2 > 0)
		{
			liquidVolume.PourIntoCell(ParentObject, ParentObject.CurrentCell.GetCellFromDirection("S"), num2);
		}
		if (num3 > 0)
		{
			liquidVolume.PourIntoCell(ParentObject, ParentObject.CurrentCell.GetCellFromDirection("E"), num3);
		}
		if (num4 > 0)
		{
			liquidVolume.PourIntoCell(ParentObject, ParentObject.CurrentCell.GetCellFromDirection("W"), num4);
		}
		if (num5 > 0)
		{
			liquidVolume.PourIntoCell(ParentObject, ParentObject.CurrentCell, num5);
		}
	}

	public void CheckDischargeLiquid()
	{
		if (ShouldDischargeLiquid())
		{
			DischargeLiquid();
		}
	}

	public bool ShouldMingleLiquids()
	{
		if (!MingleLiquidsWhenPowered && !MingleLiquidsWhenUnpowered)
		{
			return false;
		}
		if (MingleLiquidsWhenPowered && MingleLiquidsWhenUnpowered)
		{
			return true;
		}
		if (!AnyChargeActivity(0L))
		{
			return MingleLiquidsWhenUnpowered;
		}
		return MingleLiquidsWhenPowered;
	}

	public void MingleScan(GameObject obj)
	{
		if (GetCorrespondingPart(obj) != null)
		{
			LiquidVolume liquidVolume = obj.LiquidVolume;
			if (liquidVolume != null)
			{
				ParentObject.LiquidVolume?.MingleAdjacent(liquidVolume);
			}
		}
	}

	public void MingleScan(Cell C)
	{
		int num = C.Objects.Count - 1;
		while (num >= 0 && num < C.Objects.Count)
		{
			MingleScan(C.Objects[num]);
			num--;
		}
	}

	public void MingleLiquids()
	{
		if (ParentObject.HasPart<LiquidVolume>() && ParentObject.CurrentCell != null)
		{
			List<Cell> adjacentCells = ParentObject.CurrentCell.GetAdjacentCells();
			for (int i = 0; i < adjacentCells.Count; i++)
			{
				MingleScan(adjacentCells[i]);
			}
		}
	}

	public void CheckMingleLiquids()
	{
		if (ShouldMingleLiquids())
		{
			MingleLiquids();
		}
	}

	public string GetBehaviorDescription()
	{
		if (IsConduit)
		{
			return "Functions as part of " + Grammar.A(Assembly) + ", " + Activity + " " + GetPowerTransmissionType() + " " + Substance + " between locations.";
		}
		if (IsProducer)
		{
			return "Contains " + Constituent + " enabling " + ParentObject.them + " to function as part of " + Grammar.A(Assembly) + ", producing " + GetPowerTransmissionType() + " " + Substance + ".";
		}
		if (IsConsumer)
		{
			return "Contains " + Constituent + " enabling " + ParentObject.them + " to function as part of " + Grammar.A(Assembly) + ", consuming " + GetPowerTransmissionType() + " " + Substance + ".";
		}
		return "Contains " + Constituent + " enabling " + ParentObject.them + " to function as part of " + Grammar.A(Assembly) + ", " + Activity + " " + GetPowerTransmissionType() + " " + Substance + " between locations.";
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (!DependsOnLiquid.IsNullOrEmpty())
		{
			return DependsOnLiquid == LiquidType;
		}
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		TurnProcessing();
	}

	private void TurnProcessing()
	{
		Cell cell = ParentObject.FastGetCurrentCell();
		if (cell == null || cell.ParentZone.IsActive())
		{
			CheckSparking();
			CheckMingleLiquids();
			CheckDischargeLiquid();
		}
	}

	public long GetGridBit()
	{
		return GridBit;
	}

	public List<GameObject> GetGrid()
	{
		CheckGrid();
		return Grid;
	}

	private void Process(IChargeProductionEvent E)
	{
		if (!IsProducer)
		{
			return;
		}
		long gridMask = E.GridMask;
		if ((gridMask & GridBit) != 0L)
		{
			return;
		}
		long gridMask2 = E.GridMask | GridBit;
		if (!IsReady(UseCharge: false, IgnoreCharge: true, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, gridMask2))
		{
			return;
		}
		CheckGrid();
		if (Consumers == null || Consumers.Count <= 0)
		{
			return;
		}
		int amount = E.Amount;
		if (amount <= 0)
		{
			return;
		}
		int num = amount;
		int num2 = 0;
		int iD = E.GetID();
		int cascadeLevel = E.GetCascadeLevel();
		E.GridMask |= GridBit;
		try
		{
			int i = 0;
			for (int count = Consumers.Count; i < count; i++)
			{
				GameObject gameObject = Consumers[i];
				if (!gameObject.WantEvent(iD, cascadeLevel))
				{
					continue;
				}
				int gridCapacity = GridCapacity;
				if (gridCapacity <= num2)
				{
					continue;
				}
				int num3 = gridCapacity - num2;
				int num4 = (E.Amount = ((num > num3) ? num3 : num));
				gameObject.HandleEvent(E);
				int amount2 = E.Amount;
				if (amount2 < num4)
				{
					int num5 = num4 - amount2;
					num2 += num5;
					num -= num5;
					HadChargeActivity();
					if (num <= 0)
					{
						break;
					}
				}
			}
		}
		finally
		{
			E.Amount = num;
			E.GridMask = gridMask;
		}
	}
}

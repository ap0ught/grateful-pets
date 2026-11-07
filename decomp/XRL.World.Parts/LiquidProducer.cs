using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LiquidProducer : IPoweredPart
{
	public int Tick;

	public int Rate = 1000;

	public int ChanceSkipSelf;

	public int ChanceSkipSameCell;

	public string Liquid = "water";

	public string VariableRate;

	public bool PreferCollectors;

	public bool PureOnFloor;

	public bool FillSelfOnly;

	[NonSerialized]
	private static LiquidVolume produced = new LiquidVolume();

	public LiquidProducer()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		LiquidProducer liquidProducer = p as LiquidProducer;
		if (liquidProducer.Rate != Rate)
		{
			return false;
		}
		if (liquidProducer.ChanceSkipSelf != ChanceSkipSelf)
		{
			return false;
		}
		if (liquidProducer.ChanceSkipSameCell != ChanceSkipSameCell)
		{
			return false;
		}
		if (liquidProducer.Liquid != Liquid)
		{
			return false;
		}
		if (liquidProducer.VariableRate != VariableRate)
		{
			return false;
		}
		if (liquidProducer.PreferCollectors != PreferCollectors)
		{
			return false;
		}
		if (liquidProducer.PureOnFloor != PureOnFloor)
		{
			return false;
		}
		if (liquidProducer.FillSelfOnly != FillSelfOnly)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AllowLiquidCollectionEvent.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == ProducesLiquidEvent.ID;
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

	public override bool HandleEvent(ProducesLiquidEvent E)
	{
		if (E.Liquid == Liquid)
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
		Process(Amount);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void Process(int Turns = 1)
	{
		if (!IsNeeded() || !IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L))
		{
			return;
		}
		Tick += Turns;
		while (Tick >= Rate)
		{
			Tick -= Rate;
			if (string.IsNullOrEmpty(ConsumesLiquid) || ConsumeLiquid(ConsumesLiquid, LiquidConsumptionAmount, !LiquidMustBePure))
			{
				DistributeLiquid();
			}
			if (!string.IsNullOrEmpty(VariableRate))
			{
				Rate = VariableRate.RollCached();
			}
		}
	}

	public bool IsNeeded()
	{
		if (!FillSelfOnly)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null && !cell.OnWorldMap())
			{
				return true;
			}
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && liquidVolume.Volume >= liquidVolume.MaxVolume)
		{
			if (liquidVolume.IsPureLiquid(Liquid))
			{
				return false;
			}
			if (string.IsNullOrEmpty(ConsumesLiquid))
			{
				return false;
			}
			if (!liquidVolume.ComponentLiquids.ContainsKey(ConsumesLiquid))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsLiquidCollectionCompatible(string LiquidType)
	{
		if (Liquid == LiquidType)
		{
			return true;
		}
		return WantsLiquidCollection(LiquidType);
	}

	public bool DistributeLiquid()
	{
		if (!produced.ComponentLiquids.ContainsKey(Liquid) || produced.ComponentLiquids.Count > 1)
		{
			if (produced.ComponentLiquids.Count > 0)
			{
				produced.ComponentLiquids.Clear();
			}
			produced.ComponentLiquids.Add(Liquid, 1000);
		}
		produced.MaxVolume = 1;
		produced.Volume = 1;
		LiquidVolume V;
		if (FillSelfOnly || !ChanceSkipSelf.in100())
		{
			V = ParentObject.LiquidVolume;
			if (V != null && V.Volume < V.MaxVolume)
			{
				V.MixWith(produced);
				return true;
			}
		}
		if (FillSelfOnly)
		{
			return false;
		}
		Cell C = ParentObject.GetCurrentCell();
		if (C == null || C.ParentZone.IsWorldMap())
		{
			return false;
		}
		bool result;
		if (!ChanceSkipSameCell.in100() && !C.IsSolid(ForFluid: true))
		{
			result = C.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
			{
				V = GO.LiquidVolume;
				if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
				{
					V.MixWith(produced);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
			result = C.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
			{
				V = GO.LiquidVolume;
				if (V.MaxVolume == -1)
				{
					V.MixWith(produced);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
			GameObject gameObject = GameObject.Create("Water");
			V = gameObject.LiquidVolume;
			V.Volume = produced.Volume;
			if (PureOnFloor || string.IsNullOrEmpty(C.GroundLiquid))
			{
				V.InitialLiquid = Liquid + "-1000";
			}
			else
			{
				V.InitialLiquid = C.GroundLiquid;
				V.MixWith(produced);
			}
			C.AddObject(gameObject);
			return true;
		}
		int num = 0;
		result = C.ForeachLocalAdjacentCell((Cell AC) =>
		{
			if (PreferCollectors)
			{
				result = AC.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
				{
					V = GO.LiquidVolume;
					if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
					{
						V.MixWith(produced);
						return false;
					}
					return true;
				});
				if (!result)
				{
					return false;
				}
			}
			if (!AC.IsSolid(ForFluid: true))
			{
				num++;
			}
			return true;
		});
		if (!result)
		{
			return true;
		}
		if (num > 0)
		{
			int select = Stat.Random(1, num);
			int pos = 0;
			result = C.ForeachLocalAdjacentCell((Cell AC) =>
			{
				if (++pos == select)
				{
					result = AC.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
					{
						V = GO.LiquidVolume;
						if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
						{
							V.MixWith(produced);
							return false;
						}
						return true;
					});
					if (!result)
					{
						return false;
					}
					result = AC.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
					{
						V = GO.LiquidVolume;
						if (V.MaxVolume == -1)
						{
							V.MixWith(produced);
							return false;
						}
						return true;
					});
					if (!result)
					{
						return false;
					}
					GameObject gameObject2 = GameObject.Create("Water");
					V = gameObject2.LiquidVolume;
					V.Volume = produced.Volume;
					if (PureOnFloor || string.IsNullOrEmpty(C.GroundLiquid))
					{
						V.InitialLiquid = Liquid + "-1000";
					}
					else
					{
						V.InitialLiquid = C.GroundLiquid;
						V.MixWith(produced);
					}
					AC.AddObject(gameObject2);
					return false;
				}
				return true;
			});
			if (!result)
			{
				return true;
			}
		}
		return false;
	}
}

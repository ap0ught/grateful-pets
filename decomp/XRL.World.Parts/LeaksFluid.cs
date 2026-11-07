using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LeaksFluid : IPoweredPart
{
	public int Tick;

	public int Rate = -1;

	public int ChanceSmearParent = 100;

	public string SmearParentDuration = "100-200";

	public int ChanceSkipSelf;

	public int ChanceSkipSameCell;

	public string Liquid = "water";

	public string Frequency = "300-400";

	public string Drams = "2-3";

	public bool PreferCollectors;

	public bool PureOnFloor;

	public bool FillSelfOnly;

	public bool Message = true;

	public string MessageVerb = "leak";

	[NonSerialized]
	private static LiquidVolume produced = new LiquidVolume();

	public LeaksFluid()
	{
		ChargeUse = 0;
		IsEMPSensitive = false;
		WorksOnSelf = true;
	}

	public override bool SameAs(IPart p)
	{
		LeaksFluid leaksFluid = p as LeaksFluid;
		if (leaksFluid.Rate != Rate)
		{
			return false;
		}
		if (leaksFluid.ChanceSmearParent != ChanceSmearParent)
		{
			return false;
		}
		if (leaksFluid.SmearParentDuration != SmearParentDuration)
		{
			return false;
		}
		if (leaksFluid.ChanceSkipSelf != ChanceSkipSelf)
		{
			return false;
		}
		if (leaksFluid.ChanceSkipSameCell != ChanceSkipSameCell)
		{
			return false;
		}
		if (leaksFluid.Liquid != Liquid)
		{
			return false;
		}
		if (leaksFluid.Frequency != Frequency)
		{
			return false;
		}
		if (leaksFluid.Drams != Drams)
		{
			return false;
		}
		if (leaksFluid.PreferCollectors != PreferCollectors)
		{
			return false;
		}
		if (leaksFluid.PureOnFloor != PureOnFloor)
		{
			return false;
		}
		if (leaksFluid.FillSelfOnly != FillSelfOnly)
		{
			return false;
		}
		if (leaksFluid.Message != Message)
		{
			return false;
		}
		if (leaksFluid.MessageVerb != MessageVerb)
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
		if (!string.IsNullOrEmpty(Frequency))
		{
			Rate = Frequency.RollCached();
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
		if (IsNeeded() && IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: true, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, Turns, null, UseChargeIfUnpowered: false, 0L) && ++Tick >= Rate)
		{
			if (string.IsNullOrEmpty(ConsumesLiquid) || ConsumeLiquid(ConsumesLiquid, LiquidConsumptionAmount, LiquidMustBePure))
			{
				DistributeLiquid();
			}
			Tick = 0;
			if (!string.IsNullOrEmpty(Frequency))
			{
				Rate = Frequency.RollCached();
			}
		}
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
		produced.MaxVolume = Drams.RollCached();
		produced.Volume = produced.MaxVolume;
		LiquidVolume V;
		if (FillSelfOnly || ChanceSkipSelf.in100())
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
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.ParentZone.IsWorldMap())
		{
			return false;
		}
		string text = ((produced.Volume == 1) ? "dram" : "drams");
		if (Message && ParentObject.IsVisible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.Does(MessageVerb) + " " + produced.Volume + " " + text + " of " + produced.GetLiquidName() + ".");
		}
		if (ChanceSmearParent.in100())
		{
			ParentObject.ApplyEffect(new LiquidCovered(produced, 1, SmearParentDuration.RollCached(), Poured: true));
		}
		bool result;
		if (ChanceSkipSameCell < 100 && !cell.IsSolid(ForFluid: true) && !ChanceSkipSameCell.in100())
		{
			result = cell.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
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
			result = cell.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
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
			if (PureOnFloor || string.IsNullOrEmpty(cell.GroundLiquid))
			{
				V.InitialLiquid = Liquid + "-1000";
			}
			else
			{
				V.InitialLiquid = cell.GroundLiquid + "-1000";
				V.MixWith(produced);
			}
			cell.AddObject(gameObject);
			return true;
		}
		int num = 0;
		result = cell.ForeachLocalAdjacentCell((Cell AC) =>
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
			result = cell.ForeachLocalAdjacentCell((Cell AC) =>
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
					if (PureOnFloor || string.IsNullOrEmpty(AC.GroundLiquid))
					{
						V.InitialLiquid = Liquid + "-1000";
					}
					else
					{
						V.InitialLiquid = AC.GroundLiquid + "-1000";
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

	public bool IsNeeded()
	{
		if (!FillSelfOnly)
		{
			return true;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && liquidVolume.Volume >= liquidVolume.MaxVolume)
		{
			if (liquidVolume.ComponentLiquids.Count == 1 && liquidVolume.ComponentLiquids.ContainsKey(Liquid))
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
}

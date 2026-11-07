using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Gas : IPart
{
	public int _Density = 100;

	public int Level = 1;

	public bool Seeping;

	public bool Stable;

	public string GasType = "BaseGas";

	public string ColorString;

	[NonSerialized]
	private int FrameOffset;

	public GameObject _Creator;

	public GameObject Creator
	{
		get
		{
			GameObject.Validate(ref _Creator);
			return _Creator;
		}
		set
		{
			_Creator = value;
		}
	}

	public int Density
	{
		get
		{
			return _Density;
		}
		set
		{
			if (_Density != value && ParentObject != null && ParentObject.HasRegisteredEvent("DensityChange"))
			{
				ParentObject.FireEvent(Event.New("DensityChange", "OldValue", _Density, "NewValue", value));
			}
			_Density = value;
		}
	}

	public Gas()
	{
		FrameOffset = Stat.Random(0, 60);
	}

	public override void Initialize()
	{
		if (ParentObject.Render != null)
		{
			ParentObject.Render.RenderString = "°";
			if (!ColorString.IsNullOrEmpty())
			{
				ParentObject.Render.ColorString = ColorString;
			}
		}
	}

	public override bool SameAs(IPart Part)
	{
		Gas gas = Part as Gas;
		if (gas._Density != _Density)
		{
			return false;
		}
		if (gas.Level != Level)
		{
			return false;
		}
		if (gas.Seeping != Seeping)
		{
			return false;
		}
		if (gas.Stable != Stable)
		{
			return false;
		}
		if (gas.GasType != GasType)
		{
			return false;
		}
		if (gas.ColorString != ColorString)
		{
			return false;
		}
		if (gas._Creator != _Creator)
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<GeneralAmnestyEvent>.ID && ID != SingletonEvent<GetDebugInternalsEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID)
		{
			return ID == ObjectEnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetDebugInternalsEvent E)
	{
		E.AddEntry(this, "Density", Density);
		E.AddEntry(this, "Level", Level);
		E.AddEntry(this, "Seeping", Seeping);
		E.AddEntry(this, "Stable", Stable);
		E.AddEntry(this, "GasType", GasType);
		E.AddEntry(this, "ColorString", ColorString);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GeneralAmnestyEvent E)
	{
		Creator = null;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(3);
		return false;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(ObjectEnteredCellEvent E)
	{
		GameObject.Validate(ref _Creator);
		if (CheckMergeGas(E.Object))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (ProcessGasBehavior() && Amount > 1)
		{
			Disperse(Amount);
		}
	}

	public override bool Render(RenderEvent E)
	{
		if (ColorString != null)
		{
			E.ColorString = ColorString;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 60;
		if (num < 15)
		{
			E.RenderString = "◄";
			E.Tile = "Tiles2/gas_0.png";
		}
		else if (num < 30)
		{
			E.RenderString = "▒";
			E.Tile = "Tiles2/gas_1.png";
		}
		else if (num < 45)
		{
			E.RenderString = "▓";
			E.Tile = "Tiles2/gas_2.png";
		}
		else
		{
			E.RenderString = "█";
			E.Tile = "Tiles2/gas_3.png";
		}
		if (Density < 50 && GasType != "Cryo")
		{
			E.BackgroundString = "^k";
		}
		return true;
	}

	public bool ProcessGasBehavior()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		int num = 0;
		string text = null;
		Zone parentZone = cell.ParentZone;
		if (parentZone != null && GlobalConfig.GetBoolSetting("WindAffectsGasDispersal"))
		{
			num = parentZone.CurrentWindSpeed;
			text = parentZone.CurrentWindDirection;
		}
		GameObject.Validate(ref _Creator);
		if (Density > 10)
		{
			if (!Stable)
			{
				Density -= GetDispersalRate();
			}
			if ((25 + num).in100())
			{
				int i = 0;
				for (int num2 = Stat.Random(1 + num / 30, 4 + num / 20); i < num2; i++)
				{
					string direction = ((num.in100() && 90.in100()) ? text : null) ?? Directions.GetRandomDirection();
					Cell localCellFromDirection = cell.GetLocalCellFromDirection(direction);
					if (localCellFromDirection == null)
					{
						continue;
					}
					bool flag = false;
					List<GameObject> list = null;
					if (Seeping || !localCellFromDirection.IsSolidFor(ParentObject))
					{
						if (Stable)
						{
							flag = localCellFromDirection.IsEmpty();
							if (localCellFromDirection.GetObjectCountWithPart("Gas") > 0)
							{
								if (list == null)
								{
									list = Event.NewGameObjectList();
									localCellFromDirection.GetObjectsWithPart("Gas", list);
								}
								int j = 0;
								for (int count = list.Count; j < count; j++)
								{
									GameObject gameObject = list[j];
									if (gameObject.PhaseMatches(ParentObject))
									{
										ParentObject.FireEvent(Event.New("GasPressureOut", "Object", gameObject));
										gameObject.FireEvent(Event.New("GasPressureIn", "Object", ParentObject));
										flag = false;
									}
								}
							}
						}
						else
						{
							flag = true;
						}
					}
					if (!flag)
					{
						continue;
					}
					int num3 = Stat.Random(1, Math.Min(Density, 30));
					if (list == null)
					{
						list = Event.NewGameObjectList();
						localCellFromDirection.GetObjectsWithPart("Gas", list);
					}
					bool flag2 = false;
					int k = 0;
					for (int count2 = list.Count; k < count2; k++)
					{
						if (CheckMergeToGas(list[k], num3))
						{
							flag2 = true;
							break;
						}
					}
					if (!flag2)
					{
						GameObject gameObject2 = GameObject.Create(ParentObject.Blueprint);
						if (gameObject2.TryGetPart<Gas>(out var Part))
						{
							Part.Creator = Creator;
							Part.Density = num3;
							Part.ColorString = ColorString;
							Part.Level = Level;
							Part.Seeping = Seeping;
						}
						Density -= num3;
						gameObject2.FireEvent(Event.New("GasSpawned", "Parent", ParentObject));
						Temporary.CarryOver(ParentObject, gameObject2);
						Phase.carryOver(ParentObject, gameObject2);
						localCellFromDirection.AddObject(gameObject2);
					}
					if (Density <= 0)
					{
						break;
					}
				}
			}
		}
		if (Density <= 0 || (Density <= 10 && (50 + num).in100()))
		{
			Dissipate();
			return false;
		}
		return true;
	}

	public void Dissipate()
	{
		if (ParentObject.IsPlayer() || ParentObject.HasTag("Creature"))
		{
			ParentObject.Die(null, "dissipation", "You dissipated.", ParentObject.It + " @@dissipated.", Accidental: true);
		}
		else
		{
			ParentObject.Obliterate(null, Silent: true);
		}
	}

	public void Disperse(int Factor = 1)
	{
		Density -= GetDispersalRate(Factor);
		if (Density < 0 || (Density <= 10 && 50.in100()))
		{
			Dissipate();
		}
	}

	public int GetDispersalRate(int Factor = 1)
	{
		int num = Stat.Random(Factor, Factor * 3);
		if (Creator != null && Creator.DistanceTo(ParentObject) <= 1 && Creator.HasRegisteredEvent("CreatorModifyGasDispersal"))
		{
			Event obj = new Event("CreatorModifyGasDispersal", "Rate", num);
			Creator.FireEvent(obj);
			num = obj.GetIntParameter("Rate");
		}
		return num;
	}

	private bool IsGasMergeable(Gas gas)
	{
		if (gas != null && gas.GasType == GasType)
		{
			return gas.ColorString == ColorString;
		}
		return false;
	}

	private void MergeGas(Gas gas)
	{
		Density += gas.Density;
		if (gas.Level > Level)
		{
			Level = gas.Level;
		}
		if (gas.Seeping && !Seeping)
		{
			Seeping = true;
		}
		if (gas.Creator != null && Creator == null)
		{
			Creator = gas.Creator;
		}
	}

	private void MergeToGas(Gas gas, int Contribution)
	{
		if (Contribution > Density)
		{
			Contribution = Density;
		}
		gas.Density += Contribution;
		if (Level > gas.Level)
		{
			gas.Level = Level;
		}
		if (Seeping && !gas.Seeping)
		{
			gas.Seeping = true;
		}
		if (Creator != null && gas.Creator == null)
		{
			gas.Creator = Creator;
		}
		Density -= Contribution;
	}

	private bool CheckMergeGas(GameObject obj)
	{
		if (obj != ParentObject && obj.TryGetPart<Gas>(out var Part) && IsGasMergeable(Part) && obj.PhaseMatches(ParentObject))
		{
			MergeGas(Part);
			Part.Dissipate();
			return true;
		}
		return false;
	}

	private bool CheckMergeToGas(GameObject obj, int Contribution)
	{
		if (obj != ParentObject && obj.TryGetPart<Gas>(out var Part) && IsGasMergeable(Part) && obj.PhaseMatches(ParentObject))
		{
			MergeToGas(Part, Contribution);
			return true;
		}
		return false;
	}
}

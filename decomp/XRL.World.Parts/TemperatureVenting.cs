using System;
using System.Collections.Generic;
using UnityEngine;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class TemperatureVenting : IPart
{
	public int TemperatureTarget = 25;

	public int TemperatureCeiling = 2000;

	public int TemperatureFloor = -100;

	public int DifferentialPercent = 20;

	public int CloudRadius = 1;

	public int Warmup = 3;

	public int CurrentWarmup;

	[NonSerialized]
	private List<GameObject> GasObjects = new List<GameObject>();

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		int temperature = ParentObject.Physics.Temperature;
		if (temperature > TemperatureCeiling || temperature < TemperatureFloor)
		{
			CurrentWarmup += Amount;
			if (CurrentWarmup >= Warmup)
			{
				Trigger();
				CurrentWarmup = 0;
			}
		}
		else if (CurrentWarmup > 0)
		{
			CurrentWarmup = Math.Max(0, CurrentWarmup - Amount);
		}
	}

	public void Trigger()
	{
		int temperature = ParentObject.Physics.Temperature;
		float num = (float)DifferentialPercent / 100f;
		int num2 = temperature - TemperatureTarget;
		ParentObject.TemperatureChange(-Mathf.RoundToInt((float)num2 * num), null, Radiant: false, MinAmbient: false, MaxAmbient: false, IgnoreResistance: true);
		PlayWorldSound("sfx_ability_gasMutation_activeRelease");
		DidX("release", (num2 > 0) ? "a cloud of steam to cool off" : "a cloud of cryogenic mist to heat up", "!", null, null, ParentObject);
		Cell cell = ParentObject.CurrentCell;
		List<GameObject> gasObjects = GasObjects;
		try
		{
			foreach (Cell item in cell.IterateAdjacent(CloudRadius, IncludeSelf: true, LocalOnly: true))
			{
				foreach (GameObject @object in item.Objects)
				{
					if (@object.HasPart<Gas>())
					{
						gasObjects.Add(@object);
					}
				}
			}
			foreach (GameObject item2 in gasObjects)
			{
				string text = ParentObject.GetDirectionToward(item2);
				if (text == "." || text == "?")
				{
					text = Directions.GetRandomDirection();
				}
				item2.CurrentCell.GetCellFromDirection(text)?.AddObject(item2);
			}
		}
		finally
		{
			gasObjects.Clear();
		}
		foreach (Cell item3 in cell.IterateAdjacent(CloudRadius))
		{
			item3.AddObject((num2 > 0) ? "SteamGas80" : "CryoGas80");
		}
	}
}

using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class ExtinguishSelf : GoalHandler
{
	public GameObject Target;

	public int PoolTries;

	[NonSerialized]
	public static List<AICommandList> CommandList = new List<AICommandList>();

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return !ParentObject.IsAflame();
	}

	public GameObject FindPoolInRadius(int Radius)
	{
		Cell currentCell = ParentObject.CurrentCell;
		List<Cell> list = currentCell.ParentZone.FastFloodNeighbors(currentCell.X, currentCell.Y, Radius);
		GameObject result = null;
		float num = 0f;
		int num2 = int.MaxValue;
		foreach (Cell item in list)
		{
			if (item.IsSolidFor(ParentObject) || item.HasObjectWithPart("Combat") || !item.IsReachable())
			{
				continue;
			}
			foreach (GameObject @object in item.Objects)
			{
				LiquidVolume liquidVolume = @object.LiquidVolume;
				if (liquidVolume == null || !liquidVolume.IsWadingDepth())
				{
					continue;
				}
				float liquidCooling = liquidVolume.GetLiquidCooling();
				if (liquidCooling > 1f)
				{
					int num3 = currentCell.PathDistanceTo(item);
					if (liquidCooling > num || (liquidCooling == num && num3 < num2))
					{
						result = @object;
						num = liquidCooling;
						num2 = num3;
					}
				}
			}
		}
		return result;
	}

	public GameObject FindDousingContainer()
	{
		GameObject BestContainer = null;
		float BestContainerCooling = 0f;
		int BestContainerVolume = 0;
		ParentObject.ForeachInventoryAndEquipment((GameObject obj) =>
		{
			LiquidVolume liquidVolume = obj.LiquidVolume;
			if (liquidVolume != null && liquidVolume.Volume > 0 && !liquidVolume.EffectivelySealed())
			{
				float liquidCooling = liquidVolume.GetLiquidCooling();
				if (!(liquidCooling < 1f) && (liquidCooling > BestContainerCooling || (liquidCooling == BestContainerCooling && liquidVolume.Volume > BestContainerVolume)))
				{
					BestContainer = obj;
					BestContainerCooling = liquidCooling;
					BestContainerVolume = liquidVolume.Volume;
				}
			}
		});
		return BestContainer;
	}

	public override void TakeAction()
	{
		if (!GameObject.Validate(ref Target))
		{
			Target = null;
		}
		int num = ParentObject.Stat("Intelligence");
		if (num > 3 && Stat.Random(3, 21) < num && ++PoolTries < 3)
		{
			Target = FindPoolInRadius(3);
			if (Target != null)
			{
				Cell currentCell = Target.CurrentCell;
				if (ParentObject.IsFlying)
				{
					PushChildGoal(new Land(overridesCombat: true));
				}
				PushChildGoal(new MoveTo(currentCell.ParentZone.ZoneID, currentCell.X, currentCell.Y, careful: false, overridesCombat: true), ParentHandler);
				return;
			}
		}
		if (num >= 7 && Stat.Random(6, 25) < num)
		{
			GameObject gameObject = FindDousingContainer();
			if (gameObject != null)
			{
				LiquidVolume liquidVolume = gameObject.LiquidVolume;
				float liquidCooling = liquidVolume.GetLiquidCooling();
				if (liquidVolume.Pour(PourAmount: (int)Math.Ceiling((float)(ParentObject.Physics.Temperature / ParentObject.Physics.FlameTemperature) / liquidCooling), Actor: ParentObject, TargetCell: null, Forced: false, Douse: true))
				{
					return;
				}
			}
		}
		if (Firefighting.NeedsToLandToAttemptFirefighting(ParentObject, ParentObject))
		{
			PushChildGoal(new Land(overridesCombat: true));
		}
		else
		{
			if (Firefighting.AttemptFirefighting(ParentObject, ParentObject) || num <= 3 || Stat.Random(3, 21) >= num)
			{
				return;
			}
			Target = FindPoolInRadius(40);
			if (Target != null)
			{
				Cell currentCell2 = Target.CurrentCell;
				if (ParentObject.IsFlying)
				{
					PushChildGoal(new Land(overridesCombat: true));
				}
				PushChildGoal(new MoveTo(currentCell2.ParentZone.ZoneID, currentCell2.X, currentCell2.Y, careful: false, overridesCombat: true), ParentHandler);
			}
		}
	}
}

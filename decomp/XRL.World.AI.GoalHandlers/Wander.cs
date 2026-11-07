using System;
using System.Collections.Generic;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class Wander : IMovementGoal
{
	public Cell Target;

	[NonSerialized]
	public static List<AICommandList> CommandList = new List<AICommandList>();

	public override bool IsBusy()
	{
		return false;
	}

	private bool SuitableToWander(Cell C, Cell CC, int MaxWeight, ref int Nav)
	{
		if (C.IsOccluding())
		{
			return false;
		}
		if (C.IsSolidFor(ParentObject))
		{
			return false;
		}
		if (CC.PathDistanceTo(C) > ParentBrain.MaxWanderRadius)
		{
			return false;
		}
		if (!C.IsReachable())
		{
			return false;
		}
		if (C.HasObjectWithTagOrProperty("WanderStopper"))
		{
			return false;
		}
		if (C.NavigationWeight(ParentObject, ref Nav) >= MaxWeight)
		{
			return false;
		}
		return true;
	}

	public override void Create()
	{
		Pop();
		Cell currentCell = ParentObject.CurrentCell;
		Zone parentZone = currentCell.ParentZone;
		int Nav = 268435456;
		int num = 0;
		Cell randomCell = parentZone.GetRandomCell();
		while (!SuitableToWander(randomCell, currentCell, (num < 50) ? 3 : 10, ref Nav))
		{
			if (++num >= 100)
			{
				return;
			}
			randomCell = parentZone.GetRandomCell();
		}
		Target = randomCell;
		Think("I'll wander around to " + Target.X + "," + Target.Y);
		if (Target.HasObjectWithPart("StairsUp") && ParentObject.HasTagOrProperty("WanderUpStairs"))
		{
			PushChildGoal(new Step("U", careful: true, overridesCombat: false, wandering: true), ParentHandler);
		}
		else if (Target.HasObjectWithPart("StairsDown") && ParentObject.HasTagOrProperty("WanderDownStairs"))
		{
			PushChildGoal(new Step("D", careful: true, overridesCombat: false, wandering: true), ParentHandler);
		}
		PushChildGoal(new MoveTo(parentZone.ZoneID, Target.X, Target.Y, careful: true, overridesCombat: false, 0, wandering: true), ParentHandler);
	}

	public override void TakeAction()
	{
	}
}

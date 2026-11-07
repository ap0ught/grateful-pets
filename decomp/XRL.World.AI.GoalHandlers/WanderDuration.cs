using System;
using System.Collections.Generic;
using XRL.Core;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class WanderDuration : IMovementGoal
{
	public long duration;

	public long endturn;

	public Cell Target;

	[NonSerialized]
	public static List<AICommandList> CommandList = new List<AICommandList>();

	public WanderDuration()
	{
	}

	public WanderDuration(long duration)
		: this()
	{
		endturn = XRLCore.Core.Game.Turns + duration;
		this.duration = duration;
	}

	public override bool IsBusy()
	{
		return false;
	}

	public override void Create()
	{
		Cell currentCell = ParentObject.CurrentCell;
		Zone parentZone = currentCell.ParentZone;
		Cell randomCell = parentZone.GetRandomCell();
		int Nav = 268435456;
		int num = 0;
		while (randomCell.IsOccluding() || currentCell.PathDistanceTo(randomCell) > duration || randomCell.IsSolidFor(ParentObject) || !randomCell.IsReachable() || randomCell.HasObjectWithTagOrProperty("WanderStopper") || randomCell.NavigationWeight(ParentObject, ref Nav) >= 10)
		{
			if (++num >= 300)
			{
				return;
			}
			randomCell = parentZone.GetRandomCell();
		}
		Target = randomCell;
		Think("I'll wander around to " + Target.X + "," + Target.Y);
		PushChildGoal(new MoveTo(parentZone.ZoneID, Target.X, Target.Y, careful: true, overridesCombat: false, 0, wandering: true), ParentHandler);
	}

	public override void TakeAction()
	{
		if (XRLCore.Core.Game.Turns >= endturn)
		{
			Pop();
		}
		else
		{
			Create();
		}
	}
}

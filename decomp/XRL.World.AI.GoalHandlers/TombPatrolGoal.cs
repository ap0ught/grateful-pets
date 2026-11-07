using System;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class TombPatrolGoal : GoalHandler
{
	public string TargetZoneID = "JoppaWorld.5.2.1.1.10";

	public TombPatrolGoal()
	{
	}

	public TombPatrolGoal(string ZoneID)
		: this()
	{
		TargetZoneID = ZoneID;
	}

	public TombPatrolGoal(GlobalLocation loc)
		: this(loc.ZoneID)
	{
	}

	public override bool Finished()
	{
		return ParentObject.InZone(TargetZoneID);
	}

	public override void Create()
	{
	}

	public void MoveToTargetZone()
	{
	}

	public override void TakeAction()
	{
		ParentBrain.PushGoal(new MoveToZone(TargetZoneID));
	}
}

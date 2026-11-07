using System;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MoveToInterior : IMovementGoal
{
	[NonSerialized]
	public GameObject Target;

	[NonSerialized]
	public Interior Interior;

	public MoveToInterior()
	{
	}

	public MoveToInterior(GameObject Target)
		: this()
	{
		this.Target = Target;
		Interior = Target?.GetPart<Interior>();
	}

	public override bool Finished()
	{
		if (GameObject.Validate(Target))
		{
			return CurrentZone.ZoneID == Interior.ZoneID;
		}
		return true;
	}

	public override void TakeAction()
	{
		int num = ParentObject.DistanceTo(Target);
		if (num == 9999999)
		{
			Target = null;
		}
		else if (num > 1)
		{
			PushChildGoal(new MoveTo(Target, careful: false, overridesCombat: false, 1));
		}
		else if (!Interior.TryEnter(ParentObject, Force: false, ParentObject.IsPlayerControlled()))
		{
			Target = null;
		}
	}
}

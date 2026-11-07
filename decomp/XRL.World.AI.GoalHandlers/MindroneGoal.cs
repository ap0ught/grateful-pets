using System;
using XRL.World.AI.Pathfinding;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class MindroneGoal : GoalHandler
{
	public GameObject Target;

	public int LastSeen;

	public MindroneGoal()
	{
	}

	public MindroneGoal(GameObject Target)
		: this()
	{
		this.Target = Target;
	}

	public override void Create()
	{
		Think("I'm trying to heal " + Target.Blueprint + "!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void TakeAction()
	{
		if (Target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
			return;
		}
		if (Target.IsInvalid())
		{
			Target = null;
			Think("My target has been destroyed!");
			FailToParent();
			return;
		}
		if (Target.IsNowhere())
		{
			Think("My target is dead!");
			Target = null;
			FailToParent();
			return;
		}
		int num = Target.DistanceTo(ParentObject);
		if (num > 80)
		{
			LastSeen++;
		}
		if (LastSeen > 5)
		{
			Think("I can't find my target...");
			Target = null;
			FailToParent();
		}
		else if (num <= 1)
		{
			Think("I'm going to heal my target!");
			Target.Sparksplatter();
			Target.Heal(3, Message: true, FloatText: true);
			if (!Target.isDamaged())
			{
				Pop();
			}
		}
		else if (ParentObject.IsMobile())
		{
			Think("I'm going to move towards my target.");
			if (Target.OnWorldMap())
			{
				Think("Target's on the world map, can't follow!");
				Target = null;
				FailToParent();
				return;
			}
			FindPath findPath = new FindPath(ParentObject.CurrentCell, Target.CurrentCell, Target.IsPlayer(), PathUnlimited: true, ParentObject);
			if (findPath.Usable)
			{
				PushChildGoal(new Step(findPath.Directions[0]));
				return;
			}
			Think("I can't find a path to my target!");
			FailToParent();
		}
		else
		{
			ParentObject.UseEnergy(1000);
			Think("My target is too far and I'm immobile.");
			FailToParent();
		}
	}
}

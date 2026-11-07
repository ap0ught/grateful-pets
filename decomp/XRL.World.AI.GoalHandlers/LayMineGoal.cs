using System;
using Genkit;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class LayMineGoal : GoalHandler
{
	public Location2D Target;

	public string Mine;

	public string MineName;

	public string Timer = "-1";

	public int HideDifficulty;

	public LayMineGoal(Location2D Target, string Mine, string MineName = "", string Timer = "-1", int HideDifficulty = 0)
	{
		this.Target = Target;
		this.Mine = Mine;
		this.MineName = MineName;
		this.Timer = Timer;
		this.HideDifficulty = HideDifficulty;
	}

	public override void Create()
	{
		Think("I'm trying to lay mines!");
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
		Miner Part;
		if (Target == null)
		{
			Think("I don't have a target anymore!");
			FailToParent();
		}
		else if (!ParentObject.TryGetPart<Miner>(out Part))
		{
			Think("I'm not a miner any more!");
			FailToParent();
		}
		else if (!Part.IsMyActivatedAbilityAIUsable(Part.ActivatedAbilityID))
		{
			Think("I'm not supposed to be laying mines any more!");
			FailToParent();
		}
		else if (ParentObject.DistanceTo(Target) == 1)
		{
			int num = Timer.RollCached();
			Think((num > 0) ? "I'm going to set a bomb!" : "I'm going to lay a mine!");
			GameObject gameObject = ((num > 0) ? Tinkering_LayMine.CreateBomb(Mine, ParentObject, num) : Tinkering_LayMine.CreateMine(Mine, ParentObject));
			if (HideDifficulty > 0)
			{
				gameObject.RequirePart<Hidden>().Difficulty = HideDifficulty;
			}
			ParentObject.CurrentCell.AddObject(gameObject);
			ParentObject.UseEnergy(1000);
			FailToParent();
			ParentObject.Brain.DidXToY("place", gameObject, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
		}
		else if (!MoveTowards(ParentObject.CurrentZone.GetCell(Target)))
		{
			Think("I can't get to my target!");
			FailToParent();
		}
	}
}

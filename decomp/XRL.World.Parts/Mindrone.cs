using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Mindrone : IPart
{
	public int HealCooldown;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (HealCooldown <= 0 && !ParentObject.HasGoal("MindroneGoal"))
		{
			List<GameObject> list = ParentObject.CurrentZone.FastSquareVisibility(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y, 12, "Combat", ParentObject);
			List<GameObject> list2 = Event.NewGameObjectList();
			foreach (GameObject item in list)
			{
				if (!item.IsPlayer() && item.HasPart<Metal>() && item.isDamaged())
				{
					list2.Add(item);
				}
			}
			if (list2.Count == 0)
			{
				if (The.Player != null)
				{
					ParentObject.Brain.PushGoal(new Flee(The.Player, 1));
				}
			}
			else
			{
				ParentObject.Brain.Goals.Clear();
				ParentObject.Brain.PushGoal(new MindroneGoal(list2.GetRandomElement()));
				HealCooldown = 20;
			}
		}
		return base.HandleEvent(E);
	}
}

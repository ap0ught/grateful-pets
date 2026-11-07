using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class Graftek : IPart
{
	public int GraftCooldown;

	public override bool SameAs(IPart p)
	{
		if ((p as Graftek).GraftCooldown != GraftCooldown)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			GraftCooldown--;
			if (GraftCooldown <= 0)
			{
				for (int i = 0; i < ParentObject.Brain.Goals.Items.Count; i++)
				{
					if (ParentObject.Brain.Goals.Items[i].GetType().FullName.Contains("Graftek"))
					{
						return true;
					}
				}
				Cell cell = ParentObject.CurrentCell;
				List<GameObject> list = cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 12, "Combat", ParentObject);
				List<GameObject> list2 = Event.NewGameObjectList();
				foreach (GameObject item in list)
				{
					if (!item.IsPlayer() && !item.HasPart<Graftek>() && !item.HasPart<GraftekGraft>() && !item.HasPart<Metal>() && !item.Statistics.ContainsKey("Noclone") && (item.IsOrganic || item.HasPart<Brain>()))
					{
						list2.Add(item);
					}
				}
				if (list2.Count == 0)
				{
					ParentObject.Brain.PushGoal(new WanderRandomly(5));
				}
				else
				{
					ParentObject.Brain.Goals.Clear();
					ParentObject.Brain.PushGoal(new GraftekGoal(list2.GetRandomElement()));
				}
			}
		}
		return base.FireEvent(E);
	}
}

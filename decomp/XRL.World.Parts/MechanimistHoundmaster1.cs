using System;
using System.Collections.Generic;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class MechanimistHoundmaster1 : IPart
{
	public bool bCreated;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell")
		{
			try
			{
				if (bCreated)
				{
					return true;
				}
				bCreated = true;
				Physics physics = ParentObject.Physics;
				List<Cell> list = new List<Cell>();
				physics.CurrentCell.GetAdjacentCells(4, list);
				List<Cell> list2 = new List<Cell>();
				foreach (Cell item in list)
				{
					if (item.IsEmpty())
					{
						list2.Add(item);
					}
				}
				List<string> list3 = new List<string>();
				for (int i = 0; i < 9; i++)
				{
					list3.Add("Hyrkhound");
				}
				for (int j = 0; j < list3.Count; j++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObject.Create(list3[j]);
					gameObject.SetAlliedLeader<AllyHoundmaster>(ParentObject);
					Cell randomElement = list2.GetRandomElement();
					randomElement.AddObject(gameObject);
					gameObject.RequirePart<Frenzy>();
					gameObject.MakeActive();
					list2.Remove(randomElement);
				}
			}
			catch
			{
			}
		}
		return base.FireEvent(E);
	}
}

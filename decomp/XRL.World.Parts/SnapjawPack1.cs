using System;
using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class SnapjawPack1 : IPart
{
	public bool Created;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return !Created;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Created)
		{
			Created = true;
			try
			{
				List<Cell> list = Event.NewCellList();
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells(4))
				{
					if (adjacentCell.IsEmptyOfSolid())
					{
						list.Add(adjacentCell);
					}
				}
				int num = Stat.Random(1, 3);
				int num2 = Stat.Random(4, 8);
				List<string> list2 = new List<string>(num + num2);
				for (int i = 0; i < num; i++)
				{
					list2.Add("Snapjaw Hunter 1");
				}
				for (int j = 0; j < num2; j++)
				{
					list2.Add("Snapjaw Scavenger 1");
				}
				int k = 0;
				for (int count = list2.Count; k < count; k++)
				{
					Cell randomElement = list.GetRandomElement();
					if (randomElement != null)
					{
						GameObject gameObject = GameObject.Create(list2[k]);
						gameObject.PartyLeader = ParentObject;
						randomElement.AddObject(gameObject);
						gameObject.MakeActive();
						list.Remove(randomElement);
						continue;
					}
					break;
				}
			}
			catch (Exception x)
			{
				MetricsManager.LogError("SnapjawPack1 setup", x);
			}
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}

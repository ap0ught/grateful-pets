using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class BaboonHero1Pack : IPart
{
	public bool Created;

	[NonSerialized]
	public bool Hat;

	[NonSerialized]
	public int rings = 1;

	[NonSerialized]
	public float FollowerMultiplier = 1f;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
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
			int num = Stat.Random(4, 8);
			int num2 = Stat.Random(1, 3);
			List<string> list2 = new List<string>(num + num2);
			for (int i = 0; (float)i < (float)num * FollowerMultiplier; i++)
			{
				list2.Add("Baboon");
			}
			for (int j = 0; (float)j < (float)num2 * FollowerMultiplier; j++)
			{
				list2.Add("Hulking Baboon");
			}
			int k = 0;
			for (int count = list2.Count; k < count; k++)
			{
				Cell randomElement = list.GetRandomElement();
				if (randomElement != null)
				{
					GameObject gameObject = GameObject.Create(list2[k]);
					if (Hat)
					{
						gameObject.ReceiveObject("Leather Cap");
					}
					gameObject.SetAlliedLeader<AllyPack>(ParentObject);
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
			MetricsManager.LogError("BaboonHero setup", x);
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

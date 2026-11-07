using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class NaphtaaliPack : IPart
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
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		try
		{
			if (!Created)
			{
				Created = true;
				List<Cell> list = new List<Cell>();
				ParentObject.CurrentCell.GetAdjacentCells(4, list);
				List<Cell> list2 = new List<Cell>();
				foreach (Cell item in list)
				{
					if (item.IsEmpty())
					{
						list2.Add(item);
					}
				}
				List<string> list3 = new List<string>();
				int num = Stat.Random(1, 3);
				int num2 = Stat.Random(1, 3);
				int num3 = Stat.Random(1, 3);
				int num4 = Stat.Random(1, 3);
				int num5 = Stat.Random(4, 8);
				for (int i = 0; i < num; i++)
				{
					list3.Add("Naphtaali Forager");
				}
				for (int j = 0; j < num2; j++)
				{
					list3.Add("Naphtaali Runt");
				}
				for (int k = 0; k < num3; k++)
				{
					list3.Add("Naphtaali Blowgunner");
				}
				for (int l = 0; l < num4; l++)
				{
					list3.Add("Naphtaali Stalker");
				}
				for (int m = 0; m < num5; m++)
				{
					list3.Add("Naphtaali Forager");
				}
				for (int n = 0; n < list3.Count; n++)
				{
					if (list2.Count <= 0)
					{
						break;
					}
					GameObject gameObject = GameObject.Create(list3[n]);
					gameObject.SetAlliedLeader<AllyPack>(ParentObject);
					Cell randomElement = list2.GetRandomElement();
					randomElement.AddObject(gameObject);
					gameObject.MakeActive();
					list2.Remove(randomElement);
				}
			}
		}
		catch
		{
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

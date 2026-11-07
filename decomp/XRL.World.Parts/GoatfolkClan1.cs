using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class GoatfolkClan1 : IPart
{
	public bool Created;

	[NonSerialized]
	public bool Photosynthetic;

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
			int num = Stat.Random(2, 4);
			int num2 = Stat.Random(1, 2);
			int num3 = Stat.Random(1, 2);
			int num4 = (95.in100() ? 1 : 0);
			List<string> list2 = new List<string>(num + num2 + num3 + num4);
			for (int i = 0; i < num; i++)
			{
				list2.Add("Goatfolk Bully");
			}
			for (int j = 0; j < num2; j++)
			{
				list2.Add("Goatfolk Sower");
			}
			for (int k = 0; k < num3; k++)
			{
				list2.Add("Goatfolk Yurtwarden");
			}
			for (int l = 0; l < num4; l++)
			{
				list2.Add("Goatfolk Hornblower");
			}
			int m = 0;
			for (int count = list2.Count; m < count; m++)
			{
				Cell randomElement = list.GetRandomElement();
				if (randomElement == null)
				{
					break;
				}
				GameObject gameObject = GameObject.Create(list2[m]);
				if (Photosynthetic)
				{
					Mutations mutations = gameObject.RequirePart<Mutations>();
					if (!mutations.HasMutation("PhotosyntheticSkin"))
					{
						mutations.AddMutation(new PhotosyntheticSkin());
					}
				}
				gameObject.SetAlliedLeader<AllyClan>(ParentObject);
				randomElement.AddObject(gameObject);
				gameObject.MakeActive();
				list.Remove(randomElement);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogError("GoatfolkClan1 setup", x);
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

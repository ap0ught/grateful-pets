using System;

namespace XRL.World.Parts;

[Serializable]
public class DrowsingUrchin : IPart
{
	public int Puffed;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurn");
		Registrar.Register("BeforeDie");
		Registrar.Register("PuffPlease");
		base.Register(Object, Registrar);
	}

	public void Puff()
	{
		Puffed = 40;
		foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
		{
			adjacentCell.AddObject(GameObject.Create("SleepGas80"));
		}
		ParentObject.DustPuff();
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDie")
		{
			Puff();
		}
		else if (E.ID == "PuffPlease")
		{
			Puff();
		}
		else if (E.ID == "EndTurn")
		{
			if (Puffed > 0)
			{
				Puffed--;
			}
			else if (ParentObject.CurrentCell != null)
			{
				foreach (Cell adjacentCell in ParentObject.CurrentCell.GetAdjacentCells())
				{
					foreach (GameObject item in adjacentCell.GetObjectsWithPartReadonly("Brain"))
					{
						if (ParentObject.IsHostileTowards(item))
						{
							Puff();
							return true;
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

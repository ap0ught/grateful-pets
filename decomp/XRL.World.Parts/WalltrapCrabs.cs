using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapCrabs : IPart
{
	public int SpawnDistance = 3;

	public bool Seen;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public void SpawnCrab(string D)
	{
		Cell cellFromDirection = ParentObject.CurrentCell;
		if (cellFromDirection == null)
		{
			return;
		}
		for (int i = 0; i < SpawnDistance; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(D);
			if (cellFromDirection != null && !cellFromDirection.IsSolid(ForFluid: true))
			{
				if (cellFromDirection.IsEmpty())
				{
					GameObject gameObject = GameObject.Create("Chute Crab");
					cellFromDirection.AddObject(gameObject);
					gameObject.MakeActive();
					break;
				}
				continue;
			}
			break;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WalltrapTrigger")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && !IsBroken() && !IsRusted() && !IsEMPed())
			{
				if (!Seen && cell.IsVisible())
				{
					Seen = true;
				}
				if (Seen)
				{
					string[] cardinalDirectionList = Directions.CardinalDirectionList;
					foreach (string text in cardinalDirectionList)
					{
						Cell cellFromDirection = cell.GetCellFromDirection(text);
						if (cellFromDirection != null && !cellFromDirection.IsSolid(ForFluid: true))
						{
							SpawnCrab(text);
						}
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}

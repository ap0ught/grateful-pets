using System;
using System.Collections.Generic;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Leader : IPart
{
	public bool bCreated;

	public string popTable = "";

	public int SpawnRadius = 2;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		SetupLeader(builtKnown: true);
		return base.HandleEvent(E);
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
			SetupLeader();
		}
		return base.FireEvent(E);
	}

	private void SetupLeader(bool builtKnown = false)
	{
		try
		{
			if (bCreated || (!builtKnown && !ParentObject.CurrentCell.ParentZone.Built))
			{
				return;
			}
			bCreated = true;
			_ = ParentObject.Physics;
			List<string> list = new List<string>();
			foreach (PopulationResult item in PopulationManager.Generate(popTable))
			{
				for (int i = 0; i < item.Number; i++)
				{
					list.Add(item.Blueprint);
				}
			}
			List<Cell> list2 = new List<Cell>();
			ParentObject.CurrentCell.GetAdjacentCells(SpawnRadius, list2);
			List<Cell> list3 = new List<Cell>();
			foreach (Cell item2 in list2)
			{
				if (item2.IsEmpty())
				{
					list3.Add(item2);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				GameObject gameObject = GameObject.Create(list[j]);
				gameObject.SetAlliedLeader<AllyPack>(ParentObject);
				Cell cell = ((list3 != null) ? list3.GetRandomElement() : null);
				if (cell != null)
				{
					cell.AddObject(gameObject);
					gameObject.MakeActive();
					list3.Remove(cell);
				}
			}
		}
		catch
		{
		}
	}
}

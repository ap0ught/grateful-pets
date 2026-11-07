using System;
using System.Collections.Generic;
using XRL.Names;
using XRL.World.AI;
using XRL.World.Parts;

namespace XRL.World.ObjectBuilders;

public class GenericHero : IPart
{
	public int HitpointMultiplier = 2;

	public string ExtraEquipment = "Junk 2R,Junk 3";

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
				string text = MakeTitle();
				ParentObject.GiveProperName(MakeName());
				ParentObject.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
				if (!text.IsNullOrEmpty())
				{
					ParentObject.RequirePart<Titles>().AddTitle(text, -40);
				}
				AfterNameGeneration();
				CreateRetinue();
				GenerateEquipment();
				ModifyStats();
			}
			catch (Exception x)
			{
				MetricsManager.LogError("GenericHero setup", x);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public virtual void AfterNameGeneration()
	{
	}

	public virtual List<ObjectRetinueEntry> GenerateRetinue()
	{
		return new List<ObjectRetinueEntry>();
	}

	public virtual void EquipFollower(GameObject Follower)
	{
	}

	private void CreateRetinue()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return;
		}
		List<Cell> list = Event.NewCellList();
		foreach (Cell adjacentCell in cell.GetAdjacentCells(4))
		{
			if (adjacentCell.IsEmptyOfSolid())
			{
				list.Add(adjacentCell);
			}
		}
		foreach (ObjectRetinueEntry item in GenerateRetinue())
		{
			foreach (GameObject item2 in item.Generate())
			{
				EquipFollower(item2);
				item2.SetAlliedLeader<AllyRetinue>(ParentObject);
				Cell cell2 = list.GetRandomElement() ?? cell.ParentZone?.GetRandomCell() ?? cell;
				cell2.AddObject(item2);
				item2.MakeActive();
				list.Remove(cell2);
			}
		}
	}

	public virtual string MakeName()
	{
		return NameMaker.MakeName(ParentObject, null, null, null, null, null, null, null, null, null, "Hero", null, null, FailureOkay: false, SpecialFaildown: true);
	}

	public virtual string MakeTitle()
	{
		return NameMaker.MakeTitle(ParentObject, null, null, null, null, null, null, null, null, null, "Hero", null, SpecialFaildown: true);
	}

	public virtual void GenerateEquipment()
	{
	}

	public virtual void ModifyStats()
	{
		ParentObject.GetStat("Hitpoints").BaseValue *= HitpointMultiplier;
	}
}

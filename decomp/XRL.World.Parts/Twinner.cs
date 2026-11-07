using System;
using System.Collections.Generic;
using Genkit;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Twinner : IPart
{
	public GameObject Twin;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		return new Twinner
		{
			ParentObject = Parent
		};
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != PooledEvent<IsSensableAsPsychicEvent>.ID)
		{
			return ID == OnDestroyObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(IsSensableAsPsychicEvent E)
	{
		E.Sensable = true;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		EnteredCell();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OnDestroyObjectEvent E)
	{
		OnDestroyObject();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AITakingAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITakingAction" && GameObject.Validate(ParentObject))
		{
			Act();
		}
		return base.FireEvent(E);
	}

	public List<Cell> GetFollowCells()
	{
		List<Cell> list = new List<Cell>();
		for (int i = 0; i < 8; i++)
		{
			Cell cellFromDirectionGlobal = ParentObject.CurrentCell.GetCellFromDirectionGlobal(Cell.DirectionList[i]);
			if (cellFromDirectionGlobal != null && cellFromDirectionGlobal.IsPassable())
			{
				list.Add(cellFromDirectionGlobal);
			}
		}
		return list;
	}

	public virtual void Act()
	{
		if (Twin?.CurrentCell != null || !CheckMyRealityDistortionAdvisability())
		{
			return;
		}
		Cell randomElement = GetSpawnCells().GetRandomElement();
		if (randomElement != null)
		{
			Event obj = Event.New("InitiateRealityDistortionTransit");
			obj.SetParameter("Object", ParentObject);
			obj.SetParameter("Mutation", this);
			obj.SetParameter("Cell", randomElement);
			if (ParentObject.FireEvent(obj) && randomElement.FireEvent(obj))
			{
				Twin = ParentObject.DeepCopy();
				Twin.RemovePart<GivesRep>();
				Twin.GetPart<Twinner>().Twin = ParentObject;
				Spawn(Twin, randomElement);
			}
		}
	}

	public List<Cell> GetSpawnCells()
	{
		List<Cell> list = Event.NewCellList();
		foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells())
		{
			if (localAdjacentCell.IsEmptyForPopulation() && !IsRealityStabilized(localAdjacentCell))
			{
				list.Add(localAdjacentCell);
			}
		}
		return list;
	}

	public virtual bool IsRealityStabilized(Cell Cell)
	{
		GameObject parentObject = ParentObject;
		return !IComponent<GameObject>.CheckRealityDistortionAdvisability(null, Cell, parentObject, null, this);
	}

	public void Spawn(GameObject obj, Cell cell)
	{
		obj.StripContents(KeepNatural: true, Silent: true);
		obj.RestorePristineHealth(UseHeal: false, SkipEffects: true);
		obj.StopFighting(ParentObject);
		obj.GetStat("XPValue").BaseValue = 0;
		ParentObject.StopFighting(obj);
		cell.AddObject(obj);
		obj.MakeActive();
		WasReplicatedEvent.Send(ParentObject, ParentObject, obj, "Twin");
		ReplicaCreatedEvent.Send(obj, ParentObject, ParentObject, "Twin");
		if (IComponent<GameObject>.Visible(obj))
		{
			for (int i = 0; i < 10; i++)
			{
				XRLCore.ParticleManager.AddRadial("&Mù", cell.X, cell.Y, Stat.Random(0, 5), Stat.Random(5, 10), 0.01f * (float)Stat.Random(4, 6), -0.05f * (float)Stat.Random(3, 7));
			}
		}
	}

	public virtual void EnteredCell()
	{
		if (Twin?.CurrentCell != null && ParentObject.CurrentCell.ParentZone != Twin.CurrentCell.ParentZone)
		{
			List<Cell> followCells = GetFollowCells();
			Cell targetCell = ParentObject.CurrentCell;
			if (followCells.Count > 0)
			{
				targetCell = ((followCells.Count > 1) ? followCells[Calc.R.Next(followCells.Count)] : followCells[0]);
			}
			Twin.Brain.Goals.Clear();
			Twin.SystemLongDistanceMoveTo(targetCell);
			Twin.UseEnergy(1000, "Move Join Leader");
		}
	}

	public virtual void OnDestroyObject()
	{
		if (Twin != null && Twin.CurrentCell == null)
		{
			Twinner part = Twin.GetPart<Twinner>();
			if (part != null)
			{
				part.Twin = null;
			}
			Twin.Obliterate();
		}
	}
}

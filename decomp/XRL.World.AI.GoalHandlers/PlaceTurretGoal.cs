using System;
using Genkit;
using XRL.World.Capabilities;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class PlaceTurretGoal : GoalHandler
{
	public Location2D target;

	public string turretType;

	public PlaceTurretGoal(Location2D target, string turretType)
	{
		this.target = target;
		this.turretType = turretType;
	}

	public override void Create()
	{
		Think("I'm trying to place a turret!");
	}

	public override bool CanFight()
	{
		return false;
	}

	public override bool Finished()
	{
		return false;
	}

	public override void Push(Brain pBrain)
	{
		base.Push(pBrain);
	}

	public override void TakeAction()
	{
		if (target == null)
		{
			Think("I don't have a target!");
			FailToParent();
		}
		else if (string.IsNullOrEmpty(turretType))
		{
			Think("I don't have a turret type to place!");
			FailToParent();
		}
		else if (ParentObject.CurrentCell.Location.SquareDistance(target) == 1)
		{
			Think("I'm going to place a turret!");
			GameObject gameObject = IntegratedWeaponHosts.GenerateTurret(GameObject.Create(turretType), ParentObject);
			ParentObject.CurrentZone.GetCell(target).AddObject(gameObject);
			gameObject.MakeActive();
			gameObject.FireEventOnBodyparts(Event.New("GenerateIntegratedHostInitialAmmo", "Host", gameObject));
			CommandReloadEvent.Execute(gameObject, FreeAction: true);
			ParentObject.UseEnergy(1000, "Tinker Deploy Turret");
			FailToParent();
			ParentObject.Brain.DidXToY("place", gameObject, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			TurretTinker part = ParentObject.GetPart<TurretTinker>();
			if (part == null)
			{
				return;
			}
			part.MaxTurretsPlaced--;
			if (part.MaxTurretsPlaced <= 0)
			{
				return;
			}
			int num = 0;
			foreach (Cell localAdjacentCell in ParentObject.CurrentCell.GetLocalAdjacentCells(5))
			{
				if (localAdjacentCell.HasObjectWithTagOrProperty("TinkerTurret"))
				{
					num++;
				}
			}
			if (num < 3)
			{
				Cell randomElement = ParentObject.CurrentCell.GetEmptyAdjacentCells(1, 3).GetRandomElement();
				ParentObject.Brain.PushGoal(new PlaceTurretGoal(randomElement.Location, part.GetTurretWeaponBlueprintInstance()));
			}
		}
		else if (!MoveTowards(ParentObject.CurrentCell.ParentZone.GetCell(target)))
		{
			Think("I can't get to my target!");
			FailToParent();
		}
	}
}

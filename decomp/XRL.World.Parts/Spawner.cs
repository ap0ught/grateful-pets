using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Spawner : IPart
{
	public string TrailBlueprint;

	public string SpawnBlueprint;

	public string SpawnCheckBlueprint;

	public string SpawnMessage;

	public string SpawnVerb;

	public string SpawnCooldown;

	public int SpawnChance = 100;

	public int SpawnCheckLimit;

	public bool SpawnOnEnteredCell;

	public bool SpawnOnTurnTick;

	public bool SpawnAdjacent;

	public bool PassAttitudes = true;

	public bool VillagePassAttitudes = true;

	public bool CombatOnly;

	public int CurrentCooldown;

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
		if (!E.Forced && !E.System && !E.Direction.IsNullOrEmpty() && !E.Cell.OnWorldMap())
		{
			if (!string.IsNullOrEmpty(TrailBlueprint))
			{
				E.Cell.AddObject(TrailBlueprint);
			}
			Spawn(E.Cell);
		}
		return base.HandleEvent(E);
	}

	public void Spawn(Cell Cell)
	{
		if (CurrentCooldown > 0 || (CombatOnly && !ParentObject.IsPlayer() && ParentObject.Target == null) || !SpawnChance.in100() || (SpawnCheckLimit > 0 && !SpawnCheckBlueprint.IsNullOrEmpty() && Cell.ParentZone.CountObjects(SpawnCheckBlueprint) >= SpawnCheckLimit))
		{
			return;
		}
		if (!SpawnCooldown.IsNullOrEmpty())
		{
			CurrentCooldown = SpawnCooldown.RollCached();
		}
		GameObject gameObject = GameObject.Create(SpawnBlueprint);
		if (SpawnAdjacent)
		{
			Cell = Cell.GetNavigableAdjacentCells(gameObject).GetRandomElement();
		}
		SpawnVessel part = gameObject.GetPart<SpawnVessel>();
		if (part != null)
		{
			part.SpawnedBy = ParentObject;
			if (PassAttitudes)
			{
				gameObject.TakeAllegiance<AllyBirth>(ParentObject);
				part.AdjustAttitude = true;
			}
		}
		Cell.AddObject(gameObject);
		if (!SpawnMessage.IsNullOrEmpty())
		{
			EmitMessage(GameText.VariableReplace(SpawnMessage, ParentObject, gameObject));
		}
		if (!SpawnVerb.IsNullOrEmpty())
		{
			DidXToY(SpawnVerb, gameObject, null, null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (CurrentCooldown > 0)
		{
			CurrentCooldown -= Amount;
		}
		if (SpawnOnTurnTick)
		{
			Spawn(ParentObject.CurrentCell);
		}
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "VillageInit")
		{
			CombatOnly = true;
			if (VillagePassAttitudes)
			{
				PassAttitudes = true;
			}
		}
		return base.FireEvent(E);
	}
}

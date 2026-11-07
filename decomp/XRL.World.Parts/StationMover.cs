using System;
using XRL.Rules;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class StationMover : SoundRendererPart
{
	[NonSerialized]
	public bool Moving;

	[NonSerialized]
	public byte Animation;

	[NonSerialized]
	public string Direction;

	[NonSerialized]
	public Cell Destination;

	[NonSerialized]
	private string SoundID;

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != EnteringCellEvent.ID && ID != EnteredCellEvent.ID && ID != EnteringZoneEvent.ID && ID != AfterMoveFailedEvent.ID && ID != PooledEvent<AfterPilotChangeEvent>.ID && ID != ActorGetNavigationWeightEvent.ID)
		{
			return ID == PooledEvent<GetTransitiveLocationEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteringZoneEvent E)
	{
		bool flag = E.Origin.OnWorldMap();
		bool flag2 = E.Cell.OnWorldMap();
		if (ParentObject.IsVisible() && Animation == 0)
		{
			if (!flag & flag2)
			{
				Direction = StationMoverChute.GetAdjacentDirection(E.Origin);
				if (!Direction.IsNullOrEmpty())
				{
					Animation = 1;
					Destination = E.Cell;
					return false;
				}
			}
			else if (flag && !flag2)
			{
				Direction = StationMoverChute.GetAdjacentDirection(E.Cell);
				if (!Direction.IsNullOrEmpty())
				{
					Animation = 2;
					Destination = E.Cell;
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterMoveFailedEvent E)
	{
		if (Animation != 0 && Destination != null)
		{
			if (Animation == 1)
			{
				Cell end = ((Direction == "S") ? E.Origin.ParentZone.GetCell(E.Origin.X, 24) : E.Origin.ParentZone.GetCell(E.Origin.X, 0));
				Animate(E.Origin, end);
				if (ParentObject.IsPlayer())
				{
					IComponent<GameObject>.PlayUISound("sfx_worldMap_enter");
				}
			}
			else
			{
				Cell start = ((Direction == "S") ? Destination.ParentZone.GetCell(Destination.X, 24) : Destination.ParentZone.GetCell(Destination.X, 0));
				Animate(start, Destination);
			}
			ParentObject.DirectMoveTo(Destination, 0, Forced: true, IgnoreCombat: true, IgnoreGravity: true);
			if (ParentObject.IsPlayer() && Animation == 2 && ParentObject.TryGetPart<Vehicle>(out var Part))
			{
				Part.Pilot = null;
				CheckpointingSystem.ManualCheckpoint();
			}
			Destination = null;
			Animation = 0;
		}
		return base.HandleEvent(E);
	}

	public void Animate(Cell Start, Cell End)
	{
		Zone parentZone = Start.ParentZone;
		Cell cell = Start;
		string directionFromCell = Start.GetDirectionFromCell(End);
		int num = Start.PathDistanceTo(End);
		bool flag = End.Y != 0 && End.Y != 24;
		int x = Start.X;
		int num2 = Start.Y;
		Render render = ParentObject.Render;
		try
		{
			if (flag)
			{
				PlayWorldSound("sfx_endgame_worldMap_spacestation_mover_arrive");
			}
			else
			{
				PlayWorldSound("sfx_endgame_worldMap_spacestation_mover_launch");
				Acrobatics_Jump.Leap(Start, End, 6);
			}
			for (int i = 0; i <= num; i++)
			{
				ParentObject.DirectMoveTo(cell, 0, Forced: true, IgnoreCombat: true, IgnoreGravity: true);
				BlipChute(parentZone.GetCell(x - 1, num2), 20);
				BlipChute(parentZone.GetCell(x + 1, num2), 20);
				if (!flag || i != num)
				{
					if (!The.Core.RenderDelay(flag ? Math.Min(20 + i * 5, 50) : Math.Max(150 - i * 20, 20), Interruptible: false))
					{
						return;
					}
					cell.TileParticleBlip(render.Tile, "&K", render.DetailColor, 20, IgnoreVisibility: false, render.HFlip, render.VFlip, 0L);
					if (directionFromCell == "N")
					{
						num2--;
					}
					else if (directionFromCell == "S")
					{
						num2++;
					}
					cell = parentZone.GetCell(x, num2);
				}
			}
			if (flag)
			{
				Acrobatics_Jump.Land(Start, End, 8);
			}
			else
			{
				ParentObject.Render.Visible = false;
			}
			if (ParentObject.IsPlayer())
			{
				The.Core.RenderDelay(750, Interruptible: false, SkipInput: true, AllowHiddenPlayer: true);
			}
		}
		finally
		{
			ParentObject.Render.Visible = true;
		}
	}

	public void BlipChute(Cell Cell, int Duration)
	{
		GameObject gameObject = Cell?.GetObjectWithTag("MoverChute");
		if (gameObject != null)
		{
			Render render = gameObject.Render;
			string colorString = (render.TileColor.IsNullOrEmpty() ? render.ColorString : render.TileColor);
			gameObject.TileParticleBlip(render.Tile, colorString, "R", Duration, IgnoreVisibility: false, render.HFlip, render.VFlip, 0L);
		}
	}

	public override bool HandleEvent(EnteringCellEvent E)
	{
		if (!E.Cell.HasObject(HasTransitTag) && E.Direction != null && Array.IndexOf(Directions.DirectionList, E.Direction) != -1)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!Moving && E.Direction != null && ParentObject.IsPlayer() && Array.IndexOf(Directions.DirectionList, E.Direction) != -1)
		{
			PlayWorldSound("sfx_endgame_worldMap_spacestation_mover_launch", 0.25f);
			string text = GetSingularDirection(E.Cell, Directions.GetOppositeDirection(E.Direction));
			if (text == null)
			{
				return base.HandleEvent(E);
			}
			Moving = true;
			try
			{
				bool flag = false;
				while (!ParentObject.CurrentCell.HasObject(IsMoverStop) && The.Core.RenderDelay(150))
				{
					if (!ParentObject.Move(text, Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: true, DoConfirmations: false))
					{
						text = null;
					}
					else
					{
						flag = true;
						text = GetSingularDirection(ParentObject.CurrentCell, Directions.GetOppositeDirection(text));
					}
					if (text == null)
					{
						break;
					}
				}
				if (flag)
				{
					PlayWorldSound("sfx_endgame_worldMap_spacestation_mover_arrive", 0.25f);
				}
			}
			finally
			{
				Moving = false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterPilotChangeEvent E)
	{
		if (E.NewPilot != null && E.Vehicle == ParentObject && E.Vehicle.IsPlayer() && !E.Vehicle.OnWorldMap())
		{
			Cell cell = E.Vehicle.CurrentZone?.GetWorldCell();
			if (cell != null)
			{
				E.Vehicle.DirectMoveTo(cell, 0, Forced: true, IgnoreCombat: true, IgnoreGravity: true);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ActorGetNavigationWeightEvent E)
	{
		if (E.Weight < 100 && !E.Cell.HasObject(HasTransitTag))
		{
			E.Weight = 100;
			E.Uncacheable = true;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetTransitiveLocationEvent E)
	{
		if (E.IsEgress && E.Zone == ParentObject.CurrentZone && E.Origin?.ParentZone is InteriorZone interiorZone && interiorZone.ParentObject == ParentObject)
		{
			Cell cell = ParentObject.CurrentCell;
			string adjacentDirection = StationMoverChute.GetAdjacentDirection(cell);
			if (!adjacentDirection.IsNullOrEmpty())
			{
				string oppositeDirection = Directions.GetOppositeDirection(adjacentDirection);
				Cell cellFromDirection = cell.GetCellFromDirection(oppositeDirection);
				if (cellFromDirection.IsPassable(E.Actor))
				{
					E.AddLocation(cellFromDirection, ParentObject, 750);
				}
				string[] adjacentDirections = Directions.GetAdjacentDirections(oppositeDirection);
				foreach (string direction in adjacentDirections)
				{
					cellFromDirection = cell.GetCellFromDirection(direction);
					if (cellFromDirection.IsPassable(E.Actor))
					{
						E.AddLocation(cellFromDirection, ParentObject, 700);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public static bool IsMoverStop(GameObject Object)
	{
		return Object.GetBlueprint().DescendsFrom("TerrainMoverStop");
	}

	public static bool HasTransitTag(GameObject Object)
	{
		return Object.HasTagOrIntProperty("MoverTransit");
	}

	public string GetSingularDirection(Cell Origin, string ExcludeDirection)
	{
		string text = null;
		string[] cardinalDirectionList = Directions.CardinalDirectionList;
		foreach (string text2 in cardinalDirectionList)
		{
			if (text2 == ExcludeDirection)
			{
				continue;
			}
			Cell cellFromDirection = Origin.GetCellFromDirection(text2);
			if (cellFromDirection != null && cellFromDirection.HasObject(HasTransitTag))
			{
				if (text != null)
				{
					return null;
				}
				text = text2;
			}
		}
		return text;
	}

	public override void Render(RenderSoundEvent E)
	{
		if (Moving)
		{
			if (SoundID == null)
			{
				SoundID = "StationMover" + "." + Guid.NewGuid().ToString();
			}
			E.Char.soundExtra.Add(SoundID, "sfx_endgame_worldMap_spacestation_mover_move_lp", 0.25f, 0f);
		}
	}
}

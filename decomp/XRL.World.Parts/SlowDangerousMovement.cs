using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class SlowDangerousMovement : IPart
{
	public string PreparedDirection;

	public bool LinkedToConsumer;

	public string PrepMessageSelf;

	public string PrepMessageOther;

	public bool Active = true;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EffectAppliedEvent.ID && ID != EnteredCellEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			if (ID == GetShortDescriptionEvent.ID)
			{
				return !PreparedDirection.IsNullOrEmpty();
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EffectAppliedEvent E)
	{
		if (!PreparedDirection.IsNullOrEmpty() && Active && (!ParentObject.IsMobile() || !ParentObject.CanChangeMovementMode()))
		{
			PreparedDirection = null;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		PreparedDirection = null;
		foreach (Cell localAdjacentCell in E.Cell.GetLocalAdjacentCells())
		{
			localAdjacentCell.FlushNavigationCache();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (!PreparedDirection.IsNullOrEmpty() && Active)
		{
			E.Infix.Compound(ParentObject.Itis, "\n").Append(" preparing to move ").Append(Directions.GetIndicativeDirection(PreparedDirection));
			Consumer consumer = (LinkedToConsumer ? ParentObject.GetPart<Consumer>() : null);
			if (consumer != null && consumer.Chance > 0)
			{
				E.Infix.Append(", ");
				if (consumer.Chance < 100)
				{
					E.Infix.Append("potentially ");
				}
				E.Infix.Append("consuming anything in ").Append(ParentObject.its).Append(" path");
			}
			E.Infix.Append('.');
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("VillageInit");
		Registrar.Register("CurioInit");
		Registrar.Register("BeginMove");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginMove")
		{
			if (Active && E.GetStringParameter("Type").IsNullOrEmpty())
			{
				return TryToMoveTo(E.GetParameter("DestinationCell") as Cell);
			}
		}
		else if (E.ID == "VillageInit" || E.ID == "CurioInit")
		{
			Active = false;
		}
		return base.FireEvent(E);
	}

	public bool TryToMoveTo(Cell C)
	{
		string directionFromCell = ParentObject.CurrentCell.GetDirectionFromCell(C);
		if (!Directions.IsActualDirection(directionFromCell))
		{
			return true;
		}
		if (directionFromCell == PreparedDirection)
		{
			return true;
		}
		if (ParentObject.OnWorldMap())
		{
			return true;
		}
		Consumer consumer = (LinkedToConsumer ? ParentObject.GetPart<Consumer>() : null);
		if (consumer != null && ParentObject.Target != null && !consumer.AnythingToConsume(C))
		{
			return true;
		}
		PreparedDirection = directionFromCell;
		string text = (ParentObject.IsPlayer() ? PrepMessageSelf : PrepMessageOther);
		if (!text.IsNullOrEmpty() && Visible())
		{
			if (text.Contains("=dir="))
			{
				text = text.Replace("=dir=", Directions.GetExpandedDirection(directionFromCell));
			}
			if (text.Contains("=dirward="))
			{
				text = text.Replace("=dirward=", Directions.GetIndicativeDirection(directionFromCell));
			}
			IComponent<GameObject>.AddPlayerMessage(GameText.VariableReplace(text, ParentObject));
		}
		ParentObject.UseEnergy(1000, "Movement");
		CauseFleeing(C, Immediate: true);
		foreach (Cell directionAndAdjacentCell in C.GetDirectionAndAdjacentCells(directionFromCell, LocalOnly: false))
		{
			CauseFleeing(directionAndAdjacentCell);
		}
		if (!ParentObject.IsPlayer())
		{
			ParentObject.SetIntProperty("AIKeepMoving", 1);
		}
		return false;
	}

	public void CauseFleeing(Cell C, bool Immediate = false)
	{
		foreach (GameObject @object in C.Objects)
		{
			CauseFleeing(@object, C, Immediate);
		}
	}

	public void CauseFleeing(GameObject obj, Cell C = null, bool Immediate = false)
	{
		if (obj?.Brain == null || !obj.IsCreature || !obj.IsMobile() || obj.Stat("Intelligence") <= 6)
		{
			return;
		}
		if (obj.IsPlayer())
		{
			if (AutoAct.IsActive())
			{
				AutoAct.Interrupt("you are " + (Immediate ? "in" : "near") + " the path of " + ParentObject.an(), null, ParentObject, IsThreat: true);
			}
			return;
		}
		Consumer consumer = (LinkedToConsumer ? ParentObject.GetPart<Consumer>() : null);
		if ((consumer == null || consumer.WouldConsume(obj)) && (Immediate || !obj.IsEngagedInMelee()))
		{
			obj.Brain.PushGoal(new FleeLocation(C ?? obj.CurrentCell, 1));
		}
	}

	public override bool FinalRender(RenderEvent E)
	{
		if (!PreparedDirection.IsNullOrEmpty() && Active)
		{
			E.WantsToPaint = true;
		}
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (!PreparedDirection.IsNullOrEmpty() && Visible() && Active)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				Cell cellFromDirection = cell.GetCellFromDirection(PreparedDirection);
				if (cellFromDirection != null && cellFromDirection.IsVisible())
				{
					buffer.Goto(cellFromDirection.X, cellFromDirection.Y);
					if (XRLCore.CurrentFrame >= 20)
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileForeground = The.Color.DarkRed;
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].Detail = The.Color.DarkRed;
					}
					else
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileForeground = The.Color.Red;
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].Detail = The.Color.Red;
					}
					if (XRLCore.CurrentFrame % 20 >= 10)
					{
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].TileBackground = The.Color.Red;
						buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].SetBackground('R');
					}
					buffer.Buffer[cellFromDirection.X, cellFromDirection.Y].SetForeground('r');
				}
			}
		}
		base.OnPaint(buffer);
	}
}

using System;
using ConsoleLib.Console;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class StairsUp : IPart
{
	public bool Connected = true;

	public string ConnectionObject = "StairsDown";

	public string Sound = "Sounds/Interact/sfx_interact_stairs_ascend";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != PooledEvent<CheckAttackableEvent>.ID && ID != CommandSmartUseEvent.ID && ID != EnteredCellEvent.ID && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<IdleQueryEvent>.ID && ID != InventoryActionEvent.ID)
		{
			return ID == PooledEvent<SubjectToGravityEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(SubjectToGravityEvent E)
	{
		E.SubjectToGravity = false;
		return false;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (CommandBindingManager.GetCommandFromKey(Keys.Oemcomma | Keys.Shift) == "CmdMoveU")
		{
			E.AddAction("Ascend", "ascend", "Ascend", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Ascend" && E.Actor.IsPlayer() && CommandBindingManager.GetCommandFromKey(Keys.Oemcomma | Keys.Shift) == "CmdMoveU")
		{
			Popup.ShowFail("Use " + ControlManager.getCommandInputFormatted("CmdMoveU") + " to ascend.");
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckAttackableEvent E)
	{
		return false;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.CurrentCell == ParentObject.CurrentCell)
		{
			if (E.Actor.IsPlayer())
			{
				Keyboard.PushMouseEvent("Command:CmdMoveU");
			}
			else
			{
				E.Actor.Move("U", Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false);
			}
		}
		return false;
	}

	public override bool HandleEvent(IdleQueryEvent E)
	{
		if (ParentObject.HasTagOrProperty("IdleStairs") && E.Actor.HasPart<Brain>() && Stat.Random(1, 2000) == 2000)
		{
			GameObject who = E.Actor;
			who.Brain.PushGoal(new DelegateGoal((GoalHandler h) =>
			{
				if (who.GetCurrentCell() == ParentObject.GetCurrentCell())
				{
					who.Move("U", Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false);
				}
				h.FailToParent();
			}));
			who.Brain.PushGoal(new MoveTo(ParentObject));
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		bool flag = false;
		foreach (GameObject @object in E.Cell.Objects)
		{
			if (@object != ParentObject && @object.HasPart<StairsUp>())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			E.Cell.RemoveObject(ParentObject);
			ParentObject.Obliterate();
			return false;
		}
		if (Connected)
		{
			E.Cell.ParentZone.AddZoneConnection("u", E.Cell.X, E.Cell.Y, "StairsDown", ConnectionObject);
		}
		else
		{
			E.Cell.ParentZone.AddZoneConnection("u", E.Cell.X, E.Cell.Y, "UpEnd", null);
		}
		E?.Cell?.ClearWalls();
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ClimbUp");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ClimbUp")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("GO");
			if (gameObjectParameter != null && gameObjectParameter.IsPlayer() && ParentObject.HasTag("KeyObject") && !gameObjectParameter.IsCarryingObject(ParentObject.GetTag("KeyObject")))
			{
				Popup.Show("You don't have the proper key standin message");
				return false;
			}
			if (gameObjectParameter.Move("U", Forced: false, System: false, IgnoreGravity: false, NoStack: false, AllowDashing: false))
			{
				PlayWorldSound(Sound);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}

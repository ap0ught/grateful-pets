using System.Collections.Generic;
using XRL.Rules;

namespace XRL.World.AI.GoalHandlers;

public class AICommandList
{
	public const string META_COMMAND_MOVE_AWAY = "MetaCommandMoveAway";

	public int Priority;

	public string Command;

	public GameObject Object;

	public bool Inv;

	public bool Self;

	public GameObject TargetOverride;

	public Cell TargetCellOverride;

	public string DebugName
	{
		get
		{
			string text = Command;
			if (Self)
			{
				text += "/self";
			}
			if (TargetOverride != null)
			{
				text = text + "/target:" + TargetOverride.DebugName;
			}
			if (TargetCellOverride != null)
			{
				text = text + "/targetcell:" + TargetCellOverride.DebugName;
			}
			if (Priority != 0)
			{
				text = text + "(" + Priority + ")";
			}
			if (Inv)
			{
				text = "inv:" + text;
			}
			return text;
		}
	}

	public AICommandList(string Command, int Priority)
	{
		this.Priority = Priority;
		this.Command = Command;
	}

	public AICommandList(string Command, int Priority, GameObject Object = null, bool Inv = false, bool Self = false, GameObject TargetOverride = null, Cell TargetCellOverride = null)
		: this(Command, Priority)
	{
		this.Object = Object;
		this.Inv = Inv;
		this.Self = Self;
		this.TargetOverride = TargetOverride;
		this.TargetCellOverride = TargetCellOverride;
	}

	private bool ProcessCommand(GameObject Handler, GameObject Owner, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0)
	{
		return CommandEvent.Send(Owner, Command, Target, TargetCell, StandoffDistance, Forced: false, Silent: false, Handler);
	}

	public bool HandleCommand(GameObject Owner, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0)
	{
		if (Command == "MetaCommandMoveAway")
		{
			string text = TargetCell?.GetDirectionFromCell(Owner.CurrentCell) ?? Target?.CurrentCell?.GetDirectionFromCell(Owner.CurrentCell);
			if (!text.IsNullOrEmpty())
			{
				if (text == "." || text == "?")
				{
					text = Owner.CurrentCell.GetDirectionFromCell(Owner.CurrentCell.GetRandomLocalAdjacentCell((Cell c) => c.IsEmptyOfSolidFor(Owner)), NullIfSame: true);
				}
				if (!text.IsNullOrEmpty() && Owner.Move(text))
				{
					return true;
				}
			}
		}
		else
		{
			GameObject gameObject = Object ?? Owner;
			if (Inv)
			{
				GameObject actor = Owner;
				string command = Command;
				GameObject objectTarget = TargetOverride ?? (Self ? Owner : Target);
				Cell cellTarget = TargetCellOverride ?? TargetCell;
				if (InventoryActionEvent.Check(gameObject, actor, gameObject, command, Auto: false, OwnershipHandled: false, OverrideEnergyCost: false, Forced: false, Silent: false, 0, 0, StandoffDistance, objectTarget, cellTarget))
				{
					return true;
				}
			}
			else if (ProcessCommand(gameObject, Owner, TargetOverride ?? (Self ? Owner : Target), TargetCellOverride ?? TargetCell, StandoffDistance))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HandleCommandList(List<AICommandList> List, GameObject Owner, GameObject Target = null, Cell TargetCell = null, int StandoffDistance = 0)
	{
		if (List != null && List.Count > 0)
		{
			int num = Stat.Random(0, List.Count - 1);
			if (List[num].HandleCommand(Owner, Target, TargetCell, StandoffDistance))
			{
				return true;
			}
			if (List.Count > 1)
			{
				int num2 = Stat.Random(0, List.Count - 1);
				if (num2 != num && List[num2].HandleCommand(Owner, Target, TargetCell, StandoffDistance))
				{
					return true;
				}
				if (List.Count > 2)
				{
					int num3 = Stat.Random(0, List.Count - 1);
					if (num3 != num2 && num3 != num && List[num3].HandleCommand(Owner, Target, TargetCell, StandoffDistance))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}

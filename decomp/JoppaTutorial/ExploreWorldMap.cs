using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;

namespace JoppaTutorial;

public class ExploreWorldMap : TutorialStep
{
	public GameObject joppa;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell.ParentZone.ZoneID == "JoppaWorld")
		{
			if (cell.X == 11 && cell.Y >= 22 && cell.Y <= 24)
			{
				return true;
			}
		}
		else if (cell.ParentZone.ZoneID == "JoppaWorld.11.22.1.1.10")
		{
			return true;
		}
		Popup.Show("You'll be able to explore the world freely after the tutorial.\n\nFor now, let's visit Joppa.");
		return false;
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		return true;
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (joppa == null)
		{
			joppa = The.Player.CurrentZone?.FindObject("TerrainJoppa");
		}
		if (CurrentGameView == "Looker" && step == 0)
		{
			step = 100;
		}
		if (!(CurrentGameView == "Stage"))
		{
			return;
		}
		if (The.Player.CurrentZone.Z == 10)
		{
			TutorialManager.AdvanceStep(new ExploreJoppa());
			return;
		}
		if (step == 0)
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				manager.HighlightCell(The.Player.CurrentCell.X, The.Player.CurrentCell.Y, "You can look around here just like on a local map.\n\nUse ~LookDirection", "sw");
			}
			else
			{
				manager.HighlightCell(The.Player.CurrentCell.X, The.Player.CurrentCell.Y, "You can look around here just like on a local map.\n\nPress ~CmdLook", "sw");
			}
		}
		if (step != 100)
		{
			return;
		}
		if (The.Player.CurrentCell == joppa.CurrentCell)
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				manager.HighlightByCID("Stage:TopButtonBar:DownButton", "Descend.\n\nPress ~CmdMoveD.", "sw");
			}
			else
			{
				manager.HighlightByCID("Stage:TopButtonBar:DownButton", "Descend.\n\nYou can click this button or press ~CmdMoveD.", "sw");
			}
		}
		else if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			manager.HighlightCell(joppa.CurrentCell.X, joppa.CurrentCell.Y, "There’s a village to the north called Joppa. Let’s go there.\n\nHold ~IndicateDirection towards the north and press ~Take A Step.", "ne");
		}
		else
		{
			manager.HighlightCell(joppa.CurrentCell.X, joppa.CurrentCell.Y, "There’s a village to the north called Joppa. Let’s go there.\n\nPress ~CmdMoveN or ~AdventureMouseContextAction.", "ne");
		}
	}
}

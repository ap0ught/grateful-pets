using System.Linq;
using Qud.UI;
using XRL;
using XRL.World;

namespace JoppaTutorial;

public class ExploreStairs : TutorialStep
{
	public GameObject stairsUp;

	private Cell stairsCell;

	private bool saw;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell != null && cell.ParentZone.Z == 10)
		{
			return true;
		}
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		return true;
	}

	public override void LateUpdate()
	{
		if (stairsUp == null || stairsCell == null)
		{
			stairsUp = The.Player.CurrentZone.FindObjectsWithPart("StairsUp").FirstOrDefault();
			stairsCell = stairsUp?.CurrentCell;
		}
		manager.ClearHighlight();
		if (!(CurrentGameView == "Stage"))
		{
			return;
		}
		if (The.Player.CurrentZone.Z == 10)
		{
			TutorialManager.AdvanceStep(new MakeCamp());
		}
		else
		{
			if (stairsUp == null)
			{
				return;
			}
			if (The.PlayerCell.DistanceTo(stairsUp) <= 1)
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightByCID("Stage:TopButtonBar:UpButton", "Ascend.\n\nPress ~CmdMoveU.", "sw");
				}
				else
				{
					manager.HighlightByCID("Stage:TopButtonBar:UpButton", "Ascend.\n\nYou can click this button or press ~CmdMoveU.", "sw");
				}
				return;
			}
			if (!saw)
			{
				GameObject gameObject = stairsUp;
				if (gameObject == null || !gameObject.IsVisible())
				{
					manager.HighlightCell(48, 17, "Keep exploring.", "nw", 0f, 0f);
					return;
				}
			}
			saw = true;
			if (stairsCell != null)
			{
				manager.HighlightCell(stairsCell.X, stairsCell.Y, "Finally, a staircase. Walk over to it.", "nw", 0f, 0f);
			}
		}
	}
}

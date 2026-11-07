using Qud.UI;
using XRL;
using XRL.World;

namespace JoppaTutorial;

public class ExploreDeeper : TutorialStep
{
	public GameObject chemCell;

	public bool infoShown;

	public bool snapjawDead;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell?.ParentZone?.ZoneID == "JoppaWorld.11.24.1.1.11" || cell?.ParentZone?.ZoneID == "JoppaWorld.11.24.1.0.11")
		{
			return true;
		}
		return base.BeforePlayerEnterCell(cell);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		return true;
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (CurrentGameView == "Stage" && The.Player?.CurrentCell != null)
		{
			if (The.Player.CurrentCell.Y == 0)
			{
				manager.HighlightCell(41, -1, "The whole game world is a series of contiguous maps. Our passage continues off screen. Let's follow it. Walk north.", "nw", 0f, 0f);
			}
			else if (The.Player.CurrentCell.Y > 8)
			{
				manager.HighlightCell(41, 8, "Let's move on. Keep exploring.", "nw", 0f, 0f);
			}
			else
			{
				manager.HighlightCell(41, -1, "Let's move on. Keep exploring.", "nw", 0f, 0f);
			}
			if (The.Player.CurrentZone.Y == 0)
			{
				TutorialManager.AdvanceStep(new BattleRemains());
			}
		}
	}
}

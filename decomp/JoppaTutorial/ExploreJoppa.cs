using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;

namespace JoppaTutorial;

public class ExploreJoppa : TutorialStep
{
	public GameObject mehmet;

	public GameObject yrame;

	public bool uhoh;

	private bool fancyDone;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell?.ParentZone.ZoneID == "JoppaWorld.11.22.1.1.10")
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
		manager.ClearHighlight();
		if (mehmet == null)
		{
			mehmet = The.Player.CurrentZone?.FindObject("Mehmet");
			yrame = The.Player.CurrentZone?.FindObject("Warden Yrame");
		}
		if (mehmet != null && yrame != null && yrame.Brain?.Target == mehmet)
		{
			uhoh = true;
		}
		if (CurrentGameView == "PopupMessage")
		{
			string lastPopupID = PopupMessage.lastPopupID;
			if (lastPopupID != null && lastPopupID.StartsWith("CmdMoveToPointOfInterestMenu"))
			{
				step = 200;
			}
		}
		if (step == 200 && CurrentGameView == "PopupMessage")
		{
			string lastPopupID2 = PopupMessage.lastPopupID;
			if (lastPopupID2 != null && lastPopupID2.StartsWith("CmdMoveToPointOfInterestMenu"))
			{
				manager.HighlightByCID("QudTextMenuItem:mehmet [ne]", "Choose Mehmet.", "ne", 64, 8);
			}
		}
		if (uhoh && !mehmet.IsValid())
		{
			TutorialManager.ShowCellPopup(yrame.CurrentCell.Location, "Oh… it looks like Warden Yrame killed Mehmet. Why, you ask?\n\nWell, named creatures who you can perform the water ritual with, like Mehmet, generate with dynamic faction relationships. In your game, Mehmet must have been hated by the Fellowship of Wardens for some reason.", "nw", 6, 6, () =>
			{
				TutorialManager.ShowCellPopup(yrame.CurrentCell.Location, "And so Warden Yrame took her revenge. In Qud, sometimes even our best efforts at providing a safe starter village are foiled by the simulation…\n\nWhat a charmed run! It's quite rare for this to happen, but now you get to see this hidden section of the tutorial. Good job.", "nw", 6, 6, () =>
				{
					TutorialManager.ShowCellPopup(The.Player.CurrentCell.Location, "You have a few options now. You can roll with it and continue in this world where Mehmet's been revenge-slain. The early game will be a bit harder, but the main quest is not impacted.\n\nYou can start over and play the tutorial again, for the \"normal\" ending, which you were about to reach.\n\nOr, since you were near the end anyway, you can start a new game and begin your journey proper!", "nw", 6, 6, () =>
					{
						TutorialManager.ShowCellPopup(The.Player.CurrentCell.Location, "Whatever you choose, good luck, and feel free to return here for a refresher.\n\nLive and drink.", "nw", 6, 6, () =>
						{
							Options.SetOption("OptionTutorialComplete", "Yes");
							TutorialManager.AdvanceStep(null);
						});
					});
				});
			});
		}
		if (The.Player.DistanceTo(mehmet) > 1)
		{
			GameObject gameObject = mehmet;
			if ((gameObject == null || gameObject.IsValid()) && !fancyDone)
			{
				goto IL_01c2;
			}
		}
		TutorialManager.ShowCellPopup(mehmet.CurrentCell.Location, "Quetzal! That's all for the tutorial, friend.\n\nWe just scratched the very tip of the surface. Quests, secrets, leveling up, physics, reputation, limbs... we leave that for you to discover.", "nw", 6, 6, () =>
		{
			TutorialManager.ShowCellPopup(mehmet.CurrentCell.Location, "You're ready to continue the journey on your own, but don't hesitate to play the tutorial again if you need a refresher.", "nw", 6, 6, () =>
			{
				TutorialManager.ShowCellPopup(mehmet.CurrentCell.Location, "Live and drink.", "nw", 6, 6, () =>
				{
					Options.SetOption("OptionTutorialComplete", "Yes");
					TutorialManager.AdvanceStep(null);
				});
			});
		});
		goto IL_01c2;
		IL_01c2:
		if (!(CurrentGameView == "Stage"))
		{
			return;
		}
		if (step == 0)
		{
			step = 100;
			TutorialManager.ShowCellPopup(The.Player.CurrentCell.Location, "Joppa is a village. Everything you learned in the cave applies here too. Villages are like caves with very high roofs.", "nw");
		}
		if (step == 100 || step == 200)
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				manager.HighlightByCID("Stage:TopButtonBar:POIButton", "You can see a list of points of interest by pressing ~CmdMoveToPointOfInterest.\n\nThe list will grow as you explore more of the map.", "sw");
			}
			else
			{
				manager.HighlightByCID("Stage:TopButtonBar:POIButton", "You can see a list of points of interest by clicking this button or pressing ~CmdMoveToPointOfInterest.\n\nThe list will grow as you explore more of the map.", "sw");
			}
		}
	}
}

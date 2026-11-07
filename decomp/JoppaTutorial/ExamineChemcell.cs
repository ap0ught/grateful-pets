using Qud.UI;
using XRL;
using XRL.World;

namespace JoppaTutorial;

public class ExamineChemcell : TutorialStep
{
	public GameObject chemCell;

	public bool snapjawDead;

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		if (string.IsNullOrEmpty(title))
		{
			return true;
		}
		if (step == 100 && GameManager.Instance.CurrentGameView == "PopupMessage")
		{
			string lastPopupID = PopupMessage.lastPopupID;
			if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu:" + chemCell.ID) && !chemCell.Understood())
			{
				if (command.simpleText == "examine" || command.command == "Cancel")
				{
					return true;
				}
				return false;
			}
		}
		return true;
	}

	public override int AdjustExaminationRoll(int result)
	{
		return 9999;
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (step == 0)
		{
			step = 10;
			TutorialManager.ShowIntermissionPopupAsync("The bear is dead! Looks like it dropped something, too.\n\nBut first, let's use this opportunity to heal.", () =>
			{
				step = 20;
			});
		}
		else if (step == 20)
		{
			if (The.Player.hitpoints == The.Player.baseHitpoints)
			{
				step = 100;
			}
			else
			{
				manager.HighlightObject(The.Player, "You regain hitpoints naturally as turns pass. You can pass a few turns by waiting, or if there are no hostile creatures around, you can {{W|rest until healed}}.\n\nPress ~CmdWaitUntilHealed", "ne");
			}
		}
		else
		{
			if (step != 100)
			{
				return;
			}
			if (The.Player?.CurrentCell == null)
			{
				manager.Highlight(null, null, null);
				return;
			}
			if (chemCell == null)
			{
				chemCell = The.Player.CurrentCell.ParentZone.FindObject("TutorialChemCell");
				if (chemCell == null)
				{
					chemCell = The.Player.Inventory.FindObjectByBlueprint("TutorialChemCell");
				}
				chemCell?.RequireID();
			}
			if (chemCell.Understood() && (GameManager.Instance.CurrentGameView == "PopupMessage" || GameManager.Instance.CurrentGameView == "Stage"))
			{
				TutorialManager.ShowCIDPopupAsync("PopupMessage", "It's a chem cell. It can be used later to power other artifacts you find.", "s", "[~Accept] Continue", 0, 0);
				TutorialManager.AdvanceStep(new ExploreDeeper());
			}
			if (chemCell != null && !chemCell.IsValid())
			{
				TutorialManager.AdvanceStep(new ExploreDeeper());
			}
			if (CurrentGameView == "Stage")
			{
				if (chemCell.InInventory == The.Player)
				{
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						manager.HighlightObject(The.Player, "You picked up the odd trinket automatically because it is an artifact.\n\nPress ~CmdCharacter to investigate it.", "ne");
					}
					else
					{
						manager.HighlightObject(The.Player, "You picked up the odd trinket automatically because it is an artifact.\n\nPress ~CmdInventory to investigate it.", "ne");
					}
				}
				else
				{
					manager.HighlightObject(chemCell, "The bear is dead! Looks like it dropped something, too.", "ne");
				}
			}
			if (CurrentGameView == "StatusScreensScreen")
			{
				if (SingletonWindowBase<StatusScreensScreen>.instance.CurrentScreen == 2)
				{
					if (chemCell.InInventory == The.Player)
					{
						if (step == 0)
						{
							step = 1;
							TutorialManager.ShowIntermissionPopupAsync("What's that???");
						}
						else
						{
							manager.HighlightByCID("InventoryLine:Item:" + chemCell.ID, "This is an artifact beyond simple understanding. Try examining it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
						}
					}
				}
				else
				{
					manager.HighlightByCID("StatusScreensScreenTab:2", "Select the Inventory && Equipment tab.\n\nYou can use ~Page Left and ~Page Right to navigate between tabs.", "se", 32, 16);
				}
			}
			if (!(GameManager.Instance.CurrentGameView == "PopupMessage"))
			{
				return;
			}
			string lastPopupID = PopupMessage.lastPopupID;
			if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu:" + chemCell.ID))
			{
				if (step == 0)
				{
					step = 1;
					TutorialManager.ShowIntermissionPopupAsync("What's that???");
				}
				else
				{
					manager.HighlightByCID("QudTextMenuItem:examine", "This is an artifact beyond simple understanding. Try examining it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
			}
		}
	}
}

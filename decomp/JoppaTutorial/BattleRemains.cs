using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Capabilities;

namespace JoppaTutorial;

public class BattleRemains : TutorialStep
{
	public GameObject battleAxe;

	public bool equippedPopupShown;

	public bool lookPopupShown;

	public int showingTooltip;

	public bool infoShown;

	public bool snapjawDead;

	public bool sawBattle;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowInventoryInteract(GameObject go)
	{
		if (step == 0 && showingTooltip < 3 && go == battleAxe)
		{
			Popup.Show("You should view the tooltip before equipping the axe.");
			return false;
		}
		return true;
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		if (string.IsNullOrEmpty(title))
		{
			return true;
		}
		if (step == 0 && title == "InventoryActionMenu:" + battleAxe.ID)
		{
			if (battleAxe.InInventory != The.Player)
			{
				if (command.simpleText == "get")
				{
					return true;
				}
				if (command.simpleText == "equip (auto)")
				{
					return true;
				}
				if (command.command == "Cancel")
				{
					return true;
				}
				return false;
			}
			if (showingTooltip < 3)
			{
				if (command.command == "Cancel")
				{
					return true;
				}
			}
			else
			{
				if (command.simpleText == "get")
				{
					return true;
				}
				if (command.simpleText == "equip (auto)")
				{
					return true;
				}
				if (command.command == "Cancel")
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public override bool StartingLineTooltip(GameObject go, GameObject compareGo)
	{
		if (go == battleAxe && showingTooltip == 0)
		{
			showingTooltip = 1;
		}
		return true;
	}

	public override bool AllowTooltipUpdate()
	{
		if (showingTooltip != 0)
		{
			return showingTooltip > 3;
		}
		return true;
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (The.Player?.CurrentCell == null)
		{
			manager.Highlight(null, null, null);
			return;
		}
		if (battleAxe == null)
		{
			battleAxe = The.Player.CurrentCell.ParentZone.FindObject("TutorialBattleAxe");
			if (battleAxe == null)
			{
				battleAxe = The.Player.Inventory.FindObjectByBlueprint("TutorialBattleAxe");
			}
			battleAxe?.RequireID();
		}
		if (battleAxe != null && !battleAxe.IsValid())
		{
			TutorialManager.ShowCellPopup(The.Player.CurrentCell.Location, "Huh, you somehow destroyed the battle axe. Let's move on.");
			TutorialManager.AdvanceStep(new GetBooks());
			return;
		}
		if (showingTooltip == 1)
		{
			if (ControlId.Get("DualPolatLooker:Left:DisplayName") != null)
			{
				showingTooltip = 3;
				TutorialManager.ShowIntermissionPopupAsync("The battle axe has better penetration and deals more damage than your dagger.", () =>
				{
					showingTooltip = 4;
				});
			}
			else if (ControlId.Get("PolatLooker:DisplayName") != null)
			{
				showingTooltip = 2;
				TutorialManager.ShowCIDPopupAsync("PolatLooker:Left:DisplayName", "Let’s take another look at weapon stats. Next to the {{c|→}} is 5. That means you’ll usually penetrate the armor of a creature with AV equal to 5.\r\n\r\nYou can penetrate armor multiple times, though, and each time you penetrate, you deal the weapon’s damage.\r\n\r\nlet’s equip it.", "ne", "[~Accept] Continue", 16, 16, 0f, () =>
				{
					showingTooltip = 4;
				});
			}
			else
			{
				showingTooltip = 0;
			}
		}
		if (CurrentGameView == "Stage")
		{
			manager.ClearHighlight();
			if (step != 0)
			{
				return;
			}
			if (The.Player.CurrentCell == battleAxe?.CurrentCell)
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightCell(battleAxe.CurrentCell.X, battleAxe.CurrentCell.Y, "A battle axe. Get it.\n\nHit ~CmdUse", "ne");
				}
				else
				{
					manager.HighlightCell(battleAxe.CurrentCell.X, battleAxe.CurrentCell.Y, "A battle axe. Get it.\n\nHit ~CmdGet", "ne");
				}
			}
			else if (battleAxe.CurrentCell != null)
			{
				if (sawBattle || battleAxe.CurrentCell.IsVisible())
				{
					if (!sawBattle)
					{
						AutoAct.Setting = "";
					}
					sawBattle = true;
					manager.HighlightCell(battleAxe.CurrentCell.X, battleAxe.CurrentCell.Y, "Looks like there was some sort of battle to the north. Let’s grab the gear that was left behind.", "ne");
				}
				else
				{
					manager.HighlightCell(battleAxe.CurrentCell.X, battleAxe.CurrentCell.Y, "Keep moving forward.", "ne");
				}
			}
			if (battleAxe.InInventory == The.Player)
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightCell(The.PlayerCell.X, The.PlayerCell.Y, "Let's take a look at the Equipment screen.\n\nPress ~CmdCharacter", "se");
				}
				else
				{
					manager.HighlightCell(The.PlayerCell.X, The.PlayerCell.Y, "Let's take a look at the Equipment screen.\n\nPress ~CmdEquipment", "se");
				}
			}
			return;
		}
		if (GameManager.Instance.CurrentGameView == "PopupMessage")
		{
			string lastPopupID = PopupMessage.lastPopupID;
			if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu"))
			{
				if (battleAxe.InInventory == The.Player)
				{
					if (showingTooltip < 3)
					{
						manager.HighlightByCID("QudTextMenuItem:Cancel", "Hold on, we want to show you the tooltip compare feature.", "ne");
					}
					else
					{
						manager.HighlightByCID("QudTextMenuItem:equip (auto)", "Equip the battle axe.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
				}
				else if (battleAxe?.Equipped == null)
				{
					manager.HighlightByCID("QudTextMenuItem:get", "A battle axe. Get it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
				return;
			}
		}
		if (!(CurrentGameView == "StatusScreensScreen"))
		{
			return;
		}
		if (SingletonWindowBase<StatusScreensScreen>.instance.CurrentScreen == 2)
		{
			if (battleAxe.InInventory == The.Player)
			{
				if (showingTooltip == 0)
				{
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						manager.HighlightByCID("InventoryLine:Item:" + battleAxe.ID, "Press ~GamepadAlt + ~Take A Step to view the tooltip.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
					else
					{
						manager.HighlightByCID("InventoryLine:Item:" + battleAxe.ID, "Mouse over the battle axe and hover or press {{W|Alt}} to view the tooltip.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
				}
				else
				{
					manager.HighlightByCID("InventoryLine:Item:" + battleAxe.ID, "Equip the battle axe since it's better than your dagger.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
			}
			else if (battleAxe.Equipped == The.Player)
			{
				TutorialManager.AdvanceStep(new GetBooks());
			}
		}
		else
		{
			manager.HighlightByCID("StatusScreensScreenTab:2", "Select the Inventory && Equipment tab.\n\nYou can use ~Page Left and ~Page Right to navigate between tabs.", "se", 32, 16);
		}
	}
}

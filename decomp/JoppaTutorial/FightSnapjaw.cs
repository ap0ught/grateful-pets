using Genkit;
using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;

namespace JoppaTutorial;

public class FightSnapjaw : TutorialStep
{
	public GameObject snapjaw;

	public GameObject leatherArmor;

	private Location2D snapjawLocation;

	public bool snapjawDead;

	public bool lookPopupShown;

	public bool equippedPopupShown;

	public float timer;

	public bool equipmentPopupShown;

	public void SnapjawSeen(Location2D location)
	{
		snapjawLocation = location;
		TutorialManager.ShowCellPopup(location, "What's that??? Some sort of creature. Take a closer look at it.", "ne", 6, 6, () =>
		{
			step = 200;
		});
		step = 100;
	}

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell.X > 27)
		{
			Popup.Show("Before you move on, you should loot the corpse of the snapjaw.");
			return false;
		}
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		if (string.IsNullOrEmpty(title))
		{
			return true;
		}
		if (title == "InventoryActionMenu:" + leatherArmor?.ID)
		{
			if (!lookPopupShown && (command.simpleText == "get" || command.command == "Cancel"))
			{
				return true;
			}
			if (lookPopupShown && (command.simpleText == "equip (auto)" || command.command == "Cancel"))
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override bool AllowCommand(string id)
	{
		if (CurrentGameView == "Stage")
		{
			switch (id)
			{
			case "Accept":
			case "Cancel":
			case "CmdUse":
			case "Command:CmdUse":
			case "CmdSystemMenu":
			case "Command:CmdSystemMenu":
			case "CmdNone":
				return true;
			}
			_ = GameManager.Instance.CurrentGameView == "Looker";
			if (step >= 200 && step < 300)
			{
				GameObject gameObject = snapjaw;
				if (gameObject == null || !gameObject.IsValid())
				{
					return true;
				}
				if (id != "CmdLook" && id != "Command:CmdLook" && id != "CmdEquipment" && id != "Command:CmdEquipment" && id != "CmdInventory" && id != "Command:CmdInventory" && id != "Command:CmdCharacter" && id != "CmdCharacter" && id != "AdventureMouseInteract" && id != "Command:CmdSystemMenu" && id != "CmdNone" && GameManager.IsOnGameContext())
				{
					Popup.Show("Take a look at the snapjaw before we continue.");
					return false;
				}
			}
		}
		return true;
	}

	public override void LateUpdate()
	{
		if (The.Player?.CurrentCell == null)
		{
			manager.Highlight(null, null, null);
			return;
		}
		if (snapjaw == null)
		{
			snapjaw = The.Player.CurrentCell.ParentZone.FindObject("TutorialSnapjaw");
			snapjaw?.RequireID();
		}
		if (step == 500)
		{
			if (leatherArmor == null)
			{
				leatherArmor = The.Player.CurrentZone?.FindObject("TutorialLeatherArmor");
			}
			if (leatherArmor == null)
			{
				leatherArmor = The.Player.Inventory?.FindObjectByBlueprint("TutorialChemCell");
			}
		}
		if (leatherArmor != null && !leatherArmor.IsValid())
		{
			TutorialManager.ShowCellPopup(The.Player.CurrentCell.Location, "Huh, you somehow destroyed the leather armor. Let's move on.");
			TutorialManager.AdvanceStep(new FightBear());
			return;
		}
		if (snapjawDead && step < 400)
		{
			step = 500;
		}
		if (step == 0)
		{
			if (CurrentGameView == "Stage")
			{
				if (The.Player.CurrentCell.X < 17)
				{
					manager.HighlightCell(17, 13, "Let's explore further into the cave.", "se");
				}
				else
				{
					manager.HighlightCell(26, 13, "Let's explore further into the cave.\n\nWalk down the passage.", "se");
				}
			}
			else
			{
				manager.ClearHighlight();
			}
		}
		else
		{
			if (step == 100)
			{
				return;
			}
			if (step >= 200 && step < 300)
			{
				if (GameManager.Instance.CurrentGameView == "DynamicPopupMessage")
				{
					if (step == 200)
					{
						step = 300;
						TutorialManager.ShowIntermissionPopupAsync("Looking at a creature shows its description and some other important details. Every object in the world can be looked at.", () =>
						{
							TutorialManager.ShowCIDPopupAsync("PopupMessage:ContextItemText", "The snapjaw is hostile! Let's get ready for a fight.", "ne", "[~Accept] Continue", 0, 8);
						});
					}
					return;
				}
				if (GameManager.Instance.CurrentGameView == "PopupMessage")
				{
					string lastPopupID = PopupMessage.lastPopupID;
					if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu"))
					{
						manager.HighlightByCID("QudTextMenuItem:look", "Inspect the snapjaw.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
						return;
					}
				}
				if (GameManager.Instance._CurrentGameView == "Stage")
				{
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						manager.HighlightCell(snapjawLocation.X, snapjawLocation.Y, "Use ~LookDirection to look at the snapjaw.", "sw");
					}
					else
					{
						manager.HighlightCell(snapjawLocation.X, snapjawLocation.Y, "Press ~CmdLook or ~AdventureMouseInteract the snapjaw.", "sw");
					}
				}
				else if (GameManager.Instance.CurrentGameView == "Looker")
				{
					if (Look.lookingAt == snapjaw)
					{
						if (step == 200)
						{
							step = 300;
							TutorialManager.ShowIntermissionPopupAsync("Here we have a description of the creature, along with some details about them.", () =>
							{
								TutorialManager.ShowCIDPopupAsync("PolatLooker:SubHeader", "The snapjaw is hostile! Let's get ready for a fight.", "ne", "[~Accept] Continue", 0, 12);
							});
						}
					}
					else
					{
						manager.HighlightCell(snapjawLocation.X, snapjawLocation.Y, "Look view lets you look around and investigate your surroundings.\n\nMove the cursor to the snapjaw.", "ne", 12f, 12f);
					}
				}
				else
				{
					manager.ClearHighlight();
				}
			}
			else if (step == 300)
			{
				if (GameManager.Instance._CurrentGameView == "Stage")
				{
					step = 400;
					TutorialManager.ShowCellPopup(snapjaw.CurrentCell.Location, "All actions are turn-based, meaning you take a turn, then other creatures take a turn. For the most part, creatures won't act until you do.\r\n\r\nIf you ever get panicked, just slow down and consider your next move. There's no rush.");
				}
			}
			else if (step == 400)
			{
				if (leatherArmor != null || snapjawDead)
				{
					step = 450;
					manager.ClearHighlight();
					manager.After(1000, () =>
					{
						step = 500;
					});
				}
				else if (GameManager.Instance._CurrentGameView == "Stage")
				{
					GameObject gameObject = snapjaw;
					if (gameObject != null && gameObject.DistanceTo(The.Player) == 1)
					{
						manager.HighlightCell(snapjaw.CurrentCell.X, snapjaw.CurrentCell.Y, "You can attack a hostile creature by moving into its square. This is called {{W|bump attacking}}.", "se");
					}
					else
					{
						manager.HighlightCell(snapjaw.CurrentCell.X, snapjaw.CurrentCell.Y, "Take a step toward the snapjaw.", "se");
					}
				}
				else
				{
					manager.ClearHighlight();
				}
			}
			else if (step == 500)
			{
				manager.ClearHighlight();
				if (CurrentGameView == "Stage")
				{
					if (The.Player.CurrentCell == leatherArmor.CurrentCell)
					{
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							manager.HighlightCell(leatherArmor.CurrentCell.X, leatherArmor.CurrentCell.Y, "You can pick up things that are in the same space as you.\n\nPress ~CmdUse", "ne");
						}
						else
						{
							manager.HighlightCell(leatherArmor.CurrentCell.X, leatherArmor.CurrentCell.Y, "You can pick up things that are in the same space as you.\n\nPress ~CmdGet", "ne");
						}
					}
					else if (leatherArmor.CurrentCell != null)
					{
						string text = " or press ~AdventureMouseInteract.";
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							text = ".";
						}
						manager.HighlightCell(leatherArmor.CurrentCell.X, leatherArmor.CurrentCell.Y, "Whew! You killed the snapjaw and earned some experience. It looks like they dropped a piece of equipment, too.\n\nPress ~CmdGetFrom and then choose a direction" + text, "ne");
					}
				}
				if (GameManager.Instance.CurrentGameView == "PopupMessage")
				{
					string lastPopupID2 = PopupMessage.lastPopupID;
					if (lastPopupID2 != null && lastPopupID2.StartsWith("InventoryActionMenu"))
					{
						if (leatherArmor.InInventory == The.Player)
						{
							if (!lookPopupShown)
							{
								TutorialManager.ShowCIDPopupAsync("PopupMessage:ContextItemText", "Let's look at the leather armor you picked up.\n\nArmor has an armor value ({{b|♦}}{{y|2}}) and a dodge value ({{K|○}}{{y|0}})\n\nBoth help you avoid damage.");
								lookPopupShown = true;
							}
							else
							{
								manager.HighlightByCID("QudTextMenuItem:equip (auto)", "Let's equip it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
							}
						}
						else if (leatherArmor?.Equipped == null)
						{
							manager.HighlightByCID("QudTextMenuItem:get", "It's leather armor. This time, get it instead of equipping it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
						}
					}
				}
				if (leatherArmor.InInventory == The.Player && CurrentGameView == "Stage")
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
				if (!(CurrentGameView == "StatusScreensScreen"))
				{
					return;
				}
				if (SingletonWindowBase<StatusScreensScreen>.instance.CurrentScreen == 2)
				{
					if (!equipmentPopupShown)
					{
						equipmentPopupShown = true;
						TutorialManager.ShowIntermissionPopupAsync("This is your equipment and inventory screen. Equipped items on the left, and inventory on the right.");
					}
					else if (leatherArmor.InInventory == The.Player)
					{
						if (!lookPopupShown)
						{
							TutorialManager.ShowCIDPopupAsync("InventoryLine:Item:" + leatherArmor.ID, "Let's look at the leather armor you picked up.\n\nArmor has an armor value ({{b|♦}}{{y|2}}) and a dodge value ({{K|○}}{{y|0}})\n\nBoth help you avoid damage.");
							lookPopupShown = true;
						}
						else
						{
							manager.HighlightByCID("InventoryLine:Item:" + leatherArmor.ID, "Let's equip it.", "nw", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
						}
					}
					else if (leatherArmor.Equipped == The.Player && !equippedPopupShown)
					{
						equippedPopupShown = true;
						TutorialManager.ShowCIDPopupAsync("EquipmentLine:Slot:body", "You can also equip items by dragging them from your inventory to their equipment slot.\n\nBy the way, you have several other tabs in your character sheet. Feel free to explore them later.", "ne", "[~Accept] Continue", 48, 8, 0f, () =>
						{
							TutorialManager.AdvanceStep(new FightBear());
						});
					}
				}
				else
				{
					manager.HighlightByCID("StatusScreensScreenTab:2", "Select the Inventory && Equipment tab.\n\nYou can use {{~Page Left}} and {{~Page Right}} to navigate between tabs.", "se", 32, 16);
				}
			}
			else
			{
				manager.ClearHighlight();
			}
		}
	}
}

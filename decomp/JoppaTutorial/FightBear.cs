using System;
using System.Collections.Generic;
using System.Linq;
using Genkit;
using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;

namespace JoppaTutorial;

public class FightBear : TutorialStep
{
	public GameObject bear;

	public GameObject chemCell;

	private Location2D bearLocation;

	public bool bearDead;

	public bool bearVisible;

	public override bool AllowTargetPick(GameObject go, Type ability, List<Cell> target)
	{
		if (step == 300)
		{
			if (!bear.IsValid())
			{
				return true;
			}
			if (bear.DistanceTo(The.Player) == 1)
			{
				return true;
			}
			if (GameManager.IsOnGameContext())
			{
				Popup.Show("Wait for the bear to take a step towards you.");
				return false;
			}
		}
		if (step >= 800 && (target == null || !target.Any((Cell t) => t.GetObjectCount("TutorialBear") > 0)))
		{
			Popup.Show("Make sure the bear is in the path of your freezing ray.");
		}
		return true;
	}

	public void BearSeen(Location2D location)
	{
		bearLocation = location;
		TutorialManager.ShowCellPopup(location, "What's that??? Some sort of creature. Take a closer look at it.", "ne", 6, 6, () =>
		{
			step = 200;
		});
		step = 100;
	}

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell.GetObjectCount("TutorialBear") > 0 && step >= 700)
		{
			Popup.Show("It's quite dangerous to fight this bear in melee combat! Try backing away and using Freezing Ray.");
		}
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		string.IsNullOrEmpty(title);
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
				return true;
			}
			if (step == 100)
			{
				if (!bear.IsValid())
				{
					return true;
				}
				if (id != "CmdLook" && id != "Command:CmdLook" && id != "CmdEquipment" && id != "Command:CmdEquipment" && id != "CmdInventory" && id != "Command:CmdInventory" && id != "Command:CmdCharacter" && id != "CmdCharacter" && id != "AdventureMouseInteract" && id != "Command:CmdSystemMenu" && GameManager.IsOnGameContext())
				{
					Popup.Show("Let's take a look at the bear before we continue.");
					return false;
				}
			}
			if (step == 300)
			{
				if (!bear.IsValid())
				{
					return true;
				}
				if (bear.DistanceTo(The.Player) == 1)
				{
					return true;
				}
				if (id != "CmdWait" && id != "Command:CmdWait" && id != "CmdEquipment" && id != "Command:CmdEquipment" && id != "CmdInventory" && id != "Command:CmdInventory" && id != "Command:CmdCharacter" && id != "CmdCharacter" && id != "AdventureMouseContextAction" && id != "Cancel" && id != "CmdSystemMenu" && id != "Command:CmdSystemMenu" && id != "CmdNone" && GameManager.IsOnGameContext())
				{
					Popup.Show("Wait for the bear to take a step towards you.");
					return false;
				}
			}
		}
		return true;
	}

	public override void GameSync()
	{
		bearVisible = bear?.IsVisible() ?? false;
		if (step == 600)
		{
			The.Player.ActivatedAbilities.GetAbilityByCommand("CommandToggleRunning").Refresh();
		}
		if (step == 800)
		{
			The.Player.ActivatedAbilities.GetAbilityByCommand("CommandFreezingRay").Refresh();
		}
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (The.Player?.CurrentCell == null)
		{
			manager.Highlight(null, null, null);
			return;
		}
		if (bear == null)
		{
			bear = The.Player.CurrentCell.ParentZone.FindObject("TutorialBear");
			bear?.RequireID();
		}
		if (bear.IsInvalid())
		{
			TutorialManager.AdvanceStep(new ExamineChemcell());
		}
		else if (step == 0)
		{
			if (CurrentGameView == "Stage")
			{
				manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "Keep moving down the passage...", "se");
			}
			if (bearVisible)
			{
				TutorialManager.ShowCellPopup(bear.CurrentCell.Location, "Oh! Another creature. Let's look at it more closely.\n\nWhen you come across something new, it's always a good idea to inspect it.");
				step = 100;
			}
		}
		else if (step >= 100 && step < 200)
		{
			if (GameManager.Instance.CurrentGameView == "DynamicPopupMessage")
			{
				TutorialManager.ShowIntermissionPopupAsync("It's a bear.", () =>
				{
					TutorialManager.ShowCIDPopupAsync("PopupMessage:ContextItemText", "The bear is wounded, but it's tough. This'll be a harder fight.", "ne", "[~Accept] Continue", 0, 8, 0f, () =>
					{
						step = 300;
					});
				});
				return;
			}
			if (GameManager.Instance.CurrentGameView == "PopupMessage")
			{
				string lastPopupID = PopupMessage.lastPopupID;
				if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu"))
				{
					manager.HighlightByCID("QudTextMenuItem:look", "Inspect the bear.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					return;
				}
			}
			if (GameManager.Instance._CurrentGameView == "Stage")
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "Use ~LookDirection to look at the bear.", "sw");
				}
				else
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "Press ~CmdLook or ~AdventureMouseInteract the bear.", "sw");
				}
			}
			else
			{
				if (!(GameManager.Instance.CurrentGameView == "Looker"))
				{
					return;
				}
				if (Look.lookingAt == bear)
				{
					if (step != 100)
					{
						return;
					}
					step = 200;
					TutorialManager.ShowIntermissionPopupAsync("It's a bear.", () =>
					{
						TutorialManager.ShowCIDPopupAsync("PolatLooker:SubHeader", "The bear is wounded, but it's tough. This'll be a harder fight.", "ne", "[~Accept] Continue", 0, 12, 0f, () =>
						{
							step = 300;
						});
					});
				}
				else
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "Look view lets you look around and investigate your surroundings.\n\nMove the cursor to the bear.", "ne", 12f, 12f);
				}
			}
		}
		else if (step == 300)
		{
			if (!bear.IsValid())
			{
				step = 500;
			}
			if (!(GameManager.Instance._CurrentGameView == "Stage"))
			{
				return;
			}
			if (bear.DistanceTo(The.Player) == 1)
			{
				if (CurrentGameView == "Stage")
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "Bump attack the bear.", "se");
				}
			}
			else if (CurrentGameView == "Stage")
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "You can wait a turn for the bear to approach. That way, it won't be in range to attack you when it takes its turn.\n\nTap ~Take A Step without choosing a direction with the stick to pass a turn.", "se");
				}
				else
				{
					manager.HighlightCell(bear.CurrentCell.X, bear.CurrentCell.Y, "You can wait a turn for the bear to approach. That way, it won't be in range to attack you when it takes its turn.\n\nPress %CmdWait or ~AdventureMouseContextAction on your character to pass a turn.", "se");
				}
			}
		}
		else if (step == 400)
		{
			if (CurrentGameView == "Stage")
			{
				manager.HighlightByCID("Stage:MessageLog", "You missed, then the bear attacked and missed.\n\nAttack again.", "sw", 2, 2);
			}
			if (The.Player.hitpoints < The.Player.baseHitpoints)
			{
				step = 500;
			}
		}
		else if (step == 500)
		{
			TutorialManager.ShowCIDPopupAsync("$HPBar", "This time you hit, but you failed to penetrate the bear's armor.\n\nThen, the bear hit you and DID penetrate your armor. You took 4 damage.", "sw");
			step = 600;
		}
		else if (step == 600)
		{
			if (CurrentGameView == "Stage")
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightByCID("AbilityBar:Button:CommandToggleRunning", "Let's try using an ability. Look at your ability bar. Let's start sprinting. \n\nSelect it with ~Next Ability and ~Previous Ability then press ~Use Ability.", "nw", 4, 4);
				}
				else
				{
					manager.HighlightByCID("AbilityBar:Button:CommandToggleRunning", "Let's try using an ability. Look at your ability bar. Let's start sprinting. You can click the button or hit the hotkey {{hotkey|~CmdAbility1}}.", "nw", 4, 4);
				}
			}
			if (The.Player.HasEffect("Running"))
			{
				step = 700;
			}
		}
		else if (step == 700)
		{
			if (CurrentGameView == "Stage")
			{
				manager.HighlightCell(19, 13, "Sprinting doubles your movespeed but ends if you make an attack. Let's sprint back down the corridor to put some distance between us and the bear.", "sw", 2f, 2f);
			}
			if (The.Player.DistanceTo(bear) >= 2 && bearVisible)
			{
				step = 800;
			}
		}
		else if (step == 800)
		{
			if (!bearVisible)
			{
				step = 700;
			}
			if (CurrentGameView == "Stage")
			{
				if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightByCID("AbilityBar:Button:CommandFreezingRay", "Every character has Sprint, but most of your abilities are determined by traits like your mutations. You have Freezing Ray, so you can shoot frost out of your hands.\n\nSelect it with ~Next Ability and ~Previous Ability then press ~Use Ability.", "nw", 4, 4);
				}
				else
				{
					manager.HighlightByCID("AbilityBar:Button:CommandFreezingRay", "Every character has Sprint, but most of your abilities are determined by traits like your mutations. You have Freezing Ray, so you can shoot frost out of your hands. Select it.\n\nPress {{hotkey|~CmdAbility2}}", "nw", 4, 4);
				}
			}
		}
		else
		{
			manager.ClearHighlight();
		}
	}
}

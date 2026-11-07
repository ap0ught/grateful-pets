using System.Linq;
using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace JoppaTutorial;

public class MoveToChest : TutorialStep
{
	public GameObject chest;

	public GameObject dagger;

	public GameObject torch;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (cell.X > 17)
		{
			Popup.Show("Before you move on, you should equip yourself from the nearby chest.");
			return false;
		}
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowCommand(string id)
	{
		if (id == "Take All")
		{
			return false;
		}
		return base.AllowCommand(id);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		if (string.IsNullOrEmpty(title))
		{
			return true;
		}
		if (step == 100)
		{
			if (title == "InventoryActionMenu:(noid)")
			{
				if (command.command == "Cancel")
				{
					return true;
				}
				return false;
			}
			if (title == "InventoryActionMenu:" + torch.ID)
			{
				if (command.simpleText == "equip (auto)" || command.command == "Cancel")
				{
					return true;
				}
				SoundManager.PlayUISound("Sounds/UI/ui_notification_warning", 1f, Combat: false, Interface: true);
				return false;
			}
			if (title == "InventoryActionMenu:" + dagger.ID)
			{
				if (command.simpleText == "equip (auto)" || command.command == "Cancel")
				{
					return true;
				}
				SoundManager.PlayUISound("Sounds/UI/ui_notification_warning", 1f, Combat: false, Interface: true);
				return false;
			}
		}
		return true;
	}

	public override void OnTrigger(string trigger)
	{
		if (!(trigger == "OpeningStoryClosed"))
		{
			return;
		}
		TutorialManager.ShowIntermissionPopupAsync("This is the main gameplay stage. At the top of the screen are your attributes, HP, and XP.\n\nOn the right are action buttons, minimap, and the message log.\n\nAt the bottom is your ability hotbar.", () =>
		{
			TutorialManager.ShowIntermissionPopupAsync("Let's explore this cave we've found ourselves in.", () =>
			{
				step = 100;
			});
		});
	}

	public override void LateUpdate()
	{
		GameObject gameObject = chest;
		if (gameObject != null && !gameObject.IsValid())
		{
			manager.Highlight(null, null, null);
			Popup.Show("Huh, you destroyed the tutorial chest we were going to teach you how to use.\n\nGo ahead and pick of the torch and dagger from the floor.");
			TutorialManager.AdvanceStep(new FightSnapjaw());
			return;
		}
		GameObject gameObject2 = torch;
		if ((gameObject2 != null && !gameObject2.IsValid()) || torch?.InInventory?.IsPlayer() == true || torch?.equippedOrSelf()?.IsPlayer() == true)
		{
			GameObject gameObject3 = dagger;
			if ((gameObject3 != null && !gameObject3.IsValid()) || dagger?.InInventory?.IsPlayer() == true || dagger?.equippedOrSelf()?.IsPlayer() == true)
			{
				manager.Highlight(null, null, null);
				TutorialManager.AdvanceStep(new FightSnapjaw());
				return;
			}
		}
		if (The.Player?.CurrentCell == null)
		{
			manager.Highlight(null, null, null);
			return;
		}
		if (chest == null)
		{
			chest = The.Player.CurrentCell.ParentZone.FindObject("TutorialChest1");
			if (chest != null)
			{
				dagger = chest.GetPart<Inventory>().Objects.Where((GameObject o) => o.Blueprint == "TutorialDagger").FirstOrDefault();
				dagger?.RequireID();
				torch = chest.GetPart<Inventory>().Objects.Where((GameObject o) => o.Blueprint == "TutorialTorch").FirstOrDefault();
				torch?.RequireID();
			}
		}
		if (step == 0 && The.Player != null)
		{
			foreach (GameObject equippedObject in The.Player.Body.GetEquippedObjects())
			{
				equippedObject.Obliterate();
			}
			step = 50;
		}
		if (step == 50 && GameManager.Instance.CurrentGameView == "Stage")
		{
			step = 75;
		}
		if (step == 100)
		{
			if (GameManager.Instance.CurrentGameView == "PopupMessage")
			{
				string lastPopupID = PopupMessage.lastPopupID;
				if (lastPopupID != null && lastPopupID.StartsWith("InventoryActionMenu"))
				{
					string lastPopupID2 = PopupMessage.lastPopupID;
					if (lastPopupID2 != null && lastPopupID2.StartsWith("InventoryActionMenu:(noid)"))
					{
						manager.HighlightByCID("QudTextMenuItem:cancel", "<no message>", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
					else
					{
						manager.HighlightByCID("QudTextMenuItem:equip (auto)", "<no message>", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
					return;
				}
			}
			if (GameManager.Instance.CurrentGameView == "PickGameObject")
			{
				if (torch != null && torch.IsValid() && torch.InInventory == chest)
				{
					manager.HighlightByCID("PickGameObject:Item:" + torch.ID, "A torch. When it's nighttime or when you're underground, you need a torch equipped to see. Select it.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
				else if (dagger != null && dagger.IsValid() && dagger.InInventory == chest)
				{
					manager.HighlightByCID("PickGameObject:Item:" + dagger.ID, "Equip the dagger too.", "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
				else
				{
					manager.ClearHighlight();
				}
			}
			else if (GameManager.Instance.CurrentGameView == "Stage")
			{
				if (The.Player.CurrentCell.X == 15 && The.Player.CurrentCell.Y == 12)
				{
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						manager.HighlightCell(16, 12, "You can interact with objects you're next to. Open the chest.\n\nPress {{hotkey|~Accept}}", "ne");
					}
					else
					{
						manager.HighlightCell(16, 12, "You can interact with objects you're next to. Open the chest.\n\nPress {{hotkey|~Accept}} or {{hotkey|~AdventureMouseContextAction}}", "ne");
					}
				}
				else if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
				{
					manager.HighlightCell(15, 12, "Looks like there's a chest over at the end of the room. Let's walk over to it.\n\nHold ~IndicateDirection in the direction you want to move and press ~Take A Step.", "ne");
				}
				else
				{
					manager.HighlightCell(15, 12, "Looks like there's a chest over at the end of the room. Let's walk over to it.\n\nYou can click ~AdventureNavMouseLeftClick the square you want to move to, or use the {{hotkey|arrow keys}} or {{hotkey|numpad.}}\n\nWith arrow keys, you can press {{hotkey|shift+arrow keys}} to move diagonally.", "ne");
				}
			}
			else
			{
				manager.ClearHighlight();
			}
		}
		else
		{
			manager.ClearHighlight();
		}
	}
}

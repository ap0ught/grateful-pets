using System.Linq;
using Genkit;
using Qud.UI;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;

namespace JoppaTutorial;

public class MakeCamp : TutorialStep
{
	public GameObject campfire;

	public GameObject beetle;

	public GameObject witchwood;

	public GameObject dagger;

	public override bool BeforePlayerEnterCell(Cell cell)
	{
		if (step == 500 && cell?.ParentZone?.ZoneID == "JoppaWorld")
		{
			return true;
		}
		return ConstrainToCurrentZone(cell);
	}

	public override bool AllowSelectedPopupCommand(string title, QudMenuItem command)
	{
		return true;
	}

	public override void OnTrigger(string trigger)
	{
		if (trigger == "AteRandom")
		{
			TutorialManager.ShowIntermissionPopupAsync("Tastes good.", () =>
			{
				step = 400;
			});
		}
	}

	public override void GameSync()
	{
		if (step == 0)
		{
			step = 10;
		}
	}

	public override bool OnTradeComplete()
	{
		step = 500;
		return true;
	}

	public override bool OnTradeOffer()
	{
		step = 420;
		return true;
	}

	public override void LateUpdate()
	{
		manager.ClearHighlight();
		if (campfire == null)
		{
			campfire = The.Player.CurrentZone?.FindObject("Campfire");
		}
		if (beetle == null)
		{
			beetle = The.Player.CurrentZone?.FindObject("TutorialClockworkBeetlePariah");
			witchwood = beetle?.GetPart<Inventory>().GetObjects().Where((GameObject o) => o.Blueprint == "Witchwood Bark")
				.FirstOrDefault();
			dagger = (from o in The.Player.GetPart<Inventory>().GetObjects()
				where o.Blueprint == "TutorialDagger"
				select o).FirstOrDefault();
		}
		if (CurrentGameView == "Stage" && The.Player.CurrentZone.ZoneID == "JoppaWorld")
		{
			TutorialManager.AdvanceStep(new ExploreWorldMap());
			return;
		}
		if (step == 10 && CurrentGameView == "Stage")
		{
			The.Player.GetPart<Stomach>().CookingCounter = The.Player.GetPart<Stomach>().CalculateCookingIncrement();
			The.Player.GetPart<Stomach>().UpdateHunger();
			step = 100;
			TutorialManager.ShowCIDPopupAsync("Stage:TopButtonBar:ZoneText", "We’re on the surface now. We can walk the whole world this way, or we can access the worldmap to fast travel.", "sw", "[~Accept] Continue", 16, 16, 0f, () =>
			{
				step = 200;
				TutorialManager.ShowCIDPopupAsync("Stage:TopButtonBar:FoodWaterStatus", "But first, you just became hungry. You need to eat.", "ne", "[~Accept] Continue", 16, 16, 0f, () =>
				{
					step = 250;
				});
			});
		}
		if (step == 250 && CurrentGameView == "Stage")
		{
			if (The.Player.CurrentCell.Location == Location2D.Get(48, 14))
			{
				step = 300;
			}
			else
			{
				manager.HighlightCell(48, 14, "Let's make camp. How about next to that beetle over there.", "sw");
			}
		}
		if (step == 300 && CurrentGameView == "Stage")
		{
			if (campfire != null)
			{
				manager.HighlightObject(campfire, "Use the campfire.", "nw", 4f, 4f);
			}
			else
			{
				manager.HighlightByCID("AbilityBar:Button:CommandSurvivalCamp", "Make camp.", "nw", 4, 4);
			}
			if (The.Player.GetPart<Stomach>().HungerLevel == 0 && CurrentGameView == "Stage")
			{
				step = 350;
			}
		}
		if (step >= 400 && step < 500)
		{
			GameObject gameObject = beetle;
			if (gameObject == null || !gameObject.IsValid() || witchwood?.InInventory != beetle)
			{
				step = 500;
			}
			if (CurrentGameView == "Stage")
			{
				if (The.Player.DistanceTo(beetle) <= 1)
				{
					string text = " or ~AdventureMouseContextAction.";
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						text = ".";
					}
					manager.HighlightObject(beetle, "Let's not be rude. Talk to the beetle.\n\nPress ~CmdUse" + text, "ne");
				}
				else
				{
					manager.HighlightObject(beetle, "Let's not be rude. Talk to the beetle.", "ne");
				}
			}
			if (GameManager.Instance.CurrentGameView == "PopupMessage")
			{
				string lastPopupID = PopupMessage.lastPopupID;
				if (lastPopupID != null && lastPopupID.StartsWith("Conversation:" + beetle.ID))
				{
					manager.HighlightByCID("QudTextMenuItem:trade", "Not much of a talker. But that's not true for everyone. As long as a creature isn't hostile, you can try to chat with them.\n\nLet see what the beetle has for trade.", (Media.sizeClass <= Media.SizeClass.Small) ? "ne" : "s", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
				}
			}
			if (CurrentGameView == "ModernTrade")
			{
				if (step == 400)
				{
					step = 405;
					TutorialManager.ShowCIDPopupAsync("Trade:Amounts", "The beetle's inventory is on the left, and your inventory is on the right.\n\nThe beetle has some witchwood bark. That's useful — it heals you when you eat it. Let's trade for it.", "s", "[~Accept] Continue", 16, 16, 0f, () =>
					{
						step = 410;
					});
				}
				if (step == 410)
				{
					if (witchwood != null && witchwood.InInventory == beetle && SingletonWindowBase<TradeScreen>.instance.howManySelected(witchwood) <= 0)
					{
						string text2 = " or ~AdventureMouseContextAction.";
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							text2 = ".";
						}
						manager.HighlightByCID("TradeLine:item:" + witchwood.ID, "The beetle has some witchwood bark. That's useful — it heals you when you eat it. Let's trade for it.\n\nPress ~CmdTradeAdd" + text2, "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
					else if (dagger != null && dagger.InInventory == The.Player && SingletonWindowBase<TradeScreen>.instance.howManySelected(dagger) <= 0)
					{
						string text3 = " or ~AdventureMouseContextAction.";
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							text3 = ".";
						}
						manager.HighlightByCID("TradeLine:item:" + dagger.ID, "We don't need this dagger any more, so we can trade it away.\n\nPress ~CmdTradeAdd" + text3, "ne", 64, TutorialManager.TWIDDLE_MENU_YPADDING);
					}
					else
					{
						manager.HighlightByCID("Trade:Offer", "Complete the trade.", "se");
					}
				}
			}
			if (step == 420 && CurrentGameView == "PopupMessage")
			{
				manager.HighlightByCID("PopupMessage", "The trade is uneven, so we'll have to pony up some money.\n\nIn the salt-stippled ecosystem of Qud, {{B|fresh water}} is currency. You need to drink it, too, so don't spend it all.", "se");
			}
		}
		if (step == 500 && CurrentGameView == "Stage")
		{
			if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
			{
				manager.HighlightByCID("Stage:TopButtonBar:UpButton", "Now to go to the world map. Try ascending again.\n\nPress ~CmdMoveU.", "sw");
			}
			else
			{
				manager.HighlightByCID("Stage:TopButtonBar:UpButton", "Now to go to the world map. Try ascending again.\n\nYou can click this button or press ~CmdMoveU.", "sw");
			}
		}
	}
}

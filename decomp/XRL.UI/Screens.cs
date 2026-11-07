using System.Collections.Generic;
using ConsoleLib.Console;
using Qud.UI;
using XRL.World;

namespace XRL.UI;

public class Screens
{
	public static Screens _Screens = new Screens();

	public static int CurrentScreen = 0;

	private static List<IScreen> ScreenList;

	private static Dictionary<string, IScreen> PopupScreens;

	public static ScreenBuffer OldBuffer = ScreenBuffer.create(80, 25);

	private Screens()
	{
		ScreenList = new List<IScreen>();
		ScreenList.Add(new SkillsAndPowersScreen());
		ScreenList.Add(new StatusScreen());
		ScreenList.Add(new InventoryScreen());
		ScreenList.Add(new EquipmentScreen());
		ScreenList.Add(new FactionsScreen());
		ScreenList.Add(new QuestLog());
		ScreenList.Add(new JournalScreen());
		ScreenList.Add(new TinkeringScreen());
		PopupScreens = new Dictionary<string, IScreen>();
		PopupScreens.Add("Factions", new FactionsScreen());
	}

	public static void Awake()
	{
	}

	public static void ShowPopup(string Screen, GameObject GO)
	{
		if (Screen == "Factions")
		{
			CurrentScreen = 4;
		}
		if (Options.ModernCharacterSheet && Options.ModernUI)
		{
			int startingScreen = 0;
			if (CurrentScreen == 0)
			{
				startingScreen = 0;
			}
			if (CurrentScreen == 1)
			{
				startingScreen = 1;
			}
			if (CurrentScreen == 2)
			{
				startingScreen = 2;
			}
			if (CurrentScreen == 3)
			{
				startingScreen = 2;
			}
			if (CurrentScreen == 4)
			{
				startingScreen = 6;
			}
			if (CurrentScreen == 5)
			{
				startingScreen = 5;
			}
			if (CurrentScreen == 6)
			{
				startingScreen = 4;
			}
			if (CurrentScreen == 7)
			{
				startingScreen = 3;
			}
			_ = StatusScreensScreen.show(startingScreen, GO).Result;
		}
		else
		{
			PopupScreens[Screen].Show(GO);
		}
	}

	public static void Show(GameObject GO)
	{
		if (Options.ModernUI && Options.ModernCharacterSheet)
		{
			int startingScreen = 0;
			if (CurrentScreen == 0)
			{
				startingScreen = 0;
			}
			if (CurrentScreen == 1)
			{
				startingScreen = 1;
			}
			if (CurrentScreen == 2)
			{
				startingScreen = 2;
			}
			if (CurrentScreen == 3)
			{
				startingScreen = 2;
			}
			if (CurrentScreen == 4)
			{
				startingScreen = 6;
			}
			if (CurrentScreen == 5)
			{
				startingScreen = 5;
			}
			if (CurrentScreen == 6)
			{
				startingScreen = 4;
			}
			if (CurrentScreen == 7)
			{
				startingScreen = 3;
			}
			_ = StatusScreensScreen.show(startingScreen, GO).Result;
			return;
		}
		GameManager.Instance.PushGameView("StatusScreens");
		OldBuffer.Copy(TextConsole.CurrentBuffer);
		ScreenReturn screenReturn = ScreenReturn.Next;
		while ((screenReturn = ScreenList[CurrentScreen].Show(GO)) != ScreenReturn.Exit)
		{
			if (screenReturn == ScreenReturn.Next)
			{
				CurrentScreen++;
			}
			if (screenReturn == ScreenReturn.Previous)
			{
				CurrentScreen--;
			}
			if (CurrentScreen < 0)
			{
				CurrentScreen = ScreenList.Count - 1;
			}
			if (CurrentScreen > ScreenList.Count - 1)
			{
				CurrentScreen = 0;
			}
		}
		Popup._TextConsole.DrawBuffer(OldBuffer);
		GameManager.Instance.PopGameView();
	}
}

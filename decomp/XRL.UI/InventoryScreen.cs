using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using XRL.Core;
using XRL.World;
using XRL.World.Parts;

namespace XRL.UI;

[UIView("Inventory", false, true, false, "Menu", "Inventory", false, 0, false)]
public class InventoryScreen : IScreen, IWantsTextConsoleInit
{
	private static Dictionary<char, GameObject> SelectionList = new Dictionary<char, GameObject>();

	private static Dictionary<char, CategorySelectionListEntry> CategorySelectionList = new Dictionary<char, CategorySelectionListEntry>();

	private static Dictionary<string, List<GameObject>> CategoryMap = new Dictionary<string, List<GameObject>>();

	private static Dictionary<string, InventoryCategory> CategoryList = new Dictionary<string, InventoryCategory>();

	private static List<GameObject> SortList;

	private static List<string> Categories = new List<string>();

	private static int StartObject = 0;

	private static int CategorySort = 0;

	private static bool bMore = false;

	private static InventoryCategory forceCategorySelect = null;

	private static TextConsole TextConsole;

	private static ScreenBuffer Buffer;

	public static int itemsSkippedByFilter = 0;

	public static int Selected = 0;

	public static int currentMaxWeight;

	public static string filterString = "";

	private static HotkeySpread hotkeySpread;

	public void Init(TextConsole console, ScreenBuffer buffer)
	{
		TextConsole = console;
		Buffer = buffer;
	}

	public static void ClearLists()
	{
		CategoryMap.Clear();
		SelectionList.Clear();
		CategorySelectionList.Clear();
		CategoryList.Clear();
		SortList.Clear();
	}

	public static void ResetNameCache(GameObject GO)
	{
		List<GameObject> objectsDirect = GO.Inventory.GetObjectsDirect();
		for (int i = 0; i < objectsDirect.Count; i++)
		{
			objectsDirect[i].ResetNameCache();
		}
	}

	public static void RebuildLists(GameObject GO)
	{
		hotkeySpread.restart();
		Inventory inventory = GO.Inventory;
		CategoryMap.Clear();
		SelectionList.Clear();
		itemsSkippedByFilter = 0;
		if (!Categories.CleanContains("Category"))
		{
			Categories.Add("Category");
		}
		List<GameObject> objectsDirect = inventory.GetObjectsDirect();
		for (int i = 0; i < objectsDirect.Count; i++)
		{
			GameObject gameObject = objectsDirect[i];
			if (gameObject.HasTag("HiddenInInventory"))
			{
				continue;
			}
			if (filterString != "" && !gameObject.GetDisplayName(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true).Contains(filterString, CompareOptions.IgnoreCase))
			{
				itemsSkippedByFilter++;
				continue;
			}
			gameObject.Seen();
			string inventoryCategory = gameObject.GetInventoryCategory();
			if (!CategoryList.ContainsKey(inventoryCategory))
			{
				CategoryList.Add(inventoryCategory, new InventoryCategory(inventoryCategory, Persist: true));
				Categories.Add(inventoryCategory);
			}
			if (!CategoryMap.ContainsKey(inventoryCategory))
			{
				CategoryMap.Add(inventoryCategory, new List<GameObject>());
			}
			CategoryMap[inventoryCategory].Add(gameObject);
		}
		List<string> list = new List<string>();
		foreach (string key2 in CategoryList.Keys)
		{
			if (!CategoryMap.ContainsKey(key2))
			{
				list.Add(key2);
			}
			else if (CategoryMap[key2].Count == 0)
			{
				list.Add(key2);
			}
		}
		foreach (string item in list)
		{
			if (CategoryList.ContainsKey(item))
			{
				CategoryList.Remove(item);
			}
			if (CategoryMap.ContainsKey(item))
			{
				CategoryMap.Remove(item);
			}
		}
		foreach (List<GameObject> value in CategoryMap.Values)
		{
			value.Sort((GameObject a, GameObject b) => a.SortVs(b, null, UseCategory: false));
		}
		while (CategorySort >= Categories.Count)
		{
			CategorySort--;
		}
		if (CategorySort == -1)
		{
			SortList = inventory.GetObjects();
			SortList.Sort((GameObject a, GameObject b) => a.SortVs(b, null, UseCategory: false));
		}
		else if (Categories[CategorySort] == "Category")
		{
			SortList = inventory.GetObjects();
			SortList.Sort((GameObject a, GameObject b) => a.SortVs(b));
		}
		else
		{
			if (CategoryMap.ContainsKey(Categories[CategorySort]))
			{
				SortList = CategoryMap[Categories[CategorySort]];
			}
			SortList.Sort((GameObject a, GameObject b) => a.SortVs(b, null, UseCategory: false));
		}
		int num = 0;
		bMore = false;
		if (CategorySort != -1 && Categories[CategorySort] == "Category")
		{
			CategorySelectionList.Clear();
			int num2 = 0;
			List<string> list2 = new List<string>();
			foreach (string key3 in CategoryList.Keys)
			{
				list2.Add(key3);
			}
			list2.Sort();
			for (int num3 = 0; num3 < list2.Count; num3++)
			{
				string key = list2[num3];
				InventoryCategory inventoryCategory2 = CategoryList[key];
				if (forceCategorySelect != null && inventoryCategory2 == forceCategorySelect)
				{
					if (num2 < StartObject)
					{
						StartObject = num2;
					}
					Selected = num2 - StartObject;
					forceCategorySelect = null;
				}
				if (num2 >= StartObject && num2 <= 21 + StartObject)
				{
					CategorySelectionList.Add(hotkeySpread.ch(), new CategorySelectionListEntry(inventoryCategory2));
					hotkeySpread.next();
					num++;
				}
				if (inventoryCategory2.Expanded && CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item2 in CategoryMap[inventoryCategory2.Name])
					{
						num2++;
						num++;
						if (num2 >= StartObject && num2 <= 21 + StartObject)
						{
							CategorySelectionList.Add(hotkeySpread.ch(), new CategorySelectionListEntry(item2));
							hotkeySpread.next();
						}
						else if (num2 > 21 + StartObject)
						{
							bMore = true;
							break;
						}
					}
				}
				if (CategoryList.ContainsKey(key))
				{
					CategoryList[key].Weight = 0;
					CategoryList[key].Items = 0;
				}
				if (CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item3 in CategoryMap[inventoryCategory2.Name])
					{
						if (item3.Physics != null)
						{
							CategoryList[key].Weight += item3.Physics.Weight;
						}
						CategoryList[key].Items++;
					}
				}
				if (num2 > 21 + StartObject)
				{
					bMore = true;
					break;
				}
				num2++;
			}
		}
		else
		{
			if (inventory == null)
			{
				return;
			}
			int num4 = 0;
			foreach (GameObject sort in SortList)
			{
				if (num4 >= StartObject && num4 <= 21 + StartObject)
				{
					SelectionList.Add(hotkeySpread.ch(), sort);
					hotkeySpread.next();
				}
				num4++;
				if (num4 > 21 + StartObject)
				{
					bMore = true;
					break;
				}
			}
		}
	}

	public ScreenReturn Show(GameObject GO)
	{
		GameManager.Instance.PushGameView("Inventory");
		filterString = "";
		ResetNameCache(GO);
		filterString = "";
		Keys keys = Keys.None;
		bool Done = false;
		StartObject = 0;
		Selected = 0;
		currentMaxWeight = GO.GetMaxCarriedWeight();
		hotkeySpread = HotkeySpread.get(new string[2] { "Menus", "UINav" });
		string text = "< {{W|7}} Character | Equipment {{W|9}} >";
		if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
		{
			text = "< {{W|" + ControlManager.getCommandInputDescription("Page Left", mapGlyphs: false) + "}} Character | Equipment {{W|" + ControlManager.getCommandInputDescription("Page Right", mapGlyphs: false) + "}} >";
		}
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		while (!Done)
		{
			while (true)
			{
				Event.ResetPool(resetMinEventPools: false);
				RebuildLists(GO);
				int num;
				while (true)
				{
					Buffer.Clear();
					Buffer.SingleBox(0, 0, 79, 24, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
					Buffer.Goto(35, 0);
					Buffer.Write("[ {{W|Inventory}} ]");
					if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
					{
						Buffer.Goto(60, 0);
						Buffer.Write(" {{W|" + ControlManager.getCommandInputDescription("Back", mapGlyphs: false) + "}} to exit ");
					}
					else
					{
						Buffer.Goto(60, 0);
						Buffer.Write(" {{W|ESC}} or {{W|5}} to exit ");
					}
					Buffer.Goto(79 - ColorUtility.StripFormatting(text).Length, 24);
					Buffer.Write(text);
					if (StartObject > 0)
					{
						Buffer.Goto(1, 0);
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
						{
							Buffer.Write("<more...>");
						}
						else
						{
							Buffer.Write("<{{W|8}} to scroll up>");
						}
					}
					StringBuilder stringBuilder = Event.NewStringBuilder();
					stringBuilder.Append("Total weight: {{Y|").Append(GO.GetCarriedWeight()).Append(" {{y|/}}  ")
						.Append(currentMaxWeight)
						.Append(" lbs.}}");
					Buffer.Goto(79 - ColorUtility.LengthExceptFormatting(stringBuilder), 23);
					Buffer.Write(stringBuilder.ToString());
					dictionary.Clear();
					num = 0;
					InventoryCategory inventoryCategory = null;
					GameObject gameObject = null;
					int num2 = 1;
					int num3 = 1;
					foreach (char key in CategorySelectionList.Keys)
					{
						if (CategorySelectionList[key].Category != null)
						{
							Buffer.Goto(num3, num2 + num);
							string text2 = "";
							if (num == Selected)
							{
								text2 = "{{Y|>}}";
								inventoryCategory = CategorySelectionList[key].Category;
							}
							else
							{
								text2 = " ";
							}
							StringBuilder stringBuilder2 = Event.NewStringBuilder();
							StringBuilder stringBuilder3 = Event.NewStringBuilder();
							char value = ((num == Selected) ? 'Y' : 'K');
							stringBuilder3.Append("{{").Append(value).Append('|');
							if (Options.ShowNumberOfItems)
							{
								stringBuilder3.Append(", ").Append(CategorySelectionList[key].Category.Items).Append((CategorySelectionList[key].Category.Items == 1) ? " item" : " items");
							}
							stringBuilder3.Append("}}");
							stringBuilder2.Append(" {{").Append((num == Selected) ? 'Y' : 'y').Append("|[")
								.Append(CategorySelectionList[key].Category.Weight)
								.Append("#]}}");
							if (CategorySelectionList[key].Category.Expanded)
							{
								if (num == Selected)
								{
									StringBuilder stringBuilder4 = Event.NewStringBuilder();
									stringBuilder4.Append(text2).Append(key).Append(") [-] {{K|[{{Y|")
										.Append(CategorySelectionList[key].Category.Name)
										.Append(stringBuilder3)
										.Append("}}]}}");
									Buffer.Write(stringBuilder4.ToString());
								}
								else
								{
									StringBuilder stringBuilder5 = Event.NewStringBuilder();
									stringBuilder5.Append(text2).Append(key).Append(") [-] {{K|[")
										.Append(CategorySelectionList[key].Category.Name)
										.Append(stringBuilder3)
										.Append("]}}");
									Buffer.Write(stringBuilder5.ToString());
								}
							}
							else if (num == Selected)
							{
								StringBuilder stringBuilder6 = Event.NewStringBuilder();
								stringBuilder6.Append(text2).Append(key).Append(") [+] {{K|[{{Y|")
									.Append(CategorySelectionList[key].Category.Name)
									.Append(stringBuilder3)
									.Append("}}]}}");
								Buffer.Write(stringBuilder6.ToString());
							}
							else
							{
								StringBuilder stringBuilder7 = Event.NewStringBuilder();
								stringBuilder7.Append(text2).Append(key).Append(") [+] {{K|[")
									.Append(CategorySelectionList[key].Category.Name)
									.Append(stringBuilder3)
									.Append("]}}");
								Buffer.Write(stringBuilder7.ToString());
							}
							Buffer.Goto(79 - ColorUtility.LengthExceptFormatting(stringBuilder2), num2 + num);
							Buffer.Write(stringBuilder2);
							dictionary.Add(key, num);
							num++;
							continue;
						}
						GameObject gameObject2 = CategorySelectionList[key].Object;
						string text3 = "";
						if (num == Selected)
						{
							text3 = "{{Y|>}}  ";
							gameObject = gameObject2;
						}
						else
						{
							text3 = "   ";
						}
						Buffer.Goto(num3, num2 + num);
						Buffer.Write(text3 + key + ")");
						Buffer.Goto(num3 + 6, num2 + num);
						if (gameObject2.Render != null)
						{
							if (!GO.IsConfused)
							{
								Buffer.Write(gameObject2.RenderForUI());
							}
							else
							{
								Buffer.Write(" ");
							}
							Buffer.Goto(num3 + 8, num2 + num);
						}
						Buffer.Write(gameObject2.DisplayName);
						int weight = gameObject2.Weight;
						StringBuilder stringBuilder8 = Event.NewStringBuilder();
						stringBuilder8.Append(" {{").Append((num == Selected) ? 'Y' : 'K').Append("|")
							.Append(weight)
							.Append("#}}")
							.Append('³');
						Buffer.Goto(80 - ColorUtility.LengthExceptFormatting(stringBuilder8), num2 + num);
						Buffer.Write(stringBuilder8);
						dictionary.Add(key, num);
						num++;
					}
					if (num == 0 && StartObject != 0)
					{
						break;
					}
					if (Selected < num)
					{
						if (bMore)
						{
							Buffer.Goto(num3, 24);
							if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Gamepad)
							{
								Buffer.Write("<...more>");
							}
							else
							{
								Buffer.Write("<{{W|2}} to scroll down>");
							}
						}
						if (itemsSkippedByFilter > 0)
						{
							Buffer.Goto(3, 23);
							Buffer.Write(itemsSkippedByFilter + " items hidden by filter");
						}
						if (ControlManager.activeControllerType == ControlManager.InputDeviceType.Keyboard)
						{
							Buffer.Goto(20, 24);
							Buffer.Write("[{{W|?}} view quick keys]");
						}
						TextConsole.DrawBuffer(Buffer);
						if (!XRLCore.Core.Game.Running)
						{
							GameManager.Instance.PopGameView();
							return ScreenReturn.Exit;
						}
						IEvent GeneratedEvent = null;
						keys = Keyboard.getvk(Options.MapDirectionsToKeypad, pumpActions: true);
						ScreenBuffer.ClearImposterSuppression();
						bool InterfaceExitRequested = false;
						char c = ("" + (char)Keyboard.Char + " ").ToLower()[0];
						if (keys == Keys.Enter)
						{
							keys = Keys.Space;
						}
						if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick")
						{
							Done = true;
						}
						if (keys == Keys.OemQuestion || c == '?')
						{
							Popup.Show("Inventory quick keys\r\n\r\n&WCtrl+D&y - Drop\r\n&WCtrl+A&y - Eat\r\n&WCtrl+E&y - Auto-equip\r\n&WCtrl+R&y - Drink\r\n&WCtrl+P&y - Apply\n&WCtrl+F or ,&y - Filter");
						}
						else if (keys == (Keys.A | Keys.Control))
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Eat", false, false, false, false, false, 0, 0, 0, (GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == (Keys.D | Keys.Control))
						{
							if (gameObject != null)
							{
								Zone currentZone = GO.CurrentZone;
								if (currentZone == null || !currentZone.IsWorldMap())
								{
									InventoryActionEvent.Check(ref InterfaceExitRequested, GO, GO, gameObject, "CommandDropObject");
									ResetNameCache(GO);
								}
							}
						}
						else if (keys == (Keys.F | Keys.Control) || c == ',')
						{
							filterString = Popup.AskString("Enter text to filter inventory by item name.", filterString);
							ClearLists();
						}
						else if (keys == (Keys.R | Keys.Control))
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Drink", false, false, false, false, false, 0, 0, 0, (GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == (Keys.P | Keys.Control))
						{
							if (gameObject != null)
							{
								InventoryActionEvent.Check(out GeneratedEvent, gameObject, GO, gameObject, "Apply", false, false, false, false, false, 0, 0, 0, (GameObject)null, (Cell)null, (Cell)null, (IInventory)null);
								ResetNameCache(GO);
							}
						}
						else if (keys == Keys.NumPad7 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
						{
							Done = true;
						}
						else if (keys == Keys.Escape || keys == Keys.NumPad5 || (keys == Keys.NumPad9 && Keyboard.RawCode != Keys.Prior && Keyboard.RawCode != Keys.Next))
						{
							Done = true;
						}
						else if (keys == Keys.NumPad8)
						{
							if (Selected > 0)
							{
								Selected--;
								continue;
							}
							if (StartObject > 0)
							{
								StartObject--;
							}
						}
						else if (keys == Keys.NumPad2)
						{
							if (Selected < num - 1)
							{
								Selected++;
								continue;
							}
							if (bMore)
							{
								StartObject++;
							}
						}
						else if (keys == Keys.Next || keys == Keys.Next || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next)
						{
							if (Selected < num - 1)
							{
								Selected = num - 1;
							}
							else if (bMore)
							{
								StartObject += 21;
							}
						}
						else if (keys == Keys.Prior || keys == Keys.Back || Keyboard.RawCode == Keys.Prior || Keyboard.RawCode == Keys.Back)
						{
							if (Selected > 0)
							{
								Selected = 0;
							}
							else
							{
								StartObject -= 21;
								if (StartObject < 0)
								{
									StartObject = 0;
								}
							}
						}
						else if (keys == Keys.Subtract || keys == Keys.OemMinus || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Negative"))
						{
							foreach (InventoryCategory value2 in CategoryList.Values)
							{
								value2.Expanded = false;
							}
						}
						else if (keys == Keys.Add || keys == Keys.Oemplus || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive"))
						{
							foreach (InventoryCategory value3 in CategoryList.Values)
							{
								value3.Expanded = true;
							}
						}
						else
						{
							if (gameObject != null)
							{
								if ((Keyboard.vkCode == Keys.Right || keys == Keys.NumPad6 || keys == (Keys.E | Keys.Control)) && Options.GetOption("OptionPressingRightInInventoryEquips", "Yes") == "Yes" && GO.AutoEquip(gameObject))
								{
									ResetNameCache(GO);
								}
								if (Keyboard.vkCode == Keys.Left || keys == Keys.NumPad4 || keys == (Keys.E | Keys.Control))
								{
									foreach (KeyValuePair<string, List<GameObject>> item in CategoryMap)
									{
										if (item.Value.Contains(gameObject))
										{
											CategoryList[item.Key].Expanded = false;
											forceCategorySelect = CategoryList[item.Key];
											break;
										}
									}
								}
								if (keys == Keys.Space)
								{
									EquipmentAPI.TwiddleObject(GO, gameObject, ref Done);
									ResetNameCache(GO);
								}
								if (keys == Keys.Tab)
								{
									InventoryActionEvent.Check(gameObject, GO, gameObject, "Look");
									ResetNameCache(GO);
								}
							}
							if (inventoryCategory != null)
							{
								if (keys == Keys.NumPad4)
								{
									inventoryCategory.Expanded = false;
								}
								if (keys == Keys.NumPad6)
								{
									inventoryCategory.Expanded = true;
								}
								if (keys == Keys.Space)
								{
									inventoryCategory.Expanded = !inventoryCategory.Expanded;
								}
							}
							if (keys >= Keys.A && keys <= Keys.Z && CategorySelectionList.ContainsKey(c))
							{
								if (Selected == dictionary[c] && (!CategorySelectionList.ContainsKey(c) || CategorySelectionList[c].Category == null))
								{
									EquipmentAPI.TwiddleObject(GO, gameObject, ref Done);
									ResetNameCache(GO);
								}
								else
								{
									Selected = dictionary[c];
									if (CategorySelectionList.ContainsKey(c) && CategorySelectionList[c].Category != null)
									{
										CategorySelectionList[c].Category.Expanded = !CategorySelectionList[c].Category.Expanded;
									}
								}
							}
						}
						if (!Done && (InterfaceExitRequested || (GeneratedEvent != null && GeneratedEvent.InterfaceExitRequested())))
						{
							Done = true;
						}
						goto end_IL_00c4;
					}
					goto IL_079c;
				}
				StartObject = 0;
				continue;
				IL_079c:
				Selected = num - 1;
				continue;
				end_IL_00c4:
				break;
			}
		}
		ClearLists();
		if (keys == Keys.NumPad7 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Left"))
		{
			GameManager.Instance.PopGameView();
			return ScreenReturn.Previous;
		}
		if (keys == Keys.NumPad9 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Page Right"))
		{
			GameManager.Instance.PopGameView();
			return ScreenReturn.Next;
		}
		GameManager.Instance.PopGameView();
		return ScreenReturn.Exit;
	}
}

using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Qud.API;
using Qud.UI;
using XRL.Core;
using XRL.World;
using XRL.World.Capabilities;

namespace XRL.UI;

public class PickItem
{
	public class SortGOCategory : Comparer<GameObject>
	{
		private PickItem Picker;

		private List<GameObject> Items;

		private bool PreserveOrder;

		public SortGOCategory(PickItem Picker, List<GameObject> Items, bool PreserveOrder)
		{
			this.Picker = Picker;
			this.Items = Items;
			this.PreserveOrder = PreserveOrder;
		}

		public override int Compare(GameObject x, GameObject y)
		{
			return x.SortVs(y, Picker.ObjectCategories, UseCategory: true, !PreserveOrder, !PreserveOrder, UseRenderLayer: false, Items);
		}
	}

	public enum PickItemDialogStyle
	{
		SelectItemDialog,
		StoreItemDialog,
		GetItemDialog
	}

	private Dictionary<char, GameObject> SelectionList = new Dictionary<char, GameObject>();

	private Dictionary<char, CategorySelectionListEntry> CategorySelectionList = new Dictionary<char, CategorySelectionListEntry>();

	private Dictionary<string, List<GameObject>> CategoryMap = new Dictionary<string, List<GameObject>>();

	private Dictionary<string, bool> ExpandState = new Dictionary<string, bool>();

	private Dictionary<string, InventoryCategory> CategoryList = new Dictionary<string, InventoryCategory>();

	private List<GameObject> SortList = new List<GameObject>();

	private List<string> Categories = new List<string>();

	private List<string> CatNames = new List<string>();

	private int StartObject;

	private bool bMore;

	private Dictionary<GameObject, string> ObjectCategories = new Dictionary<GameObject, string>();

	private int LastSelected = -1;

	public void ClearLists()
	{
		if (CategoryMap == null)
		{
			return;
		}
		ExpandState = new Dictionary<string, bool>(CategoryMap.Count);
		foreach (string key in CategoryMap.Keys)
		{
			ExpandState.Add(CategoryList[key].Name, value: true);
		}
		CategoryMap.Clear();
		SelectionList.Clear();
		CategorySelectionList.Clear();
		CategoryList.Clear();
		SortList.Clear();
	}

	public string GetObjectCategory(GameObject go)
	{
		if (ObjectCategories.TryGetValue(go, out var value))
		{
			return value;
		}
		value = go.GetInventoryCategory();
		ObjectCategories.Add(go, value);
		return value;
	}

	public int RebuildLists(IList<GameObject> Items, string CategoryPrioritySpec, bool PreserveOrder = false)
	{
		SortGOCategory comparer = new SortGOCategory(this, new List<GameObject>(Items), PreserveOrder);
		int num = 0;
		int num2 = 18;
		List<string> list = null;
		if (CategoryPrioritySpec != null)
		{
			list = new List<string>(CategoryPrioritySpec.CachedCommaExpansion());
		}
		CategoryMap.Clear();
		SelectionList.Clear();
		if (!Categories.Contains("Category"))
		{
			Categories.Add("Category");
		}
		int i = 0;
		for (int count = Items.Count; i < count; i++)
		{
			GameObject gameObject = Items[i];
			gameObject.Seen();
			string objectCategory = GetObjectCategory(gameObject);
			if (!CategoryList.ContainsKey(objectCategory))
			{
				InventoryCategory inventoryCategory = new InventoryCategory(objectCategory);
				if (CategoryPrioritySpec != null)
				{
					inventoryCategory.Expanded = list.Contains(objectCategory);
				}
				CategoryList.Add(objectCategory, inventoryCategory);
				Categories.Add(objectCategory);
			}
			if (!CategoryMap.ContainsKey(objectCategory))
			{
				CategoryMap.Add(objectCategory, new List<GameObject>());
			}
			CategoryMap[objectCategory].Add(gameObject);
		}
		if (Categories[0] == "Category")
		{
			SortList.Clear();
			SortList.AddRange(Items);
			SortList.Sort(comparer);
		}
		else if (CategoryMap.ContainsKey(Categories[0]))
		{
			SortList.Clear();
			SortList.AddRange(CategoryMap[Categories[0]]);
		}
		int num3 = 0;
		bMore = false;
		if (Categories[0] == "Category")
		{
			CategorySelectionList.Clear();
			int num4 = 0;
			char c = 'a';
			CatNames.Clear();
			foreach (string key in CategoryList.Keys)
			{
				CatNames.Add(key);
			}
			CatNames.Sort();
			foreach (string catName in CatNames)
			{
				InventoryCategory inventoryCategory2 = CategoryList[catName];
				if (num4 >= StartObject && num4 <= num2 + StartObject)
				{
					CategorySelectionList.Add(c, new CategorySelectionListEntry(inventoryCategory2));
					c = (char)(c + 1);
					num3++;
				}
				if (inventoryCategory2.Expanded && CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item in CategoryMap[inventoryCategory2.Name])
					{
						num4++;
						num3++;
						if (num4 >= StartObject && num4 <= num2 + StartObject)
						{
							if (num == 0)
							{
								num = num3 - 1;
							}
							CategorySelectionList.Add(c, new CategorySelectionListEntry(item));
							c = (char)(c + 1);
						}
						else if (num4 > num2 + StartObject)
						{
							bMore = true;
							goto end_IL_0424;
						}
					}
				}
				if (CategoryList.ContainsKey(catName))
				{
					CategoryList[catName].Weight = 0;
				}
				CategoryList[catName].Items = 0;
				if (CategoryMap.ContainsKey(inventoryCategory2.Name))
				{
					foreach (GameObject item2 in CategoryMap[inventoryCategory2.Name])
					{
						CategoryList[catName].Weight += item2.Weight;
						CategoryList[catName].Items++;
					}
				}
				if (num4 > num2 + StartObject)
				{
					bMore = true;
					break;
				}
				num4++;
				continue;
				end_IL_0424:
				break;
			}
		}
		else
		{
			int num5 = 0;
			char c2 = 'a';
			foreach (GameObject sort in SortList)
			{
				if (num5 >= StartObject && num5 <= num2 + StartObject)
				{
					SelectionList.Add(c2, sort);
					c2 = (char)(c2 + 1);
				}
				num5++;
				if (num5 > num2 + StartObject)
				{
					bMore = true;
					break;
				}
			}
		}
		List<string> list2 = new List<string>();
		foreach (string key2 in CategoryList.Keys)
		{
			if (!CategoryMap.ContainsKey(key2))
			{
				list2.Add(key2);
			}
			else if (CategoryMap[key2].Count == 0)
			{
				list2.Add(key2);
			}
		}
		foreach (string item3 in list2)
		{
			if (CategoryList.ContainsKey(item3))
			{
				CategoryList.Remove(item3);
			}
			if (CategoryMap.ContainsKey(item3))
			{
				CategoryMap.Remove(item3);
			}
		}
		return num;
	}

	public static GameObject ShowPicker(IList<GameObject> Items, ref bool RequestInterfaceExit, string CategoryPriority = null, PickItemDialogStyle Style = PickItemDialogStyle.SelectItemDialog, GameObject Actor = null, GameObject Container = null, Cell Cell = null, string Title = null, bool PreserveOrder = false, Func<List<GameObject>> Regenerate = null, bool ShowContext = false, bool ShowIcons = true, bool NotePlayerOwned = false)
	{
		return new PickItem().ShowPickerInternal(Items, ref RequestInterfaceExit, CategoryPriority, Style, Actor, Container, Cell, KeepSel: false, Title, PreserveOrder, 42, 78, Regenerate, ShowContext, ShowIcons, NotePlayerOwned);
	}

	public static GameObject ShowPicker(IList<GameObject> Items, string CategoryPriority = null, PickItemDialogStyle Style = PickItemDialogStyle.SelectItemDialog, GameObject Actor = null, GameObject Container = null, Cell Cell = null, string Title = null, bool PreserveOrder = false, Func<List<GameObject>> Regenerate = null, bool ShowContext = false, bool ShowIcons = true, bool NotePlayerOwned = false)
	{
		bool RequestInterfaceExit = false;
		return new PickItem().ShowPickerInternal(Items, ref RequestInterfaceExit, CategoryPriority, Style, Actor, Container, Cell, KeepSel: false, Title, PreserveOrder, 42, 78, Regenerate, ShowContext, ShowIcons, NotePlayerOwned);
	}

	public GameObject ShowPickerInternal(IList<GameObject> Items, ref bool RequestInterfaceExit, string CategoryPriority = null, PickItemDialogStyle Style = PickItemDialogStyle.SelectItemDialog, GameObject Actor = null, GameObject Container = null, Cell Cell = null, bool KeepSel = false, string Title = null, bool PreserveOrder = false, int MinWidth = 42, int MaxWidth = 78, Func<List<GameObject>> Regenerate = null, bool ShowContext = false, bool ShowIcons = true, bool NotePlayerOwned = false)
	{
		string text = Title ?? ((Style == PickItemDialogStyle.StoreItemDialog) ? "{{W|Select an item to store}}" : "{{W|Select an item}}");
		if (Options.ModernUI)
		{
			bool reentry = false;
			while (true)
			{
				if (Items == null || Items.Count == 0)
				{
					return null;
				}
				switch (Style)
				{
				case PickItemDialogStyle.GetItemDialog:
				{
					PickGameObjectScreen.Result result3 = PickGameObjectScreen.show(Items, text, Style, Actor, Container, Cell, PreserveOrder, CategoryPriority, ShowContext, NotePlayerOwned, reentry).Result;
					reentry = true;
					GameObject item3 = result3.item;
					RequestInterfaceExit = result3.requestInterfaceExit;
					if (result3.wantToStore && Actor.Inventory != null)
					{
						TradeUI.ShowTradeScreen(Container, 0f, TradeUI.TradeScreenMode.Container);
						return null;
					}
					if (item3 == null)
					{
						return null;
					}
					EquipmentAPI.TwiddleObject(Actor, item3, ref RequestInterfaceExit);
					if (!RequestInterfaceExit)
					{
						goto IL_00a5;
					}
					return null;
				}
				case PickItemDialogStyle.StoreItemDialog:
				{
					PickGameObjectScreen.Result result2 = PickGameObjectScreen.show(Items, text, Style, Actor, Container, Cell, PreserveOrder, CategoryPriority, ShowContext, NotePlayerOwned, reentry).Result;
					reentry = true;
					GameObject item2 = result2.item;
					RequestInterfaceExit = result2.requestInterfaceExit;
					if (item2 == null)
					{
						return null;
					}
					return item2;
				}
				case PickItemDialogStyle.SelectItemDialog:
				{
					PickGameObjectScreen.Result result = PickGameObjectScreen.show(Items, text, Style, Actor, Container, Cell, PreserveOrder, CategoryPriority, ShowContext, NotePlayerOwned, reentry).Result;
					reentry = true;
					GameObject item = result.item;
					RequestInterfaceExit = result.requestInterfaceExit;
					if (item == null)
					{
						return null;
					}
					return item;
				}
				}
				break;
				IL_00a5:
				Items = Regenerate();
			}
		}
		StringBuilder stringBuilder = new StringBuilder();
		GameManager.Instance.PushGameView("Popup:Item");
		TextConsole.LoadScrapBuffers();
		ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
		ScreenBuffer scrapBuffer2 = TextConsole.ScrapBuffer2;
		StringBuilder stringBuilder2 = new StringBuilder(256);
		Keys keys = Keys.None;
		bool flag = false;
		GameObject result4 = null;
		StartObject = 0;
		int num = -1;
		if (KeepSel)
		{
			num = LastSelected;
		}
		Dictionary<char, int> dictionary = new Dictionary<char, int>();
		Event.PinCurrentPool();
		RebuildLists(Items, CategoryPriority, PreserveOrder);
		while (!flag)
		{
			int num8;
			InventoryCategory inventoryCategory;
			GameObject gameObject;
			while (true)
			{
				Event.ResetToPin();
				if (Items.Count == 0)
				{
					break;
				}
				int num2 = RebuildLists(Items, CategoryPriority, PreserveOrder);
				if (num == -1)
				{
					num = ((!KeepSel || LastSelected == -1) ? num2 : (LastSelected - 1));
				}
				LastSelected = num;
				CategoryPriority = null;
				scrapBuffer.Copy(scrapBuffer2);
				int num3 = MinWidth;
				int num4 = ColorUtility.LengthExceptFormatting("[ " + text + " ]") + 6;
				if (num4 > num3)
				{
					num3 = Math.Min(num4, MaxWidth);
				}
				if (num3 < MaxWidth)
				{
					foreach (List<GameObject> value2 in CategoryMap.Values)
					{
						foreach (GameObject item4 in value2)
						{
							int num5 = ColorUtility.LengthExceptFormatting(item4.DisplayName) + 11;
							if (NotePlayerOwned && item4.OwnedByPlayer)
							{
								num5 += 15;
							}
							if (ShowIcons)
							{
								num5 += 2;
							}
							if (ShowContext)
							{
								string listDisplayContext = item4.GetListDisplayContext(Actor);
								if (!string.IsNullOrEmpty(listDisplayContext))
								{
									num5 += ColorUtility.LengthExceptFormatting(listDisplayContext) + 3;
								}
							}
							if (num5 >= MaxWidth)
							{
								num3 = MaxWidth;
								break;
							}
							if (item4.IsTakeable())
							{
								num5 += item4.Weight.ToString().Length + 2;
								if (num5 >= MaxWidth)
								{
									num3 = MaxWidth;
									break;
								}
							}
							if (num5 > num3)
							{
								num3 = num5;
							}
						}
						if (num3 >= MaxWidth)
						{
							break;
						}
					}
				}
				if (num3 % 2 == 1)
				{
					num3++;
					if (num3 > MaxWidth)
					{
						num3 = MaxWidth - 2;
					}
				}
				int num6 = (80 - num3) / 2;
				int num7 = num3 - 4;
				scrapBuffer.Fill(num6, 2, 79 - num6, 22, 32, ColorUtility.MakeColor(TextColor.Black, TextColor.Black));
				scrapBuffer.SingleBox(num6, 2, 79 - num6, 22, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));
				scrapBuffer.Goto(num6 + 5, 2);
				scrapBuffer.Write("[ " + text + " ]");
				if (Style == PickItemDialogStyle.GetItemDialog)
				{
					scrapBuffer.Goto(20, 22);
					scrapBuffer.Write("[{{W|" + ControlManager.getCommandInputDescription("Take All", mapGlyphs: false) + "}}] {{Y|Take all}}");
					if (Actor != Container && Actor != null && Container != null)
					{
						scrapBuffer.Goto(37, 22);
						scrapBuffer.Write("[" + ControlManager.getCommandInputFormatted("Store Items", mapGlyphs: false) + "] {{Y|Store an item}}");
					}
				}
				if (StartObject > 0)
				{
					scrapBuffer.Goto(42, 2);
					scrapBuffer.Write("<{{W|8}} to scroll up>");
				}
				num8 = 0;
				dictionary.Clear();
				inventoryCategory = null;
				gameObject = null;
				stringBuilder.Length = 0;
				int num9 = 3;
				int num10 = num6 + 2;
				foreach (char key2 in CategorySelectionList.Keys)
				{
					if (CategorySelectionList[key2].Category != null)
					{
						scrapBuffer.Goto(num10, num9 + num8);
						string text2 = "";
						if (num8 == num)
						{
							text2 = "{{Y|>}}";
							inventoryCategory = CategorySelectionList[key2].Category;
						}
						else
						{
							text2 = " ";
						}
						if (CategorySelectionList[key2].Category.Expanded)
						{
							if (num8 == num)
							{
								scrapBuffer.Write(text2 + key2 + ") [-] {{K|[{{Y|" + CategorySelectionList[key2].Category.Name + "}}]}}");
							}
							else
							{
								scrapBuffer.Write(text2 + key2 + ") [-] {{K|[{{|" + CategorySelectionList[key2].Category.Name + "}}]}}");
							}
						}
						else if (num8 == num)
						{
							scrapBuffer.Write(text2 + key2 + ") [+] {{K|[{{Y|" + CategorySelectionList[key2].Category.Name + "}}]}}");
						}
						else
						{
							scrapBuffer.Write(text2 + key2 + ") [+] {{K|[{{|" + CategorySelectionList[key2].Category.Name + "}}]}}");
						}
						dictionary.Add(key2, num8);
						num8++;
						continue;
					}
					int num11 = 6;
					string text3;
					char value;
					if (num8 == num)
					{
						text3 = "{{Y|>}}  ";
						value = 'y';
						gameObject = CategorySelectionList[key2].Object;
					}
					else
					{
						text3 = "   ";
						value = 'K';
					}
					scrapBuffer.Goto(num10, num9 + num8);
					scrapBuffer.Write(text3 + key2 + ") ");
					GameObject gameObject2 = CategorySelectionList[key2].Object;
					if (ShowIcons)
					{
						IRenderable renderable = gameObject2.RenderForUI();
						if (renderable != null)
						{
							scrapBuffer.Write(renderable);
							num11 += 2;
						}
					}
					stringBuilder.Clear().Append(gameObject2.DisplayName);
					if (NotePlayerOwned && gameObject2.OwnedByPlayer)
					{
						stringBuilder.Append(" {{G|[owned by you]}}");
					}
					if (ShowContext)
					{
						string listDisplayContext2 = gameObject2.GetListDisplayContext(Actor);
						if (!string.IsNullOrEmpty(listDisplayContext2))
						{
							stringBuilder.Append(" [").Append(listDisplayContext2).Append(']');
						}
					}
					StringBuilder stringBuilder3 = Event.NewStringBuilder();
					if (gameObject2.IsTakeable())
					{
						stringBuilder3.Length = 0;
						stringBuilder3.Append(" {{").Append(value).Append('|')
							.Append(gameObject2.Weight)
							.Append("#}}");
						Markup.Transform(stringBuilder3);
						int num12 = ColorUtility.LengthExceptFormatting(stringBuilder3);
						StringFormat.ClipLine(stringBuilder, num7 - num12 - num11, AddEllipsis: true, stringBuilder2);
						scrapBuffer.Goto(num10 + num11, num9 + num8);
						scrapBuffer.Write(stringBuilder2);
						scrapBuffer.Goto(num10 + num7 - num12, num9 + num8);
						scrapBuffer.Write(stringBuilder3);
					}
					else
					{
						scrapBuffer.Goto(num10 + num11, num9 + num8);
						StringFormat.ClipLine(stringBuilder, num7, AddEllipsis: true, stringBuilder2);
						scrapBuffer.Write(stringBuilder2);
					}
					dictionary.Add(key2, num8);
					num8++;
				}
				if (num8 == 0 && StartObject != 0)
				{
					StartObject = 0;
					continue;
				}
				if (num >= num8)
				{
					num = num8 - 1;
					continue;
				}
				goto IL_0897;
			}
			break;
			IL_0897:
			if (bMore)
			{
				scrapBuffer.Goto(42, 22);
				scrapBuffer.Write("<{{W|2}} to scroll down>");
			}
			if (Style == PickItemDialogStyle.GetItemDialog)
			{
				stringBuilder.Length = 0;
				stringBuilder.Append("Total weight: {{Y|").Append(EquipmentAPI.GetPlayerCurrentCarryWeight()).Append(" {{y|/}}  ")
					.Append(EquipmentAPI.GetPlayerMaxCarryWeight())
					.Append(" lbs.}}");
				scrapBuffer.Goto(79 - ColorUtility.LengthExceptFormatting(stringBuilder), 24);
				scrapBuffer.Write(stringBuilder.ToString());
			}
			Popup._TextConsole.DrawBuffer(scrapBuffer);
			if (!XRLCore.Core.Game.Running)
			{
				return null;
			}
			keys = Keyboard.getvk(Options.MapDirectionsToKeypad);
			if (ShowIcons)
			{
				ScreenBuffer.ClearImposterSuppression();
			}
			char key = ("" + (char)Keyboard.Char + " ").ToLower()[0];
			if (keys == Keys.Enter)
			{
				keys = Keys.Space;
			}
			if (keys == Keys.Escape || keys == Keys.NumPad5 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "RightClick"))
			{
				break;
			}
			if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Store Items" && Actor != Container && Actor != null && Container != null && Actor.Inventory != null)
			{
				TradeUI.ShowTradeScreen(Container, 0f, TradeUI.TradeScreenMode.Container);
				break;
			}
			if (keys == Keys.NumPad8)
			{
				if (num > 0)
				{
					num--;
				}
				else if (StartObject > 0)
				{
					StartObject--;
				}
			}
			else if (keys == Keys.NumPad2)
			{
				if (num < num8 - 1)
				{
					num++;
				}
				else if (bMore)
				{
					StartObject++;
				}
			}
			else if (keys == Keys.Next || keys == Keys.Next || keys == Keys.NumPad3 || Keyboard.RawCode == Keys.Next || Keyboard.RawCode == Keys.Next || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Page Down"))
			{
				if (num < num8 - 1)
				{
					num = num8 - 1;
				}
				else if (bMore)
				{
					StartObject += 18;
				}
			}
			else if (keys == Keys.Prior || keys == Keys.Back || keys == Keys.NumPad9 || Keyboard.RawCode == Keys.Prior || Keyboard.RawCode == Keys.Back || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Page Up"))
			{
				if (num > 0)
				{
					num = 0;
				}
				else
				{
					StartObject -= 18;
					if (StartObject < 0)
					{
						StartObject = 0;
					}
				}
			}
			else if (keys == Keys.Subtract || keys == Keys.OemMinus || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Negative"))
			{
				foreach (InventoryCategory value3 in CategoryList.Values)
				{
					value3.Expanded = false;
				}
			}
			else if (keys == Keys.Add || keys == Keys.Oemplus || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:V Positive"))
			{
				foreach (InventoryCategory value4 in CategoryList.Values)
				{
					value4.Expanded = true;
				}
			}
			else
			{
				if (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "ItemSelected")
				{
					if (Style == PickItemDialogStyle.SelectItemDialog || Style == PickItemDialogStyle.StoreItemDialog)
					{
						result4 = Keyboard.CurrentMouseEvent.Data as GameObject;
						break;
					}
					gameObject = Keyboard.CurrentMouseEvent.Data as GameObject;
					keys = Keys.Space;
				}
				if (Style == PickItemDialogStyle.SelectItemDialog || Style == PickItemDialogStyle.StoreItemDialog)
				{
					if (keys >= Keys.A && keys <= Keys.Z)
					{
						if (CategorySelectionList.ContainsKey(key) && CategorySelectionList[key].Object != null)
						{
							result4 = CategorySelectionList[key].Object;
							break;
						}
					}
					else if (gameObject != null && (keys == Keys.Space || keys == Keys.Enter))
					{
						result4 = gameObject;
						break;
					}
				}
				else if (Style == PickItemDialogStyle.GetItemDialog)
				{
					if (gameObject != null && (keys == Keys.Space || keys == Keys.Enter))
					{
						Cell currentCell = gameObject.CurrentCell;
						GameObject equipped = gameObject.Equipped;
						GameObject inInventory = gameObject.InInventory;
						EquipmentAPI.TwiddleObject(Actor, gameObject, ref RequestInterfaceExit);
						if (RequestInterfaceExit)
						{
							break;
						}
						if (Regenerate != null)
						{
							Items = Regenerate();
						}
						else if (currentCell != gameObject.CurrentCell || equipped != gameObject.Equipped || inInventory != gameObject.InInventory)
						{
							Items.Remove(gameObject);
						}
						if (Items.Count == 0)
						{
							break;
						}
					}
					else if (keys >= Keys.A && keys <= Keys.Z)
					{
						if (CategorySelectionList.ContainsKey(key) && CategorySelectionList[key].Object != null)
						{
							Cell currentCell2 = CategorySelectionList[key].Object.CurrentCell;
							GameObject equipped2 = CategorySelectionList[key].Object.Equipped;
							GameObject inInventory2 = CategorySelectionList[key].Object.InInventory;
							EquipmentAPI.TwiddleObject(Actor, CategorySelectionList[key].Object, ref RequestInterfaceExit);
							if (RequestInterfaceExit)
							{
								break;
							}
							if (Regenerate != null)
							{
								Items = Regenerate();
							}
							else if (currentCell2 != CategorySelectionList[key].Object.CurrentCell || equipped2 != CategorySelectionList[key].Object.Equipped || inInventory2 != CategorySelectionList[key].Object.InInventory)
							{
								Items.Remove(CategorySelectionList[key].Object);
							}
							if (Items.Count == 0)
							{
								break;
							}
						}
					}
					else if ((keys == Keys.Tab || keys == Keys.F1 || (keys == Keys.MouseEvent && Keyboard.CurrentMouseEvent.Event == "Command:Take All")) && TakeAll(Actor, Container, Cell, Items, ref RequestInterfaceExit))
					{
						GameManager.Instance.PopGameView(bHard: true);
						return null;
					}
				}
				if (inventoryCategory != null)
				{
					switch (keys)
					{
					case Keys.NumPad4:
						inventoryCategory.Expanded = false;
						break;
					case Keys.NumPad6:
						inventoryCategory.Expanded = true;
						break;
					case Keys.Enter:
					case Keys.Space:
						inventoryCategory.Expanded = !inventoryCategory.Expanded;
						break;
					}
				}
				if (keys >= Keys.A && keys <= Keys.Z && CategorySelectionList.ContainsKey(key))
				{
					num = dictionary[key];
					if (CategorySelectionList.ContainsKey(key) && CategorySelectionList[key].Category != null)
					{
						CategorySelectionList[key].Category.Expanded = !CategorySelectionList[key].Category.Expanded;
					}
				}
			}
			RebuildLists(Items, CategoryPriority, PreserveOrder);
		}
		ClearLists();
		GameManager.Instance.PopGameView(bHard: true);
		return result4;
	}

	public static bool TakeAll(GameObject Actor, GameObject Container, Cell Cell, IList<GameObject> Items, ref bool RequestInterfaceExit)
	{
		if (Actor != null && Actor.IsPlayer() && (Container == null || Container.GetObjectContext() != Actor))
		{
			int num = 0;
			int num2 = 0;
			foreach (GameObject Item in Items)
			{
				if (Item.ShouldTakeAll())
				{
					num += Item.Weight;
					num2 += Item.Count;
				}
			}
			int maxCarriedWeight = Actor.GetMaxCarriedWeight();
			int carriedWeight = Actor.GetCarriedWeight();
			if (carriedWeight <= maxCarriedWeight && carriedWeight + num > maxCarriedWeight && Popup.ShowYesNo("Taking " + ((num2 > 2) ? "all these objects" : ((num2 == 2) ? "these objects" : "this object")) + " will put you over your weight limit. Are you sure you want to do it?") != DialogResult.Yes)
			{
				return false;
			}
		}
		if (Container != null && Actor.IsPlayer() && Container.InSameOrAdjacentCellTo(Actor))
		{
			if (AutoAct.ShouldHostilesInterrupt("g", null, logSpot: false, popSpot: true))
			{
				return false;
			}
			AutoAct.ResumeSetting = AutoAct.Setting;
			AutoAct.Setting = "go" + Container.ID;
			The.ActionManager.SkipPlayerTurn = true;
			RequestInterfaceExit = true;
		}
		else if (Cell != null && Actor.IsPlayer() && Cell.IsSameOrAdjacent(Actor.CurrentCell))
		{
			if (AutoAct.ShouldHostilesInterrupt("g", null, logSpot: false, popSpot: true))
			{
				return false;
			}
			AutoAct.ResumeSetting = AutoAct.Setting;
			AutoAct.Setting = "gd" + Actor.CurrentCell.GetDirectionFromCell(Cell);
			The.ActionManager.SkipPlayerTurn = true;
			RequestInterfaceExit = true;
		}
		else
		{
			foreach (GameObject Item2 in Items)
			{
				if (Item2.ShouldTakeAll())
				{
					Actor.TakeObject(Item2, NoStack: false, Silent: false, null, "TakeAll");
				}
			}
		}
		return true;
	}
}

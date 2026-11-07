using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Genkit;
using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.Anatomy;

namespace Qud.API;

public static class EquipmentAPI
{
	private static Dictionary<string, InventoryAction> CheckActionTable = new Dictionary<string, InventoryAction>(16);

	public static void TwiddleObject(GameObject GO, Action After = null, bool Distant = false, bool TelekineticOnly = false)
	{
		bool Done = false;
		TwiddleObject(The.Player, GO, ref Done, Distant, TelekineticOnly);
		After?.Invoke();
	}

	public static void TwiddleObject(GameObject GO, ref bool Done, Action After = null, bool Distant = false, bool TelekineticOnly = false, bool MouseClick = false)
	{
		TwiddleObject(The.Player, GO, ref Done, Distant, TelekineticOnly, MouseClick);
		After?.Invoke();
	}

	public static InventoryAction ShowInventoryActionMenu(Dictionary<string, InventoryAction> ActionTable, GameObject Owner = null, GameObject GO = null, bool Distant = false, bool TelekineticOnly = false, string Intro = null, IComparer<InventoryAction> Comparer = null, bool MouseClick = false)
	{
		List<InventoryAction> list = new List<InventoryAction>();
		foreach (KeyValuePair<string, InventoryAction> item in ActionTable)
		{
			InventoryAction value = item.Value;
			if ((!TelekineticOnly || value.WorksTelekinetically) && value.IsVisible(Owner, GO, Distant))
			{
				list.Add(value);
			}
		}
		list.Sort(Comparer ?? (Comparer = new InventoryAction.Comparer()));
		if (CapabilityManager.AllowKeyboardHotkeys)
		{
			Dictionary<char, InventoryAction> dictionary = new Dictionary<char, InventoryAction>(16);
			List<InventoryAction> list2 = null;
			StringBuilder SB = null;
			foreach (InventoryAction item2 in list)
			{
				if (item2.Key != ' ' && !ControlManager.isKeyMapped(item2.Key, new List<string> { "UINav", "Menus" }))
				{
					if (dictionary.ContainsKey(item2.Key))
					{
						if (list2 == null)
						{
							list2 = new List<InventoryAction>();
						}
						list2.Add(item2);
					}
					else
					{
						dictionary.Add(item2.Key, item2);
						item2.Display = ApplyHotkey(item2.Display, item2.Key, item2.PreferToHighlight, ref SB);
					}
				}
				else
				{
					item2.Key = ' ';
				}
			}
			if (list2 != null)
			{
				if (SB == null)
				{
					SB = Event.NewStringBuilder();
				}
				foreach (InventoryAction item3 in list2)
				{
					char c = char.ToUpper(item3.Key);
					if (c != item3.Key && !dictionary.ContainsKey(c))
					{
						if (!ControlManager.isKeyMapped(c, new List<string> { "UINav", "Menus" }))
						{
							item3.Key = c;
							dictionary.Add(c, item3);
							item3.Display = ApplyHotkey(ColorUtility.StripFormatting(item3.Display), c, item3.PreferToHighlight, ref SB);
						}
						continue;
					}
					string display = item3.Display;
					display = ColorUtility.StripFormatting(display);
					bool flag = false;
					SB.Clear();
					int i = 0;
					for (int length = display.Length; i < length; i++)
					{
						char c2 = display[i];
						if (!dictionary.ContainsKey(c2) && !ControlManager.isKeyMapped(c2, new List<string> { "UINav", "Menus" }))
						{
							item3.Key = c2;
							dictionary.Add(c2, item3);
							SB.Append("{{hotkey|").Append(c2).Append("}}")
								.Append(display, i + 1, length - i - 1);
							flag = true;
							break;
						}
						SB.Append(c2);
					}
					if (!flag)
					{
						item3.Key = ' ';
					}
					item3.Display = SB.ToString();
				}
				list.Sort(Comparer);
			}
		}
		List<string> list3 = new List<string>();
		List<char> list4 = new List<char>();
		foreach (InventoryAction item4 in list)
		{
			list3.Add(item4.Display);
			list4.Add(item4.Key);
		}
		int defaultSelected = 0;
		int num = int.MinValue;
		int j = 0;
		for (int count = list.Count; j < count; j++)
		{
			if (list[j].Default > num)
			{
				defaultSelected = j;
				num = list[j].Default;
			}
		}
		bool isConfused = The.Player.IsConfused;
		Location2D popupLocation = null;
		if (MouseClick)
		{
			popupLocation = GO?.CurrentCell?.Location;
		}
		int num2 = Popup.PickOption("", Intro ?? (isConfused ? GO.DisplayName : null), "", "Sounds/UI/ui_notification", list3.ToArray(), list4.ToArray(), null, null, isConfused ? null : GO, isConfused ? null : GO.RenderForUI(), null, 0, 60, defaultSelected, -1, AllowEscape: true, RespectOptionNewlines: true, CenterIntro: true, CenterIntroIcon: true, ForceNewPopup: false, popupLocation, "InventoryActionMenu:" + (GO?.IDIfAssigned ?? "(noid)"));
		if (num2 < 0)
		{
			return null;
		}
		return list[num2];
	}

	public static void TwiddleObject(GameObject Owner, GameObject GO, ref bool Done, bool Distant = false, bool TelekineticOnly = false, bool MouseClick = false)
	{
		TwiddleObject(Owner, GO, ref Done, out var _, Distant, TelekineticOnly, MouseClick);
	}

	public static void TwiddleObject(GameObject Owner, GameObject GO, ref bool Done, out InventoryAction ResultingAction, bool Distant = false, bool TelekineticOnly = false, bool MouseClick = false)
	{
		ResultingAction = null;
		try
		{
			if (Options.ModernUI)
			{
				GameManager.Instance.PushGameView("ModernPopupTwiddleObject");
			}
			ControlManager.ResetInput();
			Keyboard.ClearInput();
			if (!GameObject.Validate(GO))
			{
				return;
			}
			while (true)
			{
				Dictionary<string, InventoryAction> dictionary = new Dictionary<string, InventoryAction>(16);
				if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Owner.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(dictionary);
					GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
					GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
					Owner.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
				}
				GetInventoryActionsEvent.Send(Owner, GO, dictionary);
				GetInventoryActionsAlwaysEvent.Send(Owner, GO, dictionary);
				OwnerGetInventoryActionsEvent.Send(Owner, GO, dictionary);
				GameObject inInventory = GO.InInventory;
				GameObject equipped = GO.Equipped;
				Cell currentCell = GO.CurrentCell;
				InventoryAction inventoryAction = ShowInventoryActionMenu(dictionary, Owner, GO, Distant, TelekineticOnly, null, null, MouseClick);
				if (inventoryAction == null)
				{
					GO.CheckStack();
					break;
				}
				if (Options.ModernUI && inventoryAction != null && inventoryAction.ReturnToModernUI)
				{
					ResultingAction = inventoryAction;
					break;
				}
				if (!inventoryAction.IsUsable(Owner, GO, Distant, out var Telekinetic))
				{
					if (Telekinetic)
					{
						Popup.Show(GO.Does("are") + " out of your telekinetic range.");
					}
					else
					{
						Popup.Show("You cannot do that from here.");
					}
					continue;
				}
				IEvent obj = inventoryAction.Process(GO, Owner, Telekinetic);
				if (obj != null && obj.InterfaceExitRequested())
				{
					Done = true;
					break;
				}
				if (!GO.IsInvalid() && currentCell == GO.CurrentCell && inInventory == GO.InInventory && equipped == GO.Equipped)
				{
					continue;
				}
				break;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("TwiddleObject", x);
		}
		finally
		{
			if (Options.ModernUI)
			{
				GameManager.Instance.PopGameView();
			}
		}
	}

	private static string ApplyHotkey(string display, char key, string prefer, ref StringBuilder SB)
	{
		if (SB == null)
		{
			SB = Event.NewStringBuilder();
		}
		else
		{
			SB.Clear();
		}
		if (!display.Contains("{{") && !display.Contains("&"))
		{
			char c = char.ToLower(key);
			int num = -1;
			int num2 = -1;
			if (!string.IsNullOrEmpty(prefer))
			{
				num2 = display.IndexOf(prefer);
				if (num2 != -1)
				{
					num = prefer.IndexOf(key);
					if (num == -1 && c != key)
					{
						num = prefer.IndexOf(c);
					}
				}
			}
			if (num != -1)
			{
				int num3 = num + num2;
				display = SB.Append(display, 0, num3).Append("{{hotkey|").Append(key)
					.Append("}}")
					.Append(display, num3 + 1, display.Length - num3 - 1)
					.ToString();
			}
			else
			{
				int num4 = display.IndexOf(key);
				if (num4 != -1)
				{
					display = SB.Append(display, 0, num4).Append("{{hotkey|").Append(key)
						.Append("}}")
						.Append(display, num4 + 1, display.Length - num4 - 1)
						.ToString();
				}
				else if (c != key)
				{
					num4 = display.IndexOf(c);
					if (num4 != -1)
					{
						display = SB.Append(display, 0, num4).Append("{{hotkey|").Append(key)
							.Append("}}")
							.Append(display, num4 + 1, display.Length - num4 - 1)
							.ToString();
					}
				}
			}
		}
		return display;
	}

	private static bool GotUsableAction(GameObject GO, GameObject Actor, bool TelekineticOnly)
	{
		if (TelekineticOnly)
		{
			foreach (InventoryAction value in CheckActionTable.Values)
			{
				if (value.WorksTelekinetically && value.IsVisible(Actor, GO, Distant: true))
				{
					return true;
				}
			}
			return false;
		}
		return CheckActionTable.Count > 0;
	}

	public static bool CanBeTwiddled(GameObject GO, GameObject Actor, bool TelekineticOnly = false)
	{
		if (GO == null)
		{
			return false;
		}
		if (GO.HasTag("NoTwiddle"))
		{
			return false;
		}
		if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions") || GO.WantEvent(GetInventoryActionsEvent.ID, MinEvent.CascadeLevel) || GO.WantEvent(GetInventoryActionsAlwaysEvent.ID, MinEvent.CascadeLevel) || Actor.WantEvent(OwnerGetInventoryActionsEvent.ID, OwnerGetInventoryActionsEvent.CascadeLevel))
		{
			CheckActionTable.Clear();
			try
			{
				if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(CheckActionTable);
					if (GO.HasRegisteredEvent("GetInventoryActions"))
					{
						GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
					if (GO.HasRegisteredEvent("GetInventoryActionsAlways"))
					{
						GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
					if (Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
					{
						Actor.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
						if (GotUsableAction(GO, Actor, TelekineticOnly))
						{
							return true;
						}
					}
				}
				GetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
				GetInventoryActionsAlwaysEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
				OwnerGetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
				if (GotUsableAction(GO, Actor, TelekineticOnly))
				{
					return true;
				}
			}
			finally
			{
				CheckActionTable.Clear();
			}
		}
		return false;
	}

	public static List<InventoryAction> GetInventoryActions(GameObject GO, GameObject Actor, bool TelekineticOnly = false, bool TelekineticRequireUsable = true)
	{
		if (GO.HasTag("NoInventoryActions"))
		{
			return null;
		}
		List<InventoryAction> list = null;
		if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions") || GO.WantEvent(GetInventoryActionsEvent.ID, MinEvent.CascadeLevel) || GO.WantEvent(GetInventoryActionsAlwaysEvent.ID, MinEvent.CascadeLevel) || Actor.WantEvent(OwnerGetInventoryActionsEvent.ID, OwnerGetInventoryActionsEvent.CascadeLevel))
		{
			CheckActionTable.Clear();
			if (GO.HasRegisteredEvent("GetInventoryActions") || GO.HasRegisteredEvent("GetInventoryActionsAlways") || Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
			{
				EventParameterGetInventoryActions value = new EventParameterGetInventoryActions(CheckActionTable);
				if (GO.HasRegisteredEvent("GetInventoryActions"))
				{
					GO.FireEvent(Event.New("GetInventoryActions", "Actions", value));
				}
				if (GO.HasRegisteredEvent("GetInventoryActionsAlways"))
				{
					GO.FireEvent(Event.New("GetInventoryActionsAlways", "Actions", value));
				}
				if (Actor.HasRegisteredEvent("OwnerGetInventoryActions"))
				{
					Actor.FireEvent(Event.New("OwnerGetInventoryActions", "Actions", value, "Object", GO));
				}
			}
			GetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
			GetInventoryActionsAlwaysEvent.Send(Actor, GO, CheckActionTable);
			OwnerGetInventoryActionsEvent.Send(Actor, GO, CheckActionTable);
			if (CheckActionTable.Count > 0)
			{
				list = new List<InventoryAction>(CheckActionTable.Values);
				CheckActionTable.Clear();
				if (TelekineticOnly && list.Count > 0)
				{
					List<InventoryAction> list2 = new List<InventoryAction>();
					foreach (InventoryAction item in list)
					{
						if (!item.WorksTelekinetically)
						{
							continue;
						}
						if (TelekineticRequireUsable)
						{
							if (item.IsUsable(Actor, GO, Distant: true))
							{
								list2.Add(item);
							}
						}
						else if (item.IsVisible(Actor, GO, Distant: true))
						{
							list2.Add(item);
						}
					}
					list = list2;
				}
			}
		}
		return list;
	}

	public static bool UnequipObject(GameObject itemToUnequip)
	{
		GameObject equipped = itemToUnequip.Equipped;
		bool result = true;
		if (equipped != null)
		{
			result = InventoryActionEvent.Check(equipped, equipped, itemToUnequip, "Unequip");
		}
		if (itemToUnequip.Equipped == null && itemToUnequip.InInventory == null)
		{
			equipped.Inventory?.AddObject(itemToUnequip);
			result = true;
		}
		return result;
	}

	public static bool ForceUnequipObject(GameObject itemToUnequip, bool Silent = false)
	{
		GameObject equipped = itemToUnequip.Equipped;
		bool result = true;
		if (equipped != null)
		{
			Event obj = Event.New("CommandForceUnequipObject", "Object", itemToUnequip);
			obj.SetSilent(Silent);
			equipped.FireEvent(obj);
		}
		if (itemToUnequip.Equipped == null && itemToUnequip.InInventory == null)
		{
			equipped.Inventory?.AddObject(itemToUnequip, null, Silent);
			result = true;
		}
		return result;
	}

	public static void EquipObjectToPlayer(GameObject itemToEquip, BodyPart partToEquipOn)
	{
		EquipObject(The.Player, itemToEquip, partToEquipOn);
	}

	public static int GetPlayerCurrentCarryWeight()
	{
		return The.Player?.GetCarriedWeight() ?? 0;
	}

	public static int GetPlayerMaxCarryWeight()
	{
		return The.Player?.GetMaxCarriedWeight() ?? 0;
	}

	public static bool EquipObject(GameObject equippingObject, GameObject itemToEquip, BodyPart partToEquipOn)
	{
		if (itemToEquip == null)
		{
			return false;
		}
		if (itemToEquip.Equipped != null && !itemToEquip.EquippedOn().TryUnequip(Silent: false, SemiForced: false, NoStack: true))
		{
			return false;
		}
		return equippingObject.FireEvent(Event.New("CommandEquipObject", "Object", itemToEquip, "BodyPart", partToEquipOn, "Force", true));
	}

	public static void DropObject(GameObject objectToDrop)
	{
		GameObject inInventory = objectToDrop.InInventory;
		if (inInventory != null)
		{
			InventoryActionEvent.Check(inInventory, inInventory, objectToDrop, "CommandDropObject");
		}
	}
}

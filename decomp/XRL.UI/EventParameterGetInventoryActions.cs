using System.Collections.Generic;
using XRL.World;

namespace XRL.UI;

public class EventParameterGetInventoryActions
{
	public Dictionary<string, InventoryAction> Actions;

	public EventParameterGetInventoryActions()
	{
		Actions = new Dictionary<string, InventoryAction>();
	}

	public EventParameterGetInventoryActions(Dictionary<string, InventoryAction> Actions)
	{
		this.Actions = Actions ?? new Dictionary<string, InventoryAction>();
	}

	public bool AddAction(string Name, char Key, bool FireOnActor, string Display, string Command, string PreferToHighlight = null, int Default = 0, int Priority = 0, bool Override = false, bool WorksAtDistance = false, bool WorksTelekinetically = false, bool WorksTelepathically = false, bool AsMinEvent = false, GameObject FireOn = null)
	{
		if (!Override && Actions.ContainsKey(Name))
		{
			return false;
		}
		InventoryAction inventoryAction = new InventoryAction();
		inventoryAction.Name = Name;
		if (!ControlManager.isKeyMapped(Key, new List<string> { "UINav", "Menus" }))
		{
			inventoryAction.Key = Key;
		}
		inventoryAction.Display = Display;
		inventoryAction.Command = Command;
		inventoryAction.PreferToHighlight = PreferToHighlight;
		inventoryAction.Default = Default;
		inventoryAction.Priority = ((Priority == 0) ? (Actions.Count + 1) : Priority);
		inventoryAction.FireOnActor = FireOnActor;
		inventoryAction.WorksAtDistance = WorksAtDistance;
		inventoryAction.WorksTelekinetically = WorksTelekinetically;
		inventoryAction.WorksTelepathically = WorksTelepathically;
		inventoryAction.AsMinEvent = AsMinEvent;
		inventoryAction.FireOn = FireOn;
		Actions[Name] = inventoryAction;
		return true;
	}
}

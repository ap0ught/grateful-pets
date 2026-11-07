using System.Collections.Generic;

namespace XRL.World;

[GameEvent(Base = true)]
public abstract class IInventoryActionsEvent : MinEvent
{
	public GameObject Actor;

	public GameObject Object;

	public Dictionary<string, InventoryAction> Actions;

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Actor = null;
		Object = null;
		Actions = null;
	}

	public bool AddAction(string Name, string Display = null, string Command = null, string PreferToHighlight = null, char Key = ' ', bool FireOnActor = false, int Default = 0, int Priority = 0, bool Override = false, bool WorksAtDistance = false, bool WorksTelekinetically = false, bool WorksTelepathically = false, bool AsMinEvent = true, GameObject FireOn = null, bool ReturnToModernUI = false)
	{
		if (Actions == null)
		{
			Actions = new Dictionary<string, InventoryAction>();
		}
		else if (!Override && Actions.ContainsKey(Name))
		{
			return false;
		}
		InventoryAction inventoryAction = new InventoryAction();
		inventoryAction.Name = Name;
		inventoryAction.Key = Key;
		inventoryAction.Display = Display;
		inventoryAction.Command = Command;
		inventoryAction.PreferToHighlight = PreferToHighlight;
		inventoryAction.Default = Default;
		inventoryAction.Priority = Priority;
		inventoryAction.FireOnActor = FireOnActor;
		inventoryAction.WorksAtDistance = WorksAtDistance;
		inventoryAction.WorksTelekinetically = WorksTelekinetically;
		inventoryAction.WorksTelepathically = WorksTelepathically;
		inventoryAction.AsMinEvent = AsMinEvent;
		inventoryAction.FireOn = FireOn;
		inventoryAction.ReturnToModernUI = ReturnToModernUI;
		Actions[Name] = inventoryAction;
		return true;
	}
}

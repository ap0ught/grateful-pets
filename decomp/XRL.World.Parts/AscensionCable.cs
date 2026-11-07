using XRL.UI;
using XRL.World.Conversations.Parts;

namespace XRL.World.Parts;

public class AscensionCable : IPart
{
	public const string ASCEND_COMMAND = "AscendCable";

	public const string ASCEND_QUERY = "AscendQueryGolem";

	public const string ASCEND_NOTIFY = "AscendNotifyGolem";

	public const string DESCEND_COMMAND = "DescendCable";

	public bool InSheva => ParentObject.CurrentZone?.ZoneWorld == "NorthSheva";

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (E.Actor.IsPlayer())
		{
			if (InSheva && TryDescend(E.Actor))
			{
				return false;
			}
			if (!InSheva && TryAscend(E.Actor))
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (InSheva)
		{
			E.AddAction("Descend", "descend", "DescendCable", null, 'a');
		}
		else
		{
			E.AddAction("Ascend", "ascend", "AscendCable", null, 'a');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "AscendCable" && TryAscend(E.Actor, FromDialog: true))
		{
			E.RequestInterfaceExit();
		}
		else if (E.Command == "DescendCable" && TryDescend(E.Actor, FromDialog: true))
		{
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public bool TryAscend(GameObject Actor, bool FromDialog = false)
	{
		if (!Actor.IsPlayer())
		{
			return false;
		}
		Interior part = Actor.GetPart<Interior>();
		if (!Actor.HasTag("Golem") || Actor.IsTemporary || !Actor.HasPart<Vehicle>() || part?.Zone == null)
		{
			Popup.Show("You don't have the capacity to ascend " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			return false;
		}
		if (!The.Game.HasQuest("We Are Starfreight") || !The.Game.HasFinishedQuest("Reclamation"))
		{
			Popup.Show("You can't safely ascend " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " right now.");
			return false;
		}
		if (!Popup.AskString("Are you sure you want to ascend the Spindle? You know of no means to descend again.\n\nType 'ASCEND' to confirm.", "", "Sounds/UI/ui_notification", null, "ASCEND", 6, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).EqualsNoCase("ASCEND"))
		{
			return false;
		}
		if (!GenericQueryEvent.Check(ParentObject, "AscendQueryGolem", Actor, null, 0, BaseResult: true))
		{
			return false;
		}
		GenericNotifyEvent.Send(ParentObject, "AscendNotifyGolem", Actor);
		return true;
	}

	public bool TryDescend(GameObject Actor, bool FromDialog = false)
	{
		if (!Actor.IsPlayer())
		{
			return false;
		}
		Interior part = Actor.GetPart<Interior>();
		if (!Actor.HasTag("Golem") || Actor.IsTemporary || !Actor.HasPart<Vehicle>() || part?.Zone == null)
		{
			Popup.Show("You don't have the capacity to descend " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + ".");
			return false;
		}
		if (!EndGame.IsArkOpened)
		{
			Popup.Show("You can't safely descend " + ParentObject.t(int.MaxValue, null, null, AsIfKnown: false, Single: false, NoConfusion: false, NoColor: false, Stripped: true) + " right now.");
			return false;
		}
		if (!Popup.AskString("Are you sure you want to return to Qud? \n\nType 'RETURN' to confirm.", "", "Sounds/UI/ui_notification", null, "RETURN", 6, 0, ReturnNullForEscape: false, EscapeNonMarkupFormatting: true, false).EqualsNoCase("RETURN"))
		{
			return false;
		}
		The.Game.SetStringGameState("EndType", "Return");
		if (NephalProperties.IsFoiled("Ehalcodon"))
		{
			The.Game.SetStringGameState("EndGrade", "Super");
		}
		EndGame.Start();
		return true;
	}
}

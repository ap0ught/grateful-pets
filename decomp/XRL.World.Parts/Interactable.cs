using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class Interactable : IPart
{
	public const int FLAG_SMART_USE = 1;

	public const int FLAG_POPUP = 2;

	public string Action;

	public string Message;

	public string Sound;

	public int Flags = 1;

	[NonSerialized]
	private string _CommandID;

	public string CommandID
	{
		get
		{
			if (_CommandID.IsNullOrEmpty())
			{
				_CommandID = Guid.NewGuid().ToString();
			}
			return _CommandID;
		}
	}

	public bool CanSmartUse
	{
		get
		{
			return Flags.HasBit(1);
		}
		set
		{
			Flags.SetBit(1, value);
		}
	}

	public bool UsePopup
	{
		get
		{
			return Flags.HasBit(2);
		}
		set
		{
			Flags.SetBit(2, value);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != CanSmartUseEvent.ID && ID != CommandSmartUseEvent.ID && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanSmartUseEvent E)
	{
		return !CanSmartUse;
	}

	public override bool HandleEvent(CommandSmartUseEvent E)
	{
		if (CanSmartUse)
		{
			DoInteract(E.Actor);
			return false;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (!Action.IsNullOrEmpty())
		{
			E.AddAction(CommandID, Action, CommandID, null, char.ToLower(Action[0]));
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == CommandID)
		{
			DoInteract(E.Actor);
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public void DoInteract(GameObject Actor)
	{
		if (!Sound.IsNullOrEmpty())
		{
			PlayWorldSound(Sound);
		}
		if (!Message.IsNullOrEmpty())
		{
			string text = Message.StartReplace().AddObject(Actor).AddObject(ParentObject)
				.ToString();
			if (UsePopup)
			{
				Popup.Show(text);
			}
			else
			{
				EmitMessage(text);
			}
		}
	}
}

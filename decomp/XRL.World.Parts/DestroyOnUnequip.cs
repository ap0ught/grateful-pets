using System;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class DestroyOnUnequip : IPart
{
	public string Message = "The mote of light is extinguished.";

	public override bool SameAs(IPart p)
	{
		if ((p as DestroyOnUnequip).Message != Message)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (E.Actor.IsPlayer() && !Message.IsNullOrEmpty())
		{
			Popup.Show(GameText.VariableReplace(Message, ParentObject, null, StripColors: true));
		}
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		if (E.DestroyOnUnequipDeclined)
		{
			return false;
		}
		if (E.AutoEquipTry > 0)
		{
			GameObject actor = E.Actor;
			if (actor != null && actor.IsPlayer() && Popup.ShowYesNoCancel(ParentObject.T() + " will be destroyed if " + ParentObject.itis + " unequipped. Do you want to continue?") != DialogResult.Yes)
			{
				E.DestroyOnUnequipDeclined = true;
				return false;
			}
		}
		return base.HandleEvent(E);
	}
}

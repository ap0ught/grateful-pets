using System;

namespace XRL.World.Parts;

[Serializable]
public class SpiralIron : IPart
{
	public static readonly string COMMAND_NAME = "PressIron";

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		E.AddAction("Press", "press", COMMAND_NAME, null, 'p', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			PressSpiralIron(E.Actor, !E.Auto);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool PressSpiralIron(GameObject Actor, bool FromDialog)
	{
		PlayWorldSound("Sounds/Interact/sfx_interact_curlingIron_press");
		DidX("hiss", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog, Actor.IsPlayer());
		return true;
	}
}

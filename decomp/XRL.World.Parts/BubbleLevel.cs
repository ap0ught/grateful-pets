using System;

namespace XRL.World.Parts;

[Serializable]
public class BubbleLevel : IPart
{
	public static readonly string COMMAND_NAME = "BubbleLevel";

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
		E.AddAction("Flip", "flip", COMMAND_NAME, null, 'f', FireOnActor: false, 10);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			FlipBubbleLevel(E.Actor, !E.Auto);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public bool FlipBubbleLevel(GameObject Actor, bool FromDialog)
	{
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume != null && liquidVolume.Volume > 0)
		{
			PlayWorldSound("Sounds/Interact/sfx_interact_bubbleLevel_flip");
			DidX("plop", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog, Actor.IsPlayer());
		}
		else
		{
			DidX("make", "no sound", null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog, Actor.IsPlayer());
		}
		return true;
	}
}

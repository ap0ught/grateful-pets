using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class NoStandUp : IPart
{
	public bool CanBeStoodUp = true;

	public override bool SameAs(IPart p)
	{
		if ((p as NoStandUp).CanBeStoodUp != CanBeStoodUp)
		{
			return false;
		}
		return base.SameAs(p);
	}

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
		if (CanBeStoodUp && ParentObject.HasEffect<Prone>())
		{
			E.AddAction("Stand Up", "stand up", "BeStoodUp", null, 's', FireOnActor: false, 100, 0, Override: false, WorksAtDistance: false, WorksTelekinetically: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "BeStoodUp" && CanBeStoodUp && ParentObject.HasEffect<Prone>() && E.Actor.CheckFrozen(Telepathic: false, Telekinetic: true))
		{
			if (ParentObject.CanChangeBodyPosition("Standing", ShowMessage: false, Involuntary: true))
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, "stand", ParentObject, "up", null, null, null, ParentObject, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
				ParentObject.RemoveEffect<Prone>();
				ParentObject.BodyPositionChanged(null, Involuntary: true);
			}
			else
			{
				IComponent<GameObject>.XDidYToZ(E.Actor, "try", "to stand", ParentObject, "up, but cannot", null, null, null, null, E.Actor, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: false, IndefiniteObjectForOthers: false, PossessiveObject: false, null, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: false, AlwaysVisible: false, FromDialog: true);
			}
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("CanStandUp");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "CanStandUp")
		{
			return false;
		}
		return base.FireEvent(E);
	}
}

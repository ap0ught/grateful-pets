using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class FungalVision : IPart
{
	public bool originalSolid;

	public bool originalOccluding;

	public override bool SameAs(IPart p)
	{
		FungalVision fungalVision = p as FungalVision;
		if (fungalVision.originalSolid != originalSolid)
		{
			return false;
		}
		if (fungalVision.originalOccluding != originalOccluding)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != CanHaveConversationEvent.ID)
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0 && ParentObject == E.Object && !ParentObject.IsPlayer())
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanHaveConversationEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (FungalVisionary.VisionLevel <= 0)
		{
			if (ParentObject.Render.Occluding && ParentObject.Physics.CurrentCell != null)
			{
				ParentObject.CurrentCell.ClearOccludeCache();
			}
			ParentObject.Render.Visible = false;
			ParentObject.Physics.Solid = false;
			ParentObject.Render.Occluding = false;
		}
		else
		{
			if (ParentObject.Render.Occluding != originalOccluding && ParentObject.Physics.CurrentCell != null)
			{
				ParentObject.CurrentCell.ClearOccludeCache();
			}
			ParentObject.Render.Visible = true;
			ParentObject.Physics.Solid = originalSolid;
			ParentObject.Render.Occluding = originalOccluding;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		originalSolid = ParentObject.Physics.Solid;
		originalOccluding = ParentObject.Render.Occluding;
		if (FungalVisionary.VisionLevel <= 0)
		{
			ParentObject.Physics.Solid = false;
			ParentObject.Render.Occluding = false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforePhysicsRejectObjectEntringCell");
		Registrar.Register("CanHypersensesDetect");
		Registrar.Register("PreventSmartUse");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "PreventSmartUse" || E.ID == "CanHypersensesDetect")
		{
			if (FungalVisionary.VisionLevel <= 0)
			{
				return false;
			}
		}
		else if (E.ID == "BeforePhysicsRejectObjectEntringCell" && FungalVisionary.VisionLevel <= 0)
		{
			return false;
		}
		return base.FireEvent(E);
	}
}

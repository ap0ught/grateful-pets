using System;

namespace XRL.World.Parts;

[Serializable]
public class CursedCybernetics : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<BeginBeingUnequippedEvent>.ID && ID != PooledEvent<CanBeUnequippedEvent>.ID)
		{
			return ID == PooledEvent<IsAfflictionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeUnequippedEvent E)
	{
		if (!E.Forced && (!ParentObject.IsImplant || ParentObject.Implantee != null))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginBeingUnequippedEvent E)
	{
		if (!E.Forced && (!ParentObject.IsImplant || ParentObject.Implantee != null))
		{
			E.AddFailureMessage("You can't remove " + ParentObject.t() + ".");
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(IsAfflictionEvent E)
	{
		return false;
	}
}

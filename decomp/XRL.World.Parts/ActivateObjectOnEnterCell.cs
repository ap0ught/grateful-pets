using System;

namespace XRL.World.Parts;

[Serializable]
public class ActivateObjectOnEnterCell : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		ParentObject.MakeActive();
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

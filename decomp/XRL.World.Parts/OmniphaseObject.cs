using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class OmniphaseObject : IPart
{
	public bool Visual = true;

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		E.Object.ForceApplyEffect(new Omniphase(9999, Name, Visual));
		E.Object.RemovePart(this);
		return base.HandleEvent(E);
	}
}

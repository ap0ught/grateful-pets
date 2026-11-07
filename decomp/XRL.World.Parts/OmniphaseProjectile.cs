using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class OmniphaseProjectile : IPart
{
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
		ParentObject.ForceApplyEffect(new Omniphase(Name));
		return base.HandleEvent(E);
	}
}

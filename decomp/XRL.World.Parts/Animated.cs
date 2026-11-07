using System;

namespace XRL.World.Parts;

[Serializable]
[Obsolete]
public class Animated : IPart
{
	public override bool SameAs(IPart p)
	{
		return base.SameAs(p);
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
		AnimateObject.Animate(ParentObject);
		return base.HandleEvent(E);
	}
}

using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GreaseBoots : IPart
{
	public string BehaviorDescription = "You may walk freely over webs.";

	public override bool SameAs(IPart p)
	{
		if ((p as GreaseBoots).BehaviorDescription != BehaviorDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(BehaviorDescription);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.ApplyEffect(new Greased());
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveEffect<Greased>();
		return base.HandleEvent(E);
	}
}

using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class SpringBoots : IPart
{
	public int Timer;

	public override bool SameAs(IPart p)
	{
		if ((p as SpringBoots).Timer != Timer)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != EquippedEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (Timer > 0)
		{
			Timer--;
			if (Timer == 0)
			{
				Timer--;
				ParentObject.Equipped?.ApplyEffect(new Springing(ParentObject));
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		Timer = 100;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		Timer = 0;
		Effect effect = E.Actor.GetEffect((Springing fx) => fx.Source == ParentObject);
		if (effect != null)
		{
			E.Actor.RemoveEffect(effect);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

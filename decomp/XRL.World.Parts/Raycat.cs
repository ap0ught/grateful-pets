using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Raycat : IPart
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<PhysicalContactEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(PhysicalContactEvent E)
	{
		if (E.Object == ParentObject && E.Actor != E.Object && GameObject.Validate(E.Actor) && !E.Actor.HasEffect(typeof(Luminous)))
		{
			E.Actor.ApplyEffect(new Luminous(100 + Stat.Random(0, 100)));
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}

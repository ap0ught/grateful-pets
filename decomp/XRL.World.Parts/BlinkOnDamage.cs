using System;

namespace XRL.World.Parts;

[Serializable]
public class BlinkOnDamage : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeApplyDamage");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeApplyDamage" && !ParentObject.OnWorldMap() && !(E.GetParameter("Damage") as Damage).HasAttribute("Unavoidable") && IComponent<GameObject>.CheckRealityDistortionUsability(ParentObject, null, ParentObject))
		{
			DidX("blink", "away from the danger");
			ParentObject.RandomTeleport(Swirl: true);
		}
		return base.FireEvent(E);
	}
}

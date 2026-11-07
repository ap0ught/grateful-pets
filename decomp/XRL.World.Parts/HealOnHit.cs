using System;

namespace XRL.World.Parts;

[Serializable]
public class HealOnHit : IPart
{
	public int Chance;

	public string Amount = "15-25";

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Attacker");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Defender");
			GameObject parentObject = ParentObject;
			GameObject subject = gameObjectParameter2;
			if (GetSpecialEffectChanceEvent.GetFor(gameObjectParameter, parentObject, "Part HealOnHit Activation", Chance, subject).in100() && gameObjectParameter2.HasStat("Hitpoints") && gameObjectParameter2.IsOrganic)
			{
				gameObjectParameter2.Heal(Amount.RollCached(), Message: true, FloatText: true);
			}
		}
		return base.FireEvent(E);
	}
}

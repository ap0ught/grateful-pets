using System;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class MutationPointsOnEat : IPart
{
	public int Points = 1;

	public int TrueKinPoisonDuration = 40;

	public string TrueKinPoisonDamage = "2d6";

	public int TrueKinPoisonStrength = 30;

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("OnEat");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "OnEat")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			if (gameObjectParameter.IsTrueKin())
			{
				gameObjectParameter.ApplyEffect(new Poisoned(TrueKinPoisonDuration, TrueKinPoisonDamage, TrueKinPoisonStrength));
			}
			else if (gameObjectParameter.CanGainMP())
			{
				gameObjectParameter.GainMP(Points);
				if (gameObjectParameter.IsPlayer())
				{
					Popup.Show("Your genome destabilizes and you gain " + Grammar.Cardinal(Points) + " mutation " + ((Points == 1) ? "point" : "points") + ".");
				}
			}
		}
		return base.FireEvent(E);
	}
}

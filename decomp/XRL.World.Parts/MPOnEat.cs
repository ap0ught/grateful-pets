using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class MPOnEat : IPart
{
	public string Value = "1";

	public override bool SameAs(IPart p)
	{
		return true;
	}

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
			int num = Stat.Roll(Value);
			if (gameObjectParameter != null && gameObjectParameter.CanGainMP())
			{
				gameObjectParameter.GainMP(num);
				if (gameObjectParameter.IsPlayer())
				{
					IComponent<GameObject>.AddPlayerMessage("You gain " + num + " mutation " + ((num == 1) ? "point" : "points") + "!");
				}
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Ironshroom : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectEnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectEnteredCell")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
			if (gameObjectParameter != null && gameObjectParameter.IsCombat)
			{
				IComponent<GameObject>.XDidYToZ(gameObjectParameter, "are", "impaled by", ParentObject, null, null, null, null, null, gameObjectParameter);
				if (gameObjectParameter.TakeDamage(Stat.Random(1, 10), "from %t impalement.", null, null, null, null, ParentObject))
				{
					gameObjectParameter.Bloodsplatter();
					gameObjectParameter.ApplyEffect(new Bleeding("1d3", 30, ParentObject, Stack: false));
				}
			}
		}
		return base.FireEvent(E);
	}
}

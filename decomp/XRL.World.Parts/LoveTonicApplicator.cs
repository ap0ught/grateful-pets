using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LoveTonicApplicator : IPart
{
	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ApplyTonic");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ApplyTonic")
		{
			int intParameter = E.GetIntParameter("Dosage");
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			GameObject gameObjectParameter2 = E.GetGameObjectParameter("Subject");
			if (gameObjectParameter2.IsPlayer())
			{
				if (intParameter <= 0)
				{
					return false;
				}
				LoveTonic e = new LoveTonic(GetTonicDurationEvent.GetFor(ParentObject, gameObjectParameter, gameObjectParameter2, "Love", Stat.Random(500 * intParameter, 700 * intParameter), intParameter));
				if (!gameObjectParameter2.ApplyEffect(e))
				{
					return false;
				}
			}
			else
			{
				object obj = E.GetGameObjectParameter("Attacker");
				if (obj == null)
				{
					Cell cell = gameObjectParameter2.CurrentCell;
					if (cell == null)
					{
						obj = null;
					}
					else
					{
						List<GameObject> list = cell.FastFloodVisibility("Combat", 20);
						obj = ((list != null) ? list.GetRandomElement() : null);
					}
					if (obj == null)
					{
						obj = The.Player;
					}
				}
				GameObject gameObject = (GameObject)obj;
				if (gameObject != null && intParameter > 0)
				{
					GameObject gameObject2 = gameObject;
					if (gameObjectParameter2.CheckInfluence(Name, gameObject2) && GetChance(gameObject, gameObjectParameter2).in100())
					{
						Lovesick e2 = new Lovesick(Stat.Random(3000 * intParameter, 3600 * intParameter), gameObject);
						if (!gameObjectParameter2.ApplyEffect(e2))
						{
							return false;
						}
						goto IL_016d;
					}
				}
				IComponent<GameObject>.AddPlayerMessage(gameObjectParameter2.Does("look") + " you over and" + gameObjectParameter2.GetVerb("metabolize") + " the love tonic with no effect.");
			}
		}
		goto IL_016d;
		IL_016d:
		return base.FireEvent(E);
	}

	public int GetChance(GameObject Attacker, GameObject Target)
	{
		return 95 + Attacker.Stat("Level") - Target.Stat("Level") - Target.GetIntProperty("LoveTonicResistance");
	}
}

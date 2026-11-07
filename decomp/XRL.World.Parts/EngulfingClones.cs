using System;
using XRL.Language;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingClones : IPart
{
	public int TurnsLeft = -1;

	public string Cooldown = "8";

	public string Clones = "1";

	public string Prefix = "Refracted";

	public string ColorString = "&Y";

	public int RealityStabilizationPenetration;

	public string Sound = "sfx_light_refract";

	public string Description = "It's refracted you.";

	public string Message;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EndTurnEngulfing");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurnEngulfing")
		{
			if (TurnsLeft < 0)
			{
				TurnsLeft = Stat.Roll(Cooldown);
			}
			else if (TurnsLeft == 0)
			{
				GameObject gameObjectParameter = E.GetGameObjectParameter("Object");
				Cell cell = ParentObject.CurrentCell;
				if (gameObjectParameter != null && cell != null)
				{
					int num = Stat.Roll(Clones);
					int num2 = 0;
					for (int i = 0; i < num; i++)
					{
						Cell randomElement = cell.GetEmptyAdjacentCells().GetRandomElement();
						if (randomElement == null)
						{
							continue;
						}
						Event obj = Event.New("InitiateRealityDistortionTransit");
						obj.SetParameter("Object", gameObjectParameter);
						obj.SetParameter("Cell", randomElement);
						obj.SetParameter("Mutation", this);
						obj.SetParameter("RealityStabilizationPenetration", RealityStabilizationPenetration);
						if (gameObjectParameter.FireEvent(obj) && randomElement.FireEvent(obj))
						{
							string prefix = Prefix;
							string description = Description;
							string message = Message;
							if (EvilTwin.CreateEvilTwin(gameObjectParameter, prefix, randomElement, message, ColorString, ParentObject, null, MakeExtras: true, description))
							{
								num2++;
							}
						}
					}
					if (num2 > 0)
					{
						PlayWorldSound(Sound, 0.5f, 0f, Combat: true);
						DidXToY("refract", gameObjectParameter, "into " + Grammar.Cardinal(num2) + " additional " + ((num2 == 1) ? "clone" : "clones"), null, null, null, null, gameObjectParameter);
					}
					else if (num > 0)
					{
						DidXToY("try", "to refract", gameObjectParameter, "but fails to push through the normality lattice in the local region of spacetime", null, null, null, null, ParentObject);
					}
					TurnsLeft = Stat.Roll(Cooldown);
				}
			}
			else
			{
				TurnsLeft--;
			}
		}
		return base.FireEvent(E);
	}
}

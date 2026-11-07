using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class EngulfingBleeding : IPart
{
	public int BleedFrequency = 3;

	public int Countdown;

	public string Damage = "1-2";

	public int BaseSave = 20;

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
			Countdown--;
			if (Countdown <= 0)
			{
				Countdown = BleedFrequency;
				E.GetParameter<GameObject>("Object")?.ApplyEffect(new Bleeding(Damage, BaseSave, ParentObject));
			}
		}
		return true;
	}
}

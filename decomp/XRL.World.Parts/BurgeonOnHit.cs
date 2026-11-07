using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class BurgeonOnHit : IPart
{
	public int Chance = 100;

	public string Level = "10";

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WeaponHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WeaponHit" && Chance.in100())
		{
			GameObject Object = E.GetGameObjectParameter("Attacker");
			GameObject Object2 = E.GetGameObjectParameter("Defender");
			if (GameObject.Validate(ref Object) && GameObject.Validate(ref Object2) && !Object2.IsNowhere())
			{
				UnwelcomeGermination.Germinate(Object, Level.RollCached(), 1, friendly: true, Object2.CurrentCell);
				IComponent<GameObject>.XDidY(Object, "cause", "several plants to germinate with the force of " + Object.its_(ParentObject), "!", null, null, Object);
			}
		}
		return base.FireEvent(E);
	}
}

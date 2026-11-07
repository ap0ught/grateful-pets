using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class SecretsOnEat : IPart
{
	public string Number = "1-2";

	public override bool SameAs(IPart p)
	{
		if ((p as SecretsOnEat).Number != Number)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eaten");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eaten" && E.GetGameObjectParameter("Eater").IsPlayer())
		{
			int i = 0;
			for (int num = Number.RollCached(); i < num; i++)
			{
				JournalAPI.RevealRandomSecret();
			}
		}
		return base.FireEvent(E);
	}
}

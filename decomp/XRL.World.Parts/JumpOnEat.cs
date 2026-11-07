using System;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class JumpOnEat : IPart
{
	public string Hops = "1";

	public string BonusRange = "0";

	public string SourceKey = "JumpOnEat";

	public override bool SameAs(IPart p)
	{
		JumpOnEat jumpOnEat = p as JumpOnEat;
		if (jumpOnEat.Hops != Hops)
		{
			return false;
		}
		if (jumpOnEat.BonusRange != BonusRange)
		{
			return false;
		}
		if (jumpOnEat.SourceKey != SourceKey)
		{
			return false;
		}
		return base.SameAs(p);
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
			int num = Hops.RollCached();
			bool flag = false;
			for (int i = 0; i < num; i++)
			{
				if (!Acrobatics_Jump.Jump(gameObjectParameter, BonusRange.RollCached(), null, SourceKey))
				{
					break;
				}
				flag = true;
			}
			if (flag && gameObjectParameter.IsPlayer())
			{
				E.RequestInterfaceExit();
			}
		}
		return base.FireEvent(E);
	}
}

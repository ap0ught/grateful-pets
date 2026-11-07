using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class Diseased : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ObjectCreated");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ObjectCreated" && 33.in100())
		{
			switch (Stat.Random(1, 2))
			{
			case 1:
				ParentObject.ApplyEffect(new Glotrot());
				ParentObject.GetEffect<Glotrot>().Stage = Stat.Random(0, 3);
				break;
			case 2:
				ParentObject.ApplyEffect(new Ironshank());
				ParentObject.GetEffect<Ironshank>().SetPenalty(Stat.Random(1, 75));
				break;
			}
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class BreatheOnEat : IPart
{
	public string Class = "";

	public int Level = 5;

	public override bool SameAs(IPart p)
	{
		BreatheOnEat breatheOnEat = p as BreatheOnEat;
		if (breatheOnEat.Class == Class)
		{
			return breatheOnEat.Level == Level;
		}
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("Eaten");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "Eaten")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Eater");
			BreatherBase obj = Activator.CreateInstance(ModManager.ResolveType("XRL.World.Parts.Mutation." + Class)) as BreatherBase;
			obj.ParentObject = gameObjectParameter;
			obj.Level = Level;
			return BreatherBase.Cast(obj);
		}
		return base.FireEvent(E);
	}
}

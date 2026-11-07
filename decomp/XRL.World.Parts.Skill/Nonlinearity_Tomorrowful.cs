using System;

namespace XRL.World.Parts.Skill;

[Serializable]
public class Nonlinearity_Tomorrowful : BaseSkill
{
	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("InitiatePrecognition");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "InitiatePrecognition")
		{
			int intParameter = E.GetIntParameter("Duration");
			E.SetParameter("Duration", intParameter * 2);
		}
		return base.FireEvent(E);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("time", 3);
		}
		return base.HandleEvent(E);
	}
}

using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class Calming : IPart
{
	public int Feeling = 50;

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool HandleEvent(GetFeelingEvent E)
	{
		if (E.Faction)
		{
			E.Feeling = Feeling;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AITargetCreateKill");
		Registrar.Register(PooledEvent<GetFeelingEvent>.ID);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITargetCreateKill")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (!gameObjectParameter.HasIntProperty("Calmed") && !gameObjectParameter.IsPlayerControlled())
			{
				gameObjectParameter.StopFighting(ParentObject);
				gameObjectParameter.AddOpinion<OpinionMollify>(ParentObject, Feeling);
				gameObjectParameter.SetIntProperty("Calmed", 1);
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

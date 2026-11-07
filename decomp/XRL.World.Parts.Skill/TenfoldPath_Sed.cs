using System;
using XRL.World.AI;

namespace XRL.World.Parts.Skill;

/// This part is not used in the base game.
[Serializable]
public class TenfoldPath_Sed : BaseInitiatorySkill
{
	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetSocialSifrahSetupEvent>.ID)
		{
			return ID == PooledEvent<ReputationChangeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetSocialSifrahSetupEvent E)
	{
		E.Rating += 10;
		E.Turns++;
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReputationChangeEvent E)
	{
		if (E.BaseAmount < 0 && !E.Transient && ParentObject.IsPlayer())
		{
			E.Amount /= 2;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EvilTwinAttitudeSetup");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EvilTwinAttitudeSetup")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Twin");
			if (gameObjectParameter?.Brain != null)
			{
				gameObjectParameter.Brain.Factions = "Entropic-100";
				gameObjectParameter.Brain.AddOpinion<OpinionMollify>(ParentObject, 100f);
			}
			return false;
		}
		return base.FireEvent(E);
	}
}

using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class FeelingOnTarget : IActivePart
{
	public int Chance;

	public int Feeling = 25;

	public bool CalmHate;

	public FeelingOnTarget()
	{
		WorksOnHolder = true;
		WorksOnCarrier = true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetShortDescriptionEvent.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != DroppedEvent.ID)
		{
			return ID == TakenEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (Chance > 0)
		{
			E.Postfix.Compound("{{rules|", '\n').Append(Chance).Append('%')
				.Append(" chance for hostile creatures to wander away disinterestedly when they first notice you}}");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TakenEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(DroppedEvent E)
	{
		EvaluateAsSubject(E.Actor);
		return base.HandleEvent(E);
	}

	public void EvaluateAsSubject(GameObject Object)
	{
		if (IsObjectActivePartSubject(Object))
		{
			Object.RegisterPartEvent(this, "AITargetCreateKill");
		}
		else
		{
			Object.UnregisterPartEvent(this, "AITargetCreateKill");
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AITargetCreateKill")
		{
			GameObject gameObjectParameter = E.GetGameObjectParameter("Actor");
			if (gameObjectParameter == null || gameObjectParameter.HasIntProperty("AsteriskNeutral"))
			{
				return true;
			}
			GameObject activePartFirstSubject = GetActivePartFirstSubject();
			if (activePartFirstSubject == null || IsDisabled(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
			{
				return true;
			}
			if (!CalmHate)
			{
				Faction ifExists = Factions.GetIfExists(gameObjectParameter.GetPrimaryFaction());
				if (ifExists != null && ifExists.HatesPlayer)
				{
					return true;
				}
			}
			gameObjectParameter.SetIntProperty("AsteriskNeutral", 1);
			if (Chance.in100())
			{
				gameObjectParameter.StopFighting(activePartFirstSubject);
				gameObjectParameter.AddOpinion<OpinionMollify>(activePartFirstSubject, Feeling);
				IComponent<GameObject>.XDidYToZ(gameObjectParameter, "decide", gameObjectParameter.it + gameObjectParameter.GetVerb("aren't", PrependSpace: true, PronounAntecedent: true) + " angry at", activePartFirstSubject);
				return false;
			}
		}
		return base.FireEvent(E);
	}
}

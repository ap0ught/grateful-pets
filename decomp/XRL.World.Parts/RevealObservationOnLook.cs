using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealObservationOnLook : IPart
{
	public string ObservationId;

	public bool bLookedAt;

	public RevealObservationOnLook()
	{
	}

	public RevealObservationOnLook(string _ObservationId)
	{
		ObservationId = _ObservationId;
	}

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt")
		{
			if (bLookedAt)
			{
				return true;
			}
			bLookedAt = true;
			if (ObservationId == null)
			{
				if (!ParentObject.HasPart<AddObservation>())
				{
					return true;
				}
				AddObservation part = ParentObject.GetPart<AddObservation>();
				ObservationId = part.ID;
			}
			JournalAPI.RevealObservation(ObservationId, onlyIfNotRevealed: true);
			return true;
		}
		return base.FireEvent(E);
	}
}

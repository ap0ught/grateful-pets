using System;
using Qud.API;

namespace XRL.World.Parts;

[Serializable]
public class RevealObservationOnRead : IPart
{
	public string ObservationId;

	public RevealObservationOnRead()
	{
	}

	public RevealObservationOnRead(string ObservationId)
	{
		this.ObservationId = ObservationId;
	}

	public override bool SameAs(IPart p)
	{
		if ((p as RevealObservationOnRead).ObservationId != ObservationId)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override void Initialize()
	{
		base.Initialize();
		ParentObject.SetStringProperty("BookID", ParentObject.ID);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetInventoryActionsEvent.ID && ID != PooledEvent<HasBeenReadEvent>.ID)
		{
			return ID == InventoryActionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(HasBeenReadEvent E)
	{
		if (E.Actor == The.Player && (ObservationId.IsNullOrEmpty() || JournalAPI.IsObservationRevealed(ObservationId)))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		CheckObservation();
		E.AddAction("Read", "read", "Read", null, 'r', FireOnActor: false, (!ObservationId.IsNullOrEmpty() && !JournalAPI.IsObservationRevealed(ObservationId)) ? 100 : 15, 0, Override: true);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "Read" && E.Actor.IsPlayer())
		{
			CheckObservation();
			if (ObservationId != null)
			{
				JournalAPI.RevealObservation(ObservationId, onlyIfNotRevealed: true);
			}
		}
		return base.HandleEvent(E);
	}

	private void CheckObservation()
	{
		if (ObservationId == null)
		{
			AddObservation part = ParentObject.GetPart<AddObservation>();
			if (part != null)
			{
				ObservationId = part.ID;
			}
		}
	}
}

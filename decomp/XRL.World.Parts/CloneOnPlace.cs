using System;
using XRL.World.AI;

namespace XRL.World.Parts;

[Serializable]
public class CloneOnPlace : IPart
{
	public int Amount;

	public string Context;

	public bool Force;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override IPart DeepCopy(GameObject Parent)
	{
		CloneOnPlace cloneOnPlace = (CloneOnPlace)base.DeepCopy(Parent);
		cloneOnPlace.Amount = 0;
		return cloneOnPlace;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Force || ParentObject.CanBeReplicated(ParentObject, Context))
		{
			for (int i = 0; i < Amount; i++)
			{
				GameObject gameObject = ParentObject.DeepCopy();
				Cell closestPassableCellFor = E.Cell.getClosestPassableCellFor(gameObject);
				gameObject.RemovePart(typeof(CloneOnPlace));
				closestPassableCellFor.AddObject(gameObject);
				gameObject.MakeActive();
				GameObject partyLeader = ParentObject.PartyLeader;
				if (partyLeader != null)
				{
					gameObject.Brain.SetPartyLeader(partyLeader);
					Type type = ParentObject.Brain.FindAllegiance(partyLeader.BaseID)?.Reason?.GetType();
					if (type != null && Activator.CreateInstance(type) is IAllyReason reason)
					{
						gameObject.Brain.TakeAllegiance(partyLeader, reason);
					}
				}
				WasReplicatedEvent.Send(ParentObject, ParentObject, gameObject, Context);
				ReplicaCreatedEvent.Send(gameObject, ParentObject, ParentObject, Context);
			}
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

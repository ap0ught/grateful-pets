using System;
using System.Collections.Generic;
using XRL.World.ZoneParts;

namespace XRL.World.Parts;

[Serializable]
public class BroadcastPowerField : IZonePart
{
	[NonSerialized]
	private List<GameObject> transmitters;

	public void ValidateTransmitters()
	{
		if (transmitters != null)
		{
			transmitters.RemoveAll((GameObject o) => o == null || !o.IsValid() || o.CurrentZone != ParentZone);
		}
	}

	public void RegisterTransmitter(GameObject transmitter)
	{
		if (transmitters == null)
		{
			transmitters = new List<GameObject>();
		}
		if (!transmitters.Contains(transmitter))
		{
			transmitters.Add(transmitter);
		}
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CollectBroadcastChargeEvent>.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent ev)
	{
		ValidateTransmitters();
		return true;
	}

	public override bool HandleEvent(CollectBroadcastChargeEvent ev)
	{
		if (transmitters == null)
		{
			return true;
		}
		foreach (GameObject transmitter in transmitters)
		{
			BroadcastPowerTransmitter part = transmitter.GetPart<BroadcastPowerTransmitter>();
			if (part != null && !part.HandleEvent(ev))
			{
				return false;
			}
		}
		return true;
	}
}

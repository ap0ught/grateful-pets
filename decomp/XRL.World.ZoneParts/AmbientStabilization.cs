using System;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.ZoneParts;

[Serializable]
public class AmbientStabilization : IZonePart
{
	public int Strength = 40;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<EndTurnEvent>.ID)
		{
			return ID == ZoneActivatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		Stabilize();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneActivatedEvent E)
	{
		Stabilize();
		return base.HandleEvent(E);
	}

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		Stabilize();
	}

	public void Stabilize()
	{
		if (ParentZone.HasObject(The.Player) && !The.Player.HasEffect<AmbientRealityStabilized>())
		{
			Popup.Show("You feel some ambient astral friction here.");
			The.Player.ApplyEffect(new AmbientRealityStabilized(Strength));
		}
	}
}

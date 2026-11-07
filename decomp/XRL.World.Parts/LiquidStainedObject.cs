using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LiquidStainedObject : IPart
{
	public string Liquid = "blood";

	public string Volume = "3d4";

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == ObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		E.Object.ForceApplyEffect(new LiquidStained(new LiquidVolume(Liquid, Volume.RollCached())));
		E.Object.RemovePart(this);
		return base.HandleEvent(E);
	}
}

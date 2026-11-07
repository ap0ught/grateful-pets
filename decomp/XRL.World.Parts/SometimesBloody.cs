using System;

namespace XRL.World.Parts;

[Serializable]
public class SometimesBloody : IPart
{
	public int Chance = 50;

	public string Liquid = "blood";

	public string Volume = "3d4";

	public string Duration = "1";

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
		if (Chance.in100())
		{
			ParentObject.MakeBloody(Liquid, Math.Min(Volume.RollCached(), (int)Math.Round(ParentObject.GetMaximumLiquidExposureAsDouble() * (double)LiquidVolume.GetLiquid(Liquid).Adsorbence / 100.0)), Duration.RollCached());
		}
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}
}

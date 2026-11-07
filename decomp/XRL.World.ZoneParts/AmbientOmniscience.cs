using System;

namespace XRL.World.ZoneParts;

[Serializable]
public class AmbientOmniscience : IZonePart
{
	public bool IsRealityDistortionBased;

	public int Duration;

	public override bool SameAs(IZonePart Part)
	{
		AmbientOmniscience ambientOmniscience = Part as AmbientOmniscience;
		if (ambientOmniscience.IsRealityDistortionBased != IsRealityDistortionBased)
		{
			return false;
		}
		if (ambientOmniscience.Duration != Duration)
		{
			return false;
		}
		return base.SameAs(Part);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID)
		{
			if (ID == SingletonEvent<EndTurnEvent>.ID)
			{
				return Duration > 0;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (ParentZone.HasObject(The.Player))
		{
			if (IsRealityDistortionBased)
			{
				for (int i = 0; i < ParentZone.Height; i++)
				{
					for (int j = 0; j < ParentZone.Width; j++)
					{
						if (IComponent<Zone>.CheckRealityDistortionAccessibility(ParentZone.GetCell(j, i)))
						{
							ParentZone.AddLight(j, i, 0, LightLevel.Omniscient);
						}
					}
				}
			}
			else
			{
				ParentZone.AddLight(LightLevel.Omniscient);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (Duration > 0 && --Duration <= 0)
		{
			ParentZone.RemovePart(this);
		}
		return base.HandleEvent(E);
	}
}

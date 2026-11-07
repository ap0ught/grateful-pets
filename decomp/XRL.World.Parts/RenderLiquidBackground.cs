using System;

namespace XRL.World.Parts;

[Serializable]
public class RenderLiquidBackground : IPart
{
	public override bool Render(RenderEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			GameObject openLiquidVolume = cell.GetOpenLiquidVolume();
			if (openLiquidVolume != null)
			{
				LiquidVolume liquidVolume = openLiquidVolume.LiquidVolume;
				if (liquidVolume != null)
				{
					liquidVolume.GetPrimaryLiquid()?.RenderBackgroundPrimary(liquidVolume, E);
					liquidVolume.GetSecondaryLiquid()?.RenderBackgroundSecondary(liquidVolume, E);
				}
			}
		}
		return true;
	}
}

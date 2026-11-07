using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class Fireflies : IPart
{
	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (IsNight() && Stat.RandomCosmetic(1, 3000) == 1)
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && cell.ParentZone != null && cell.ParentZone == XRLCore.Core.Game.ZoneManager.ActiveZone && !cell.IsLit())
			{
				XRLCore.ParticleManager.Add("firefly", cell.X, cell.Y, 0f, 0f, 60, 0f, 0f, 0L);
			}
		}
		return base.HandleEvent(E);
	}
}

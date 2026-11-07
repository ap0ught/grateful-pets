using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class VisualDistortion : IPart
{
	public int Chance = 2;

	public int MinimumTicks = 10000000;

	public long LastTrigger;

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
		if (Stat.RandomCosmetic(1, 1000) <= Chance && LastTrigger < DateTime.Now.Ticks - MinimumTicks)
		{
			LastTrigger = DateTime.Now.Ticks;
			ParentObject.DilationSplat();
		}
		return base.HandleEvent(E);
	}
}

using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class WarmingUp : Effect
{
	public WarmingUp()
	{
		Duration = 1;
		DisplayName = "{{C|warming up}}";
	}

	public WarmingUp(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override int GetEffectType()
	{
		return 117440514;
	}

	public override string GetDetails()
	{
		return "Getting ready to act.";
	}

	public override bool Apply(GameObject Object)
	{
		return !Object.HasEffect<WarmingUp>();
	}

	public override bool Render(RenderEvent E)
	{
		int num = XRLCore.CurrentFrame % 20;
		if (num > 10 && num < 20)
		{
			E.RenderString = "!";
			E.ColorString = "&W^R";
			E.DetailColor = "W";
			return false;
		}
		return true;
	}
}

using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class PistonPressElement : IPart
{
	public string Direction;

	public bool Danger;

	public override bool SameAs(IPart p)
	{
		return base.SameAs(p);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		base.Register(Object, Registrar);
	}

	public override bool Render(RenderEvent E)
	{
		if (Danger)
		{
			int num = XRLCore.CurrentFrame % 30;
			if (num > 0 && num < 15)
			{
				E.Tile = null;
				E.RenderString = Directions.GetArrowForDirection(Direction);
				E.ColorString = "&R";
				return false;
			}
		}
		return true;
	}

	public override bool FireEvent(Event E)
	{
		return true;
	}
}

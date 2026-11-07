using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class RifleMark : Effect
{
	public GameObject Marker;

	public RifleMark()
	{
		DisplayName = "{{R|marked}}";
		Duration = 1;
	}

	public RifleMark(GameObject Marker)
		: this()
	{
		this.Marker = Marker;
	}

	public override int GetEffectType()
	{
		return 33554433;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDescription()
	{
		if (GameObject.Validate(ref Marker) && Marker.IsVisible())
		{
			return "{{R|marked by " + Marker.an() + "}}";
		}
		return null;
	}

	public override string GetDetails()
	{
		if (GameObject.Validate(ref Marker))
		{
			return "Easier to hit with bows and rifles wielded by " + Marker.an() + ".";
		}
		return "Easier to hit with bows and rifles.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!ApplyEffectEvent.Check(Object, "RifleMark", this))
		{
			return false;
		}
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == EnteredCellEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!GameObject.Validate(ref Marker) || !Object.InActiveZone() || !Marker.HasLOSTo(Object, IncludeSolid: true, BlackoutStops: false, UseTargetability: true))
		{
			Duration = 0;
			Object.CleanEffects();
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && GameObject.Validate(ref Marker) && Marker.IsVisible() && Object != null && !Object.IsPlayer())
		{
			int num = XRLCore.CurrentFrame % 60;
			if (num > 35 && num < 40)
			{
				E.Tile = null;
				E.RenderString = "Î";
				E.ColorString = "^R&k";
			}
		}
		return true;
	}
}

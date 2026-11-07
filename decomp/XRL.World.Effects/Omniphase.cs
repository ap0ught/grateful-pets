using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Omniphase : Effect, ITierInitialized
{
	private int FrameOffset;

	public string Tile;

	public string RenderString = "@";

	public string SourceKey;

	public bool Visual = true;

	public Omniphase()
	{
		DisplayName = "{{Y|omniphase}}";
		FrameOffset = Stat.Random(1, 10000);
		Duration = 9999;
	}

	public Omniphase(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public Omniphase(string SourceKey)
		: this()
	{
		this.SourceKey = SourceKey;
	}

	public Omniphase(int Duration, string SourceKey, bool Visual = true)
		: this(Duration)
	{
		this.SourceKey = SourceKey;
		this.Visual = Visual;
	}

	public Omniphase(Omniphase Source)
		: this()
	{
		Duration = Source.Duration;
		FrameOffset = Source.FrameOffset;
		Tile = Source.Tile;
		RenderString = Source.RenderString;
		SourceKey = Source.SourceKey;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(20, 100);
	}

	public override int GetEffectType()
	{
		return 256;
	}

	public override bool SameAs(Effect e)
	{
		Omniphase omniphase = e as Omniphase;
		if (omniphase.Tile != Tile)
		{
			return false;
		}
		if (omniphase.RenderString != RenderString)
		{
			return false;
		}
		return base.SameAs(e);
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == WasDerivedFromEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(WasDerivedFromEvent E)
	{
		E.Derivation.ForceApplyEffect(new Omniphase(this));
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Physically interactive with both in-phase and out-of-phase creatures and objects.";
	}

	public override bool Apply(GameObject Object)
	{
		bool flag = Object.HasEffect<Omniphase>();
		if (!Object.FireEvent("ApplyOmniphase"))
		{
			return false;
		}
		if (!flag)
		{
			Object?.PlayWorldSound("Sounds/Misc/sfx_phase_shiftOmni");
			Object.FireEvent("AfterOmniphaseStart");
		}
		Tile = Object.Render.Tile;
		RenderString = Object.Render.RenderString;
		FlushNavigationCaches();
		Object.Gravitate();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		if (!Object.HasEffectOtherThan(typeof(Omniphase), this))
		{
			Object?.PlayWorldSound("Sounds/Misc/sfx_phase_shiftIn");
			Object.FireEvent("AfterOmniphaseEnd");
		}
		FlushNavigationCaches();
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Visual && Duration > 0)
		{
			int num = (XRLCore.CurrentFrameLong10 + FrameOffset) % 10000;
			if ((num >= 2000 && num <= 2070) || (num >= 8380 && num <= 8450))
			{
				E.ColorString = "&Y";
			}
			else if ((num >= 3910 && num <= 3980) || (num > 6300 && num <= 6370))
			{
				E.ColorString = "&M";
			}
		}
		return true;
	}
}

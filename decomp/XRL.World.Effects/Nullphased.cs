using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Effects;

[Serializable]
public class Nullphased : Effect
{
	[NonSerialized]
	private int FrameOffset;

	public Nullphased()
	{
		DisplayName = "{{g|nullphased}}";
		FrameOffset = Stat.Random(1, 60);
	}

	public Nullphased(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override int GetEffectType()
	{
		return 256;
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
		if (Duration != 9999 && E.Cell.OnWorldMap())
		{
			Object.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override string GetDetails()
	{
		return "Can't physically interact with creatures and objects.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Nullphased>())
		{
			return false;
		}
		Object.ModIntProperty("Nullphase", 1, RemoveIfZero: true);
		Object.PlayWorldSound("Sounds/Misc/sfx_phase_shiftOut");
		FlushNavigationCaches();
		return true;
	}

	public override void Remove(GameObject Object)
	{
		Object.ModIntProperty("Nullphase", -1, RemoveIfZero: true);
		Object.PlayWorldSound("Sounds/Misc/sfx_phase_shiftIn");
		FlushNavigationCaches();
		Object.Gravitate();
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			int num = (XRLCore.CurrentFrame + FrameOffset) % 60 / 2 % 6;
			if (num == 0)
			{
				E.ApplyColors("&k", 100);
			}
			if (num == 1)
			{
				E.ApplyColors("&K", 100);
			}
			if (num == 2)
			{
				E.ApplyColors("&c", 100);
			}
			if (num == 4)
			{
				E.ApplyColors("&y", 100);
			}
		}
		return true;
	}

	public override bool allowCopyOnNoEffectDeepCopy()
	{
		return true;
	}
}

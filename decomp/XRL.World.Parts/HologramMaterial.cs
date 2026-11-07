using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class HologramMaterial : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public string ColorStrings = "&C,&b,&c,&B";

	public string DetailColors = "c,C,b,b";

	[NonSerialized]
	public int FrameOffset;

	[NonSerialized]
	public int FlickerFrame;

	[NonSerialized]
	private List<string> ColorStringParts;

	[NonSerialized]
	private List<string> DetailColorParts;

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != ObjectCreatedEvent.ID)
		{
			return ID == PooledEvent<RespiresEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ObjectCreatedEvent E)
	{
		ParentObject.MakeImperviousToHeat();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject)
		{
			E.Value = 0;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CanBeInvoluntarilyMovedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetMatterPhaseEvent E)
	{
		E.MinMatterPhase(4);
		return false;
	}

	public override bool HandleEvent(GetMaximumLiquidExposureEvent E)
	{
		E.PercentageReduction = 100;
		return false;
	}

	public override bool HandleEvent(GetScanTypeEvent E)
	{
		if (E.Object == ParentObject)
		{
			E.ScanType = Scanning.Scan.Tech;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public void StartFlicker()
	{
		FlickerFrame = 4;
	}

	public void reset()
	{
		ColorStringParts = null;
		DetailColorParts = null;
	}

	public override bool Render(RenderEvent E)
	{
		string text = null;
		string text2 = null;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (!ColorStrings.IsNullOrEmpty())
		{
			if (ColorStringParts == null)
			{
				ColorStringParts = ColorStrings.CachedCommaExpansion();
			}
			int count = ColorStringParts.Count;
			int num2 = num / count;
			text = ((num2 < count) ? ColorStringParts[num2] : ColorStringParts[count - 1]);
		}
		if (!DetailColors.IsNullOrEmpty())
		{
			if (DetailColorParts == null)
			{
				DetailColorParts = DetailColors.CachedCommaExpansion();
			}
			int count2 = DetailColorParts.Count;
			int num3 = num / count2;
			text2 = ((num3 < count2) ? DetailColorParts[num3] : DetailColorParts[count2 - 1]);
		}
		if (FlickerFrame > 0 || Stat.RandomCosmetic(1, 200) == 1)
		{
			E.Tile = null;
			if (FlickerFrame == 0)
			{
				E.RenderString = "_";
			}
			if (FlickerFrame == 1)
			{
				E.RenderString = "-";
			}
			if (FlickerFrame == 2)
			{
				E.RenderString = "|";
			}
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
			text = "&Y";
		}
		else if (Stat.RandomCosmetic(1, 400) == 1)
		{
			text = "&Y";
		}
		if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty())
		{
			E.ApplyColors(text, text2, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		return true;
	}
}

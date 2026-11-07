using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class ConcealedHologramMaterial : IPart
{
	public static readonly int ICON_COLOR_PRIORITY = 30;

	public int FlickerFrame;

	public int FrameOffset;

	public bool DescriptionPostfix = true;

	public override void AddedAfterCreation()
	{
		base.AddedAfterCreation();
		ParentObject.MakeNonflammable();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID && ID != PooledEvent<GetMaximumLiquidExposureEvent>.ID && ID != PooledEvent<GetScanTypeEvent>.ID && ID != GetShortDescriptionEvent.ID && ID != ObjectCreatedEvent.ID)
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

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (DescriptionPostfix)
		{
			E.Base.Compound(ParentObject.It + ParentObject.GetVerb("flicker") + " subtly.", ' ');
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RespiresEvent E)
	{
		return false;
	}

	public override bool Render(RenderEvent E)
	{
		if (ParentObject.CurrentCell == null)
		{
			return true;
		}
		if (!ParentObject.CurrentCell.IsExplored())
		{
			return true;
		}
		if (!ParentObject.CurrentCell.IsVisible())
		{
			return true;
		}
		if (IComponent<GameObject>.ThePlayer?.CurrentCell == null)
		{
			return true;
		}
		if (ParentObject.DistanceTo(IComponent<GameObject>.ThePlayer) > 1)
		{
			return true;
		}
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		string text = null;
		string text2 = null;
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
			if (num < 8)
			{
				text = "&C";
				text2 = "c";
			}
			else
			{
				text = "&Y";
				text2 = "y";
			}
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		if (num < 4)
		{
			text = "&C";
			text2 = "c";
		}
		else if (num < 8)
		{
			text = "&b";
			text2 = "C";
		}
		else if (num < 12)
		{
			text = "&c";
			text2 = "b";
		}
		if (FlickerFrame == 0 && Stat.RandomCosmetic(1, 400) == 1)
		{
			text = "&Y";
			text2 = "y";
		}
		if (!text.IsNullOrEmpty() || !text2.IsNullOrEmpty())
		{
			E.ApplyColors(text, text2, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.RandomCosmetic(0, 20);
		}
		return true;
	}
}

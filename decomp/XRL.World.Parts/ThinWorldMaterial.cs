using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class ThinWorldMaterial : IPart
{
	public string Tile;

	public string RenderString = "@";

	public int FlickerFrame;

	public int FrameOffset;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CanBeDismemberedEvent>.ID && ID != PooledEvent<CanBeInvoluntarilyMovedEvent>.ID && ID != PooledEvent<GetElectricalConductivityEvent>.ID && ID != PooledEvent<GetMatterPhaseEvent>.ID)
		{
			return ID == PooledEvent<GetMaximumLiquidExposureEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanBeDismemberedEvent E)
	{
		if (E.Object == ParentObject)
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetElectricalConductivityEvent E)
	{
		if (E.Pass == 1 && E.Object == ParentObject)
		{
			E.Value = 100;
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

	public override bool Render(RenderEvent E)
	{
		if (Tile == null)
		{
			Tile = ParentObject.Render.Tile;
			RenderString = ParentObject.Render.RenderString;
		}
		Render render = ParentObject.Render;
		render.Tile = null;
		int num = (XRLCore.CurrentFrame + FrameOffset) % 200;
		if (Stat.Random(1, 200) == 1 || FlickerFrame > 0)
		{
			render.Tile = null;
			if (FlickerFrame == 0)
			{
				render.RenderString = "_";
			}
			if (FlickerFrame == 1)
			{
				render.RenderString = "-";
			}
			if (FlickerFrame == 2)
			{
				render.RenderString = "|";
			}
			E.ColorString = "&C";
			if (FlickerFrame == 0)
			{
				FlickerFrame = 3;
			}
			FlickerFrame--;
		}
		else
		{
			render.RenderString = RenderString;
			render.Tile = Tile;
		}
		if (num < 4)
		{
			render.ColorString = "&C";
			render.TileColor = "&C";
			render.DetailColor = "c";
		}
		else if (num < 8)
		{
			render.ColorString = "&b";
			render.TileColor = "&b";
			render.DetailColor = "C";
		}
		else if (num < 12)
		{
			render.ColorString = "&c";
			render.TileColor = "&c";
			render.DetailColor = "b";
		}
		else
		{
			render.ColorString = "&B";
			render.TileColor = "&B";
			render.DetailColor = "b";
		}
		if (!Options.DisableTextAnimationEffects)
		{
			FrameOffset += Stat.Random(0, 20);
		}
		if (Stat.Random(1, 400) == 1 || FlickerFrame > 0)
		{
			render.ColorString = "&Y";
			render.TileColor = "&Y";
		}
		return base.Render(E);
	}
}

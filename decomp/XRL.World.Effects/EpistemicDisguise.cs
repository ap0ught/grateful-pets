using System;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class EpistemicDisguise : Effect
{
	[NonSerialized]
	public GameObject Sample;

	public EpistemicDisguise()
	{
		Duration = 1;
	}

	public EpistemicDisguise(GameObject Sample)
		: this()
	{
		this.Sample = Sample;
	}

	public static bool ApplyTo(GameObject Object, GameObject Sample)
	{
		EpistemicDisguise effect = Object.GetEffect<EpistemicDisguise>();
		if (effect != null)
		{
			effect.Sample = Sample;
			return true;
		}
		return Object.ApplyEffect(new EpistemicDisguise(Sample));
	}

	public override bool SameAs(Effect FX)
	{
		return FX is EpistemicDisguise;
	}

	public override string GetDescription()
	{
		return null;
	}

	public override void FinalizeRead(SerializationReader Reader)
	{
		if (Object.TryGetPart<Examiner>(out var Part) && !Part.KeepTile)
		{
			Sample = Part.GetActiveSample(Part.EpistemicStatus);
			if (Sample != null)
			{
				return;
			}
		}
		Object.RemoveEffect(this);
	}

	public override bool Render(RenderEvent E)
	{
		if (!E.AsIfKnown)
		{
			GameObject sample = Sample;
			Render render = sample.Render;
			E.RenderString = render.RenderString;
			E.HFlip = render.getHFlip();
			E.VFlip = render.getVFlip();
			if (The.Core.TilesEnabled && !string.IsNullOrEmpty(render.TileColor))
			{
				E.ColorString = render.TileColor;
			}
			else
			{
				E.ColorString = render.ColorString;
			}
			E.DetailColor = render.DetailColor;
			E.HighestLayer = render.RenderLayer;
			if (The.Core.TilesEnabled)
			{
				E.Tile = render.Tile;
			}
			sample.ComponentRender(E);
		}
		return base.Render(E);
	}

	public override bool OverlayRender(RenderEvent E)
	{
		if (!E.AsIfKnown)
		{
			GameObject sample = Sample;
			Render render = sample.Render;
			E.RenderString = render.RenderString;
			E.HFlip = render.getHFlip();
			E.VFlip = render.getVFlip();
			E.HighestLayer = render.RenderLayer;
			if (The.Core.TilesEnabled)
			{
				E.Tile = render.Tile;
			}
			sample.OverlayRender(E);
		}
		return base.OverlayRender(E);
	}
}

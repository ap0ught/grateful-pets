using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;
using UnityEngine;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class NephalVFX : IPart
{
	private class ColorDrip
	{
		public Color C;

		public float life;

		public ColorDrip(Color C)
		{
			this.C = C;
			life = 0.2f;
		}
	}

	[NonSerialized]
	private List<RadialTextParticle> zoomies = new List<RadialTextParticle>();

	private float nextZoomie;

	[NonSerialized]
	private Dictionary<Location2D, ColorDrip> drips = new Dictionary<Location2D, ColorDrip>();

	private double lastStep;

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeRenderEvent.ID;
		}
		return true;
	}

	public override bool FinalRender(RenderEvent E)
	{
		E.WantsToPaint = true;
		return true;
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		double num = (float)XRLCore.FrameTimer.ElapsedMilliseconds / 1000f;
		float num2 = (float)(num - lastStep);
		lastStep = num;
		if (!GameManager.dirtyFocused)
		{
			return;
		}
		Zone currentZone = ParentObject.CurrentZone;
		if (currentZone != null && currentZone.IsActive())
		{
			if (num2 > 0f)
			{
				nextZoomie -= num2;
				if (nextZoomie <= 0f)
				{
					float num3 = 0.1f;
					nextZoomie = num3 * (float)Stat.RandomCosmetic(5, 20);
					RadialTextParticle radialTextParticle = new RadialTextParticle();
					radialTextParticle.x = ParentObject.CurrentCell.X;
					radialTextParticle.y = ParentObject.CurrentCell.Y;
					radialTextParticle.d = Math.Max(Math.Max(ParentObject.CurrentCell.X, ParentObject.CurrentCell.Y), Math.Max(80 - ParentObject.CurrentCell.X, 25 - ParentObject.CurrentCell.Y));
					radialTextParticle.r = (float)Stat.Random(1, 1000) * 2f * MathF.PI / 1000f;
					radialTextParticle.Life = 1;
					zoomies.Add(radialTextParticle);
				}
			}
			for (int i = 0; i < zoomies.Count; i++)
			{
				RadialTextParticle radialTextParticle2 = zoomies[i];
				int x = Convert.ToInt32(ParentObject.CurrentCell.X) + (int)((double)radialTextParticle2.d * Math.Cos(radialTextParticle2.r));
				int y = Convert.ToInt32(ParentObject.CurrentCell.Y) + (int)((double)radialTextParticle2.d * Math.Sin(radialTextParticle2.r) * 0.6660000085830688);
				Location2D location2D = Location2D.Get(x, y);
				if (location2D != null)
				{
					drips[location2D] = new ColorDrip(The.Color[Crayons.GetRandomColorAll()[0]]);
				}
				ConsoleChar consoleChar = buffer.get(x, y);
				if (consoleChar != null && ParentObject.CurrentCell.Location != Location2D.Get(x, y))
				{
					consoleChar.Foreground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.TileForeground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.Background = The.Color.k;
					consoleChar.TileBackground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.Detail = The.Color[Crayons.GetRandomColorAll()[0]];
				}
				radialTextParticle2.d -= num2 * 4f;
				if (radialTextParticle2.d < 0f)
				{
					radialTextParticle2.d = 0f;
					radialTextParticle2.Life = 0;
					continue;
				}
				radialTextParticle2.r += num2;
				x = Convert.ToInt32(ParentObject.CurrentCell.X) + (int)((double)radialTextParticle2.d * Math.Cos(radialTextParticle2.r));
				y = Convert.ToInt32(ParentObject.CurrentCell.Y) + (int)((double)radialTextParticle2.d * Math.Sin(radialTextParticle2.r) * 0.6660000085830688);
				consoleChar = buffer.get(x, y);
				if (consoleChar != null && ParentObject.CurrentCell.Location != Location2D.Get(x, y))
				{
					consoleChar.Foreground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.TileForeground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.Background = The.Color.k;
					consoleChar.TileBackground = The.Color[Crayons.GetRandomColorAll()[0]];
					consoleChar.Detail = The.Color[Crayons.GetRandomColorAll()[0]];
				}
			}
			foreach (KeyValuePair<Location2D, ColorDrip> drip in drips)
			{
				drip.Value.life -= num2;
				ConsoleChar consoleChar2 = buffer.get(drip.Key.X, drip.Key.Y);
				if (consoleChar2 != null && ParentObject.CurrentCell.Location != Location2D.Get(drip.Key.X, drip.Key.Y))
				{
					consoleChar2.Foreground = drip.Value.C;
					consoleChar2.TileForeground = drip.Value.C;
					consoleChar2.Detail = drip.Value.C;
				}
			}
			drips.RemoveAll((KeyValuePair<Location2D, ColorDrip> d) => d.Value.life <= 0f);
			zoomies.RemoveAll((RadialTextParticle z) => z.Life <= 0);
		}
		base.OnPaint(buffer);
	}

	public override bool FireEvent(Event E)
	{
		return base.FireEvent(E);
	}
}

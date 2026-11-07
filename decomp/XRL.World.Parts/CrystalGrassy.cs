using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class CrystalGrassy : IPart
{
	[NonSerialized]
	private static FastNoise fastNoise = new FastNoise();

	public override bool SameAs(IPart p)
	{
		return false;
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
		try
		{
			if (!Options.DisableFloorTextureObjects)
			{
				Zone parentZone = E.Cell.ParentZone;
				for (int i = 0; i < parentZone.Height; i++)
				{
					for (int j = 0; j < parentZone.Width; j++)
					{
						PaintCell(parentZone.GetCell(j, i));
					}
				}
			}
		}
		catch
		{
		}
		return base.HandleEvent(E);
	}

	private static int getSeed(string seed)
	{
		return XRLCore.Core.Game.GetWorldSeed(seed);
	}

	public static double sampleSimplexNoise(string type, int x, int y, int z, int amplitude, float frequencyMultiplier = 1f)
	{
		fastNoise.SetSeed(getSeed(type));
		fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
		fastNoise.SetFrequency(0.25f * frequencyMultiplier);
		fastNoise.SetFractalType(FastNoise.FractalType.FBM);
		fastNoise.SetFractalOctaves(3);
		fastNoise.SetFractalLacunarity(0.7f);
		fastNoise.SetFractalGain(1f);
		return Math.Ceiling((double)fastNoise.GetNoise(x, y, z) * (double)amplitude);
	}

	public static void PaintCell(Cell C)
	{
		if (sampleSimplexNoise(C.ParentZone.ZoneID, C.X, C.Y, C.ParentZone.Z, 5) <= 0.5)
		{
			int num = Stat.Random(1, 16);
			if (num <= 2)
			{
				C.PaintColorString = "&y";
				C.PaintTile = "Tiles/tile-dirt1.png";
				C.PaintDetailColor = "y";
			}
			else if (num <= 13)
			{
				C.PaintColorString = "&y";
				C.PaintTile = DirtPicker.GetRandomGrassTile();
			}
			else if (num == 14)
			{
				C.PaintColorString = "&K";
				C.PaintTile = DirtPicker.GetRandomGrassTile();
			}
			else
			{
				C.PaintColorString = "&Y";
				C.PaintTile = "Tiles/tile-dirt1.png";
				C.PaintDetailColor = "y";
			}
			int num2 = Stat.Random(1, 5);
			if (num2 == 1)
			{
				C.PaintRenderString = ".";
			}
			if (num2 == 2)
			{
				C.PaintRenderString = ",";
			}
			if (num2 == 3)
			{
				C.PaintRenderString = "`";
			}
			if (num2 == 4)
			{
				C.PaintRenderString = "'";
			}
		}
		else if (!(sampleSimplexNoise(C.ParentZone.ZoneID, C.X, C.Y, C.ParentZone.Z, 5) >= 5.0))
		{
			C.PaintColorString = "&K";
			C.PaintTile = "Tiles/tile-dirt1.png";
			C.PaintDetailColor = "k";
		}
	}
}

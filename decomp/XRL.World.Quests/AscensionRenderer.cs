using System;
using ConsoleLib.Console;
using XRL.Core;
using XRL.UI;

namespace XRL.World.Quests;

public class AscensionRenderer : IPart
{
	[NonSerialized]
	private AscensionSystem system;

	private FastNoise fastNoise;

	private FastNoise fastNoise2;

	public double t;

	public double lastElapsed = -1.0;

	public double chargeTime;

	public double climbTime;

	public double altitude;

	public double accel;

	private bool ascending
	{
		get
		{
			if (system == null)
			{
				system = The.Game.GetSystem<AscensionSystem>();
			}
			AscensionSystem ascensionSystem = system;
			if (ascensionSystem == null)
			{
				return false;
			}
			return ascensionSystem.Stage == 2;
		}
	}

	public override bool FinalRender(RenderEvent E)
	{
		E.WantsToPaint = true;
		return true;
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (!ascending && GameObject.Validate(ParentObject))
		{
			ParentObject.Destroy();
		}
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		if (fastNoise == null)
		{
			fastNoise = new FastNoise();
			fastNoise.SetSeed(XRLCore.Core.Game.GetWorldSeed("ASCENTCOUNDS"));
			fastNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
			fastNoise.SetFrequency(0.02f);
			fastNoise.SetFractalType(FastNoise.FractalType.FBM);
			fastNoise.SetFractalOctaves(5);
			fastNoise.SetFractalLacunarity(2f);
			fastNoise.SetFractalGain(0.5f);
			fastNoise2 = new FastNoise();
			fastNoise2.SetSeed(XRLCore.Core.Game.GetWorldSeed("ASCENTCOUNDS2"));
			fastNoise2.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
			fastNoise2.SetFrequency(0.02f);
			fastNoise2.SetFractalType(FastNoise.FractalType.FBM);
			fastNoise2.SetFractalOctaves(5);
			fastNoise2.SetFractalLacunarity(2f);
			fastNoise2.SetFractalGain(0.5f);
		}
		if (lastElapsed == -1.0)
		{
			lastElapsed = XRLCore.FrameTimer.ElapsedMilliseconds;
		}
		double num = ((double)XRLCore.FrameTimer.ElapsedMilliseconds - lastElapsed) / 1000.0;
		lastElapsed = XRLCore.FrameTimer.ElapsedMilliseconds;
		double num2 = 5.0;
		double num3 = 1.0;
		double num4 = 2200.0;
		double num5 = -1400.0;
		accel = 0.0;
		double num6 = -8.0;
		int num7 = 1;
		int num8 = 2;
		int num9 = num8;
		if (!ascending)
		{
			return;
		}
		if (num9 == num7)
		{
			if (climbTime <= 0.0)
			{
				chargeTime += num;
				if (chargeTime > num2)
				{
					climbTime = num3;
					chargeTime = 0.0;
					accel = num4;
					SoundManager.PlaySound("Sounds/Interact/sfx_interact_artifact_finish_whir", 0f, 0.1f);
				}
			}
			else
			{
				climbTime -= num;
				accel += num5 * num;
				altitude += accel * num;
				if (climbTime <= 0.0)
				{
					chargeTime = 0.0;
					SoundManager.PlaySound("Sounds/Grenade/sfx_grenade_highExplosive_explode", 0f, 0.1f);
					CombatJuice.cameraShake(0.1f);
				}
			}
		}
		else if (num9 == num8)
		{
			altitude += num6 * num;
		}
		int num10 = 46;
		int num11 = 6;
		for (int i = num10; i < num10 + num11; i++)
		{
			for (int j = 0; j < 25; j++)
			{
				buffer[i, j].Char = '|';
				if (Options.UseTiles)
				{
					buffer[i, j].Tile = "Furniture/spindle-ribbon-middle.png";
				}
				buffer[i, j].SetForeground('K');
				buffer[i, j].SetDetail('k');
			}
		}
		for (int k = 0; k < 80; k++)
		{
			for (int l = 0; l < 25; l++)
			{
				if (buffer[k, l].Tile != null)
				{
					_ = buffer[k, l].Tile == "Furniture/spindle-ribbon-middle.png";
				}
				float noise = fastNoise.GetNoise((float)(k / 4) - (float)(t / 200.0), (float)((double)l + altitude));
				float noise2 = fastNoise2.GetNoise((float)(k / 4) - (float)(t / 3000.0), (float)((double)l + altitude));
				if (buffer[k, l].Tile == null && (double)noise2 > 0.3)
				{
					ConsoleChar consoleChar = buffer[k, l];
					string tile;
					if ((double)noise2 > 0.8)
					{
						tile = "Tiles2/gas_0.png";
					}
					else
					{
						tile = (((double)noise2 > 0.6) ? "Tiles2/gas_0.png" : "Tiles2/gas_0.png");
					}
					consoleChar.Tile = tile;
					buffer[k, l].SetForeground('K');
					buffer[k, l].SetBackground('k');
				}
				if ((double)noise > 0.5)
				{
					ConsoleChar consoleChar2 = buffer[k, l];
					string tile2;
					if ((double)noise > 0.8)
					{
						tile2 = "Tiles2/gas_0.png";
					}
					else
					{
						tile2 = (((double)noise > 0.6) ? "Tiles2/gas_0.png" : "Tiles2/gas_0.png");
					}
					consoleChar2.Tile = tile2;
					buffer[k, l].SetForeground('K');
					buffer[k, l].SetBackground('k');
				}
			}
		}
	}
}

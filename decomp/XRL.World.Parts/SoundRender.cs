using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using Genkit;

namespace XRL.World.Parts;

[Serializable]
public class SoundRender : SoundRendererPart
{
	public string Sound;

	private string ID;

	public float Volume = 1f;

	public float PitchVariance;

	public float CostMultiplier = 1f;

	public int CostMaximum = int.MaxValue;

	[NonSerialized]
	private int d;

	[NonSerialized]
	private static Dictionary<string, int> ClosestDistance = new Dictionary<string, int>();

	[NonSerialized]
	private static Dictionary<string, List<Location2D>> Positions = new Dictionary<string, List<Location2D>>();

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override bool Render(RenderEvent E)
	{
		try
		{
			if (!string.IsNullOrEmpty(Sound) && Sound == "sfx_torch_flame_lp")
			{
				E.WantsToPaint = true;
				d = ParentObject.DistanceTo(The.Player);
				if (!Positions.ContainsKey(Sound))
				{
					Positions.Add(Sound, new List<Location2D>());
				}
				if (ParentObject?.CurrentCell?.Location != null)
				{
					Positions[Sound].Add(ParentObject.CurrentCell?.Location);
				}
				if (!ClosestDistance.ContainsKey(Sound))
				{
					ClosestDistance.Add(Sound, d);
				}
				else if (ClosestDistance[Sound] > d)
				{
					ClosestDistance[Sound] = d;
				}
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SoundRender::Render", x);
		}
		return base.Render(E);
	}

	public override void OnPaint(ScreenBuffer buffer)
	{
		try
		{
			if (!string.IsNullOrEmpty(Sound) && ClosestDistance.ContainsKey(Sound))
			{
				ClosestDistance.Remove(Sound);
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SoundRender::OnPaint", x);
		}
	}

	public override void Render(RenderSoundEvent E)
	{
		try
		{
			if (!string.IsNullOrEmpty(Sound) && Sound == "sfx_torch_flame_lp")
			{
				if (ClosestDistance.ContainsKey(Sound) && ClosestDistance[Sound] == d && Positions.ContainsKey(Sound))
				{
					int num = 0;
					int num2 = 0;
					for (int i = 0; i < Positions[Sound].Count; i++)
					{
						num += Positions[Sound][i].X;
						num2 += Positions[Sound][i].Y;
					}
					if (Positions[Sound].Count > 0)
					{
						num /= Positions[Sound].Count;
						num2 /= Positions[Sound].Count;
					}
					num = Math.Clamp(num, 0, 79);
					num2 = Math.Clamp(num2, 0, 24);
					ClosestDistance.Remove(Sound);
					Positions[Sound].Clear();
					if (ID == null)
					{
						ID = Guid.NewGuid().ToString();
					}
					E.Buffer[num, num2]?.soundExtra.Add(ID, Sound, Volume, PitchVariance, CostMultiplier, CostMaximum);
				}
				return;
			}
		}
		catch (Exception x)
		{
			MetricsManager.LogException("SoundRender::OnPaint", x);
		}
		if (ID == null)
		{
			ID = Guid.NewGuid().ToString();
		}
		E.Char.soundExtra.Add(ID, Sound, Volume, PitchVariance, CostMultiplier, CostMaximum);
	}
}

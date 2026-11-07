using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using UnityEngine;
using XRL.Rules;
using XRL.World.Parts;

[ExecuteInEditMode]
public class MotedAir : MonoBehaviour
{
	public ParticleSystem system;

	public bool reset;

	public int count;

	private Dictionary<int, Vector3> startPositions = new Dictionary<int, Vector3>();

	private Dictionary<int, float> pulseDuration = new Dictionary<int, float>();

	private Dictionary<int, float> pulseStart = new Dictionary<int, float>();

	private Dictionary<int, Color32> color = new Dictionary<int, Color32>();

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[4];

	private bool init;

	public float t;

	private void Awake()
	{
		if (!init)
		{
			init = true;
			reset = true;
		}
	}

	private void Update()
	{
		t += Time.deltaTime;
		int particles = system.GetParticles(gos);
		if (reset || particles < count)
		{
			system.Stop();
			system.Clear();
			reset = false;
			if (count <= 0)
			{
				count = 5000;
			}
			system.Play();
			system.Emit(count);
			for (int i = 0; i < count; i++)
			{
				startPositions.Set(i, new Vector3(Stat.Random(-955, -955), Stat.Random(-195, 195)));
				pulseDuration.Set(i, Stat.Random(20, 600));
				pulseStart.Set(i, Stat.Random(0f, pulseDuration[i]));
				color.Add(i, ConsoleLib.Console.ColorUtility.colorFromChar(Crayons.GetRandomColor()[0]));
			}
			count = system.GetParticles(gos);
			for (int j = 0; j < count; j++)
			{
				gos[j].remainingLifetime = float.MaxValue;
				gos[j].startLifetime = float.MaxValue;
				gos[j].position = startPositions[j];
				gos[j].rotation = (float)Stat.Random(25, 600) / 300f;
				gos[j].startSize = (float)Stat.Random(25, 600) / 300f;
				gos[j].startColor = new Color32(color[j].r, color[j].g, color[j].b, (byte)(255f * ((1f + Mathf.Sin(MathF.PI * (t / (pulseStart[j] + pulseDuration[j])))) / 2f)));
			}
			system.SetParticles(gos);
		}
		for (int k = 0; k < count && k < particles; k++)
		{
			float f = MathF.PI * 2f * (t / pulseDuration[k]);
			float num = Mathf.Sin(MathF.PI * 2f * ((t + pulseStart[k]) / pulseDuration[k]));
			float num2 = (1f + num) / 2f;
			Mathf.Sin(f);
			gos[k].remainingLifetime = float.MaxValue;
			gos[k].position = startPositions[k];
			gos[k].rotation += (float)Stat.Random(0, 60) / 6000f;
			gos[k].startSize = (1f + num) / 2f * 1.5f;
			gos[k].startColor = new Color32(color[k].r, color[k].g, color[k].b, (byte)(255f * num2 - (float)Stat.RandomCosmetic(0, 64)));
		}
		system.SetParticles(gos);
	}
}

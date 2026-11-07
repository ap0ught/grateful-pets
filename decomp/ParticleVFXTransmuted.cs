using System;
using System.Collections.Generic;
using System.Text;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;
using XRL;
using XRL.World;
using XRL.World.Parts;

[ExecuteAlways]
public class ParticleVFXTransmuted : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public UnityEngine.GameObject backingSprite;

	public bool reset = true;

	public int count;

	public float delay;

	public float duration = 0.5f;

	public float delaycount;

	public Texture2D texture;

	public Texture2D textureOut;

	public ParticleSystem system;

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[384];

	private Dictionary<int, Vector3> startPositions = new Dictionary<int, Vector3>();

	private Dictionary<int, Color> startColors = new Dictionary<int, Color>();

	private Dictionary<int, Vector3> endPositions = new Dictionary<int, Vector3>();

	private Dictionary<int, Color> endColors = new Dictionary<int, Color>();

	private Dictionary<int, int> distances = new Dictionary<int, int>();

	private Dictionary<int, float> angles = new Dictionary<int, float>();

	public Color startColor;

	public Color startDetail;

	public Color endColor;

	public Color endDetail;

	private float t;

	public void configure(string configurationString)
	{
		string[] array = configurationString.Split(';');
		texture = SpriteManager.GetUnitySprite(array[0]).texture;
		startColor = ConsoleLib.Console.ColorUtility.ColorFromString(array[1]);
		startDetail = ConsoleLib.Console.ColorUtility.ColorFromString(array[2]);
		textureOut = SpriteManager.GetUnitySprite(array[3]).texture;
		endColor = ConsoleLib.Console.ColorUtility.ColorFromString(array[4]);
		endDetail = ConsoleLib.Console.ColorUtility.ColorFromString(array[5]);
		reset = true;
	}

	private void Awake()
	{
		reset = true;
	}

	private void Update()
	{
		if (reset)
		{
			if (backingSprite != null)
			{
				backingSprite.SetActive(value: true);
			}
			system.Stop();
			system.Clear();
			reset = false;
			startPositions.Clear();
			endPositions.Clear();
			startColors.Clear();
			endColors.Clear();
			distances.Clear();
			angles.Clear();
			count = 0;
			for (int i = 0; i < texture.width; i++)
			{
				for (int j = 0; j < texture.height; j++)
				{
					if (texture.GetPixel(i, j).a > 0f)
					{
						startPositions.Add(count, new Vector3(i, j) + new Vector3(-7.5f, -11.5f));
						startColors.Add(count, ((double)texture.GetPixel(i, j).r < 0.5) ? startColor : startDetail);
						count++;
					}
				}
			}
			int num = 0;
			for (int k = 0; k < textureOut.width; k++)
			{
				for (int l = 0; l < textureOut.height; l++)
				{
					if (textureOut.GetPixel(k, l).a > 0f)
					{
						endPositions.Add(num, new Vector3(k, l) + new Vector3(-7.5f, -11.5f));
						endColors.Add(num, ((double)textureOut.GetPixel(k, l).r < 0.5) ? endColor : endDetail);
						num++;
					}
				}
			}
			if (count < num)
			{
				for (int m = count; m < num; m++)
				{
					startPositions.Add(m, startPositions[m % count]);
					startColors.Add(m, startColors[m % count]);
				}
				count = num;
			}
			else
			{
				for (int n = num; n < count; n++)
				{
					endPositions.Add(n, endPositions[n % num]);
					endColors.Add(n, endColors[n % num]);
				}
			}
			Debug.Log(count);
			Debug.Log(num);
			for (int num2 = 0; num2 < count; num2++)
			{
				distances.Add(num2, UnityEngine.Random.Range(4, 32));
				angles.Add(num2, UnityEngine.Random.Range(-6.28f, 6.28f));
			}
			system.Play();
			system.Emit(count);
			t = 0f;
			delaycount = delay;
		}
		if (!(t < duration))
		{
			return;
		}
		if (delaycount > 0f)
		{
			bool flag = false;
			if (delaycount == delay)
			{
				flag = true;
			}
			delaycount -= Time.deltaTime;
			if (!flag)
			{
				return;
			}
		}
		else
		{
			t += Time.deltaTime;
			if (backingSprite != null && t >= duration && backingSprite.activeInHierarchy)
			{
				backingSprite?.SetActive(value: false);
			}
		}
		int particles = system.GetParticles(gos);
		for (int num3 = 0; num3 < count && num3 < particles; num3++)
		{
			_ = startPositions[num3];
			_ = endPositions[num3];
			Vector3 vector = Vector3.Lerp(startPositions[num3], endPositions[num3], t / duration);
			float num4 = duration / 2f;
			float num5 = t;
			if (num5 >= duration / 2f)
			{
				num5 -= duration / 2f;
			}
			if (t < duration / 2f)
			{
				float num6 = Mathf.Lerp(0f + angles[num3], MathF.PI * 2f + angles[num3], Easing.QuadraticEaseOut(t / duration));
				float num7 = Mathf.Lerp(0f, distances[num3], Easing.QuadraticEaseInOut(num5 / num4));
				double num8 = (double)vector.x + (double)num7 * Math.Cos(num6);
				double num9 = (double)vector.y + (double)num7 * Math.Sin(num6);
				gos[num3].position = new Vector3((float)num8, (float)num9, gos[num3].position.z);
			}
			else
			{
				float num10 = Mathf.Lerp(0f + angles[num3], MathF.PI * 2f + angles[num3], Easing.QuadraticEaseOut(t / duration));
				float num11 = Mathf.Lerp(distances[num3], 0f, Easing.QuadraticEaseIn(num5 / num4));
				double num12 = (double)vector.x + (double)num11 * Math.Cos(num10);
				double num13 = (double)vector.y + (double)num11 * Math.Sin(num10);
				gos[num3].position = new Vector3((float)num12, (float)num13, gos[num3].position.z);
			}
			gos[num3].startColor = Color.Lerp(startColors[num3], endColors[num3], t / duration);
		}
		system.SetParticles(gos);
	}

	public static void Play(XRL.World.GameObject From, XRL.World.GameObject To)
	{
		Render render = From.Render;
		Render render2 = To.Render;
		ColorChars colorChars = render.getColorChars();
		ColorChars colorChars2 = render2.getColorChars();
		StringBuilder sB = The.StringBuilder.Append(render.Tile).Append(';').Append(colorChars.foreground)
			.Append(';')
			.Append((colorChars.detail == '\0') ? 'k' : colorChars.detail)
			.Append(';')
			.Append(render2.Tile)
			.Append(';')
			.Append(colorChars2.foreground)
			.Append(';')
			.Append((colorChars2.detail == '\0') ? 'k' : colorChars2.detail);
		CombatJuice.playPrefabAnimation(To, "Deaths/DeathVFXTransmuted", From.ID, XRL.World.Event.FinalizeString(sB), null, async: true);
	}
}

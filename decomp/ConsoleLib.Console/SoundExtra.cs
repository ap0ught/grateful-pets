using System.Collections.Generic;
using UnityEngine;
using XRL.Core;
using XRL.Rules;

namespace ConsoleLib.Console;

public class SoundExtra : IConsoleCharExtra
{
	public struct SoundInfo(string ID, string Sound, float Volume = 1f, float PitchVariance = 0f, float CostMultiplier = 1f, int CostMaximum = int.MaxValue)
	{
		public string ID = ID;

		public string Sound = Sound;

		public float Volume = Volume;

		public float PitchVariance = PitchVariance;

		public float CostMultiplier = CostMultiplier;

		public int CostMaximum = CostMaximum;

		public bool SameAs(SoundInfo info)
		{
			return ID == info.ID;
		}
	}

	private static Dictionary<string, AudioSource> PlayingSounds = new Dictionary<string, AudioSource>();

	private static Dictionary<string, AudioSource> NextPlayingSounds = new Dictionary<string, AudioSource>();

	public List<SoundInfo> Sounds = new List<SoundInfo>();

	public int Distance;

	public bool Occluded;

	public static AnimationCurve distanceCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	private static GameObject imposterRoot;

	private static Queue<AudioSource> pool = new Queue<AudioSource>();

	public void SetDistance(int Distance)
	{
		this.Distance = Distance;
	}

	public void SetOccluded(bool Occluded)
	{
		this.Occluded = Occluded;
	}

	public void Add(string ID, string Sound, float Volume = 1f, float PitchVariance = 1f, float CostMultiplier = 1f, int CostMaximum = int.MaxValue)
	{
		Sounds.Add(new SoundInfo(ID, Sound, Volume, PitchVariance, CostMultiplier, CostMaximum));
	}

	public void CopyFrom(SoundExtra extra)
	{
		if (Sounds.Count > 0)
		{
			Sounds.Clear();
		}
		if (extra != null)
		{
			if (extra.Sounds.Count > 0)
			{
				Sounds.AddRange(extra.Sounds);
			}
			Distance = extra.Distance;
			Occluded = extra.Occluded;
		}
		else
		{
			Distance = 0;
			Occluded = false;
		}
	}

	public override IConsoleCharExtra Copy()
	{
		SoundExtra soundExtra = new SoundExtra();
		soundExtra.Sounds.AddRange(Sounds);
		soundExtra.Distance = Distance;
		soundExtra.Occluded = Occluded;
		return soundExtra;
	}

	public override void Clear(bool overtyping)
	{
		if (!overtyping)
		{
			Sounds.Clear();
		}
	}

	public void UpdateSourceFromInfo(AudioSource source, SoundInfo info, ex3DSprite2 sprite, bool start = false)
	{
		float num = 1f;
		float volume = info.Volume;
		int num2 = Mathf.RoundToInt((float)Distance * info.CostMultiplier);
		if (num2 > info.CostMaximum)
		{
			num2 = info.CostMaximum;
		}
		if (Occluded)
		{
			if (num2 == 9999)
			{
				volume = 0f;
			}
			else
			{
				num = 0.15f + 0.55f * ((float)(80 - num2) / 80f);
				volume *= Mathf.Pow(0.9f, num2);
			}
		}
		else if (num2 > 40)
		{
			volume = 0f;
		}
		else
		{
			num = 1f;
			volume *= Mathf.Pow(0.9f, num2);
		}
		if (source.gameObject.transform.position != sprite.transform.position)
		{
			source.gameObject.transform.position = sprite.transform.position;
		}
		source.GetComponent<AudioLowPassFilter>().cutoffFrequency = 22000f * num * volume;
		source.volume = volume;
		if (!source.isActiveAndEnabled)
		{
			source.gameObject.SetActive(value: true);
		}
		if (!source.isPlaying & start)
		{
			source.Play();
		}
	}

	public override void AfterRender(int x, int y, ConsoleChar ch, ex3DSprite2 sprite, ScreenBuffer buffer)
	{
		if (Globals.EnableSound)
		{
			for (int i = 0; i < Sounds.Count; i++)
			{
				string iD = Sounds[i].ID;
				if (NextPlayingSounds.ContainsKey(iD))
				{
					continue;
				}
				if (PlayingSounds.TryGetValue(iD, out var value))
				{
					NextPlayingSounds.Add(iD, value);
					UpdateSourceFromInfo(value, Sounds[i], sprite);
					PlayingSounds.Remove(iD);
					continue;
				}
				string sound = Sounds[i].Sound;
				if (SoundManager.TryGetClipSet(sound, out var Set))
				{
					if (Set.Initialized)
					{
						AudioSource audioSource = next(Sounds[i]);
						UpdateSourceFromInfo(audioSource, Sounds[i], sprite, start: true);
						NextPlayingSounds.Add(iD, audioSource);
					}
					else
					{
						SoundManager.PreloadClipSet(sound);
					}
				}
			}
		}
		if (x != 79 || y != 24)
		{
			return;
		}
		Dictionary<string, AudioSource> playingSounds = PlayingSounds;
		PlayingSounds = NextPlayingSounds;
		foreach (KeyValuePair<string, AudioSource> item in playingSounds)
		{
			free(item.Value);
		}
		playingSounds.Clear();
		NextPlayingSounds = playingSounds;
	}

	private static void free(AudioSource source)
	{
		if (source != null)
		{
			source.Stop();
			source.gameObject.SetActive(value: false);
			pool.Enqueue(source);
		}
	}

	private static AudioSource next(SoundInfo info)
	{
		if (info.Sound == null)
		{
			return null;
		}
		if (pool.Count == 0)
		{
			AudioSource audioSource = SoundManager.CreateAudioSource(info.Volume, 1f, info.PitchVariance);
			audioSource.loop = true;
			if (imposterRoot == null)
			{
				imposterRoot = new GameObject("TileSoundRoot");
			}
			audioSource.transform.parent = imposterRoot.transform;
			pool.Enqueue(audioSource);
		}
		AudioSource audioSource2 = pool.Dequeue();
		audioSource2.gameObject.name = info.ID;
		audioSource2.clip = SoundManager.GetClipSet(info.Sound).Next().Clip;
		audioSource2.pitch = 1f - ((float)Stat.Rnd5.NextDouble() * 2f - 1f) * info.PitchVariance;
		audioSource2.rolloffMode = AudioRolloffMode.Custom;
		audioSource2.dopplerLevel = 0f;
		audioSource2.minDistance = 0f;
		audioSource2.maxDistance = 10000f;
		audioSource2.SetCustomCurve(AudioSourceCurveType.CustomRolloff, distanceCurve);
		audioSource2.spatialBlend = 0.5f;
		audioSource2.gameObject.SetActive(value: true);
		return audioSource2;
	}
}

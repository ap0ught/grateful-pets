using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Genkit;
using QupKit;
using UnityEngine;
using XRL;
using XRL.Collections;
using XRL.Core;
using XRL.Messages;
using XRL.Rules;
using XRL.Sound;
using XRL.UI;
using XRL.Wish;
using XRL.World;

[HasModSensitiveStaticCache]
[HasWishCommand]
public static class SoundManager
{
	public class ModSoundFile
	{
		public ModFile File;

		public AudioType Type;

		public ModSoundFile(ModFile File, AudioType Type)
		{
			this.File = File;
			this.Type = Type;
		}
	}

	private static Dictionary<string, int[]> ClipFrames = new Dictionary<string, int[]>();

	public static bool WriteSoundsToLog;

	private static SoundRequestLog[] RequestLogs = (from _ in Enumerable.Range(0, 100)
		select new SoundRequestLog()).ToArray();

	private static int RequestLogNextIndex = 0;

	public static Queue<SoundRequest> RequestPool = new Queue<SoundRequest>();

	public static Queue<SoundRequest> SoundRequests = new Queue<SoundRequest>();

	private static List<SoundRequest> Requests = new List<SoundRequest>();

	public static AnimationCurve distanceCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));

	private static Queue<SoundRequest> DelayedRequests = new Queue<SoundRequest>();

	public static MusicSourceCollection MusicSources = new MusicSourceCollection();

	public static UnityEngine.GameObject UISource = null;

	public static Queue<AudioSource> UIAudioSources = null;

	public static int UI_AUDIO_SOURCES = 24;

	private static Queue<UnityEngine.GameObject> AudioSourcePool = new Queue<UnityEngine.GameObject>();

	private static Queue<UnityEngine.GameObject> PlayingAudioSources = new Queue<UnityEngine.GameObject>();

	private static Dictionary<string, string> ClipNames = new Dictionary<string, string>();

	public static float MasterVolume = 1f;

	public static float SoundVolume = 1f;

	public static float MusicVolume = 1f;

	private static HashSet<string> soundsPlayedThisFrame = new HashSet<string>();

	public static StringMap<AudioEntrySet> SetMap = new StringMap<AudioEntrySet>();

	public static ReaderWriterLockSlim SetMapLock = new ReaderWriterLockSlim();

	private static Rack<char> KeyBuffer = new Rack<char>(128);

	public static string MusicTrack
	{
		get
		{
			if (!MusicSources.TryGetValue("music", out var Value))
			{
				return null;
			}
			return Value.Track;
		}
	}

	[WishCommand("soundlog", null)]
	public static bool ShowSoundLog()
	{
		return ShowSoundLog("20");
	}

	[WishCommand("soundlog", null)]
	public static bool ShowSoundLog(string rest)
	{
		int result;
		if (rest == "all")
		{
			result = RequestLogs.Length;
		}
		if (!int.TryParse(rest, out result))
		{
			result = 20;
		}
		int num = RequestLogNextIndex - 1 + RequestLogs.Length;
		StringBuilder stringBuilder = XRL.World.Event.NewStringBuilder();
		stringBuilder.Append($"Log of last {result} sounds:\n\n");
		for (int i = 0; i < result; i++)
		{
			SoundRequestLog soundRequestLog = RequestLogs[(num - i) % RequestLogs.Length];
			if (soundRequestLog.timePlayed > 0f)
			{
				stringBuilder.Append(soundRequestLog.ToString()).Append('\n');
			}
		}
		Popup.Show(stringBuilder.ToString());
		return true;
	}

	[ModSensitiveCacheInit]
	public static void ClearCache()
	{
		ClipFrames = new Dictionary<string, int[]>();
	}

	public static void StopMusic(string Channel = "music", bool Crossfade = true, float CrossfadeDuration = 12f)
	{
		PlayMusic(null, Channel, Crossfade, CrossfadeDuration);
	}

	public static void PlayMusic(string Track, string Channel = "music", bool Crossfade = true, float CrossfadeDuration = 12f, float VolumeAttenuation = 1f, Action OnPlay = null, bool Loop = true)
	{
		if (RequestPool.Count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Track);
			OnPlay?.Invoke();
			return;
		}
		SoundRequest soundRequest = RequestPool.Dequeue();
		soundRequest.Channel = Channel;
		soundRequest.Clip = Track;
		soundRequest.Crossfade = Crossfade;
		soundRequest.CrossfadeDuration = CrossfadeDuration;
		soundRequest.Volume = VolumeAttenuation;
		soundRequest.Type = SoundRequest.SoundRequestType.Music;
		soundRequest.OnPlay = OnPlay;
		soundRequest.Loop = Loop;
		if (!BeforePlayMusicEvent.Check(soundRequest))
		{
			RequestPool.Enqueue(soundRequest);
			OnPlay?.Invoke();
			return;
		}
		lock (SoundRequests)
		{
			SoundRequests.Enqueue(soundRequest);
		}
	}

	public static void PlayWorldSound(string Clip, int Distance, bool Occluded, float VolumeIntensity, Location2D Cell, float PitchVariance = 0f, float Delay = 0f, float Pitch = 1f)
	{
		PlayWorldSound(Clip, Distance, Occluded, VolumeIntensity, Cell.Point, PitchVariance, Delay, Pitch);
	}

	public static void PlayWorldSound(string Clip, int Distance, bool Occluded, float VolumeIntensity, Point2D Cell, float PitchVariance = 0f, float Delay = 0f, float Pitch = 1f)
	{
		int count = RequestPool.Count;
		if (count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Clip);
			return;
		}
		VolumeIntensity *= GetSoundFrameVolume(Clip);
		if (Occluded)
		{
			if (Distance == 9999 || (float)Distance > 40f * VolumeIntensity)
			{
				return;
			}
			SoundRequest soundRequest = RequestPool.Dequeue();
			if (soundRequest != null)
			{
				soundRequest.Cell = Cell;
				soundRequest.Clip = Clip;
				soundRequest.Type = SoundRequest.SoundRequestType.Spatial;
				soundRequest.LowPass = 0.15f + 0.55f * ((float)(80 - Distance) / 80f);
				soundRequest.Volume = VolumeIntensity * ((float)((40 - Distance) * (40 - Distance)) / 1600f);
				soundRequest.PitchVariance = PitchVariance;
				soundRequest.Delay = Delay;
				soundRequest.Pitch = Pitch;
				lock (SoundRequests)
				{
					SoundRequests.Enqueue(soundRequest);
					return;
				}
			}
			Debug.LogWarning("null from request pool even though count was " + count + " for " + Clip);
		}
		else
		{
			if (Distance > 40)
			{
				return;
			}
			SoundRequest soundRequest2 = RequestPool.Dequeue();
			if (soundRequest2 != null)
			{
				soundRequest2.Cell = Cell;
				soundRequest2.Clip = Clip;
				soundRequest2.Type = SoundRequest.SoundRequestType.Spatial;
				soundRequest2.LowPass = 1f;
				soundRequest2.Volume = VolumeIntensity * ((float)((40 - Distance) * (40 - Distance)) / 1600f);
				soundRequest2.PitchVariance = PitchVariance;
				soundRequest2.Delay = Delay;
				soundRequest2.Pitch = Pitch;
				lock (SoundRequests)
				{
					SoundRequests.Enqueue(soundRequest2);
					return;
				}
			}
			Debug.LogWarning("null from request pool even though count was " + count + " for " + Clip);
		}
	}

	public static void PlayUISound(string Clip, float Volume = 1f, bool Combat = false, bool Interface = false, SoundRequest.SoundEffectType Effect = SoundRequest.SoundEffectType.None)
	{
		if (Interface)
		{
			if (Effect != SoundRequest.SoundEffectType.None)
			{
				MetricsManager.LogWarning("You can't use a spatial effect type on an interface sound.");
			}
			else
			{
				Effect = SoundRequest.SoundEffectType.None;
			}
		}
		if (!Clip.IsNullOrEmpty() && Options.Sound && (!Combat || Options.UseCombatSounds) && (!Interface || Options.UseInterfaceSounds))
		{
			if (Interface)
			{
				Volume *= Globals.InterfaceVolume;
			}
			if (Combat)
			{
				Volume *= Globals.CombatVolume;
			}
			PlaySound(Clip, 0f, Volume, 1f, Effect);
		}
	}

	public static void PlaySound(string Clip, float PitchVariance = 0f, float Volume = 1f, float Pitch = 1f, SoundRequest.SoundEffectType Effect = SoundRequest.SoundEffectType.None, float Delay = 0f)
	{
		if (Effect != SoundRequest.SoundEffectType.None)
		{
			Volume *= GetSoundFrameVolume(Clip);
			if (Volume <= 0f)
			{
				return;
			}
			Effect = SoundRequest.SoundEffectType.None;
		}
		if (RequestPool.Count == 0)
		{
			Debug.LogWarning("sound request pool was empty for request " + Clip);
			return;
		}
		SoundRequest soundRequest = RequestPool.Dequeue();
		soundRequest.Clip = Clip;
		soundRequest.Type = SoundRequest.SoundRequestType.Sound;
		soundRequest.Pitch = Pitch;
		soundRequest.PitchVariance = PitchVariance;
		soundRequest.Volume = Volume;
		soundRequest.Effect = Effect;
		soundRequest.Delay = Delay;
		lock (SoundRequests)
		{
			SoundRequests.Enqueue(soundRequest);
		}
	}

	public static List<SoundRequest> GetRequests()
	{
		if (Requests.Count > 0)
		{
			Requests.Clear();
		}
		if (SoundRequests.Count > 0)
		{
			lock (SoundRequests)
			{
				while (SoundRequests.Count > 0)
				{
					SoundRequest soundRequest = SoundRequests.Dequeue();
					if (soundRequest.Delay > 0f)
					{
						soundRequest.Delay -= Time.deltaTime;
					}
					if (soundRequest.Delay <= 0f)
					{
						Requests.Add(soundRequest);
						RequestLogs[RequestLogNextIndex].Set(Time.fixedUnscaledTime, soundRequest);
						RequestLogNextIndex = (RequestLogNextIndex + 1) % RequestLogs.Length;
					}
					else
					{
						DelayedRequests.Enqueue(soundRequest);
					}
				}
				if (DelayedRequests.Count > 0)
				{
					Queue<SoundRequest> soundRequests = SoundRequests;
					SoundRequests = DelayedRequests;
					DelayedRequests = soundRequests;
				}
			}
		}
		return Requests;
	}

	public static void UnloadClipSet(string ClipID)
	{
		if (TryGetClipSet(ClipID, out var Set))
		{
			Set.Unload();
		}
	}

	public static MusicSource RequireMusicSource(string Channel, bool Reset = false)
	{
		if (!MusicSources.TryGetValue(Channel, out var Value))
		{
			Value = (MusicSources[Channel] = ObjectPool<MusicSource>.Checkout());
		}
		else if (!Reset)
		{
			return Value;
		}
		Value.Channel = Channel;
		Value.Type = (Channel.StartsWith("ambient_bed") ? MusicType.Ambience : MusicType.General);
		Value.Enabled = true;
		Value.TargetVolume = 0f;
		Value.Track = null;
		Value.SetMusicBackground(Options.MusicBackground);
		return Value;
	}

	public static void Init()
	{
		RequireMusicSource("music");
		RequireMusicSource("ambient_bed");
		RequireMusicSource("ambient_bed_2");
		RequireMusicSource("ambient_bed_3");
		if (UIAudioSources == null)
		{
			UIAudioSources = new Queue<AudioSource>();
			for (int i = 0; i < UI_AUDIO_SOURCES; i++)
			{
				UnityEngine.GameObject gameObject = new UnityEngine.GameObject();
				gameObject.AddComponent<AudioSource>();
				AudioSource component = gameObject.GetComponent<AudioSource>();
				gameObject.transform.position = new Vector3(0f, 0f, 1f);
				gameObject.transform.parent = UnityEngine.GameObject.Find("AudioListener").transform;
				gameObject.name = "UISound";
				component.priority = 16;
				UIAudioSources.Enqueue(component);
			}
		}
		for (int j = 0; j < 256; j++)
		{
			RequestPool.Enqueue(new SoundRequest());
		}
	}

	private static void setClipName(string channel, string value)
	{
		ClipNames[channel] = value;
	}

	private static string getClipName(string channel)
	{
		if (ClipNames.TryGetValue(channel, out var value))
		{
			return value;
		}
		return null;
	}

	public static async UniTask SetChannelTrack(string Track, string Channel, bool Crossfade, float CrossfadeDuration = 12f, float VolumeAttenuation = 1f, int Priority = 0, Action OnPlay = null, bool Loop = true)
	{
		MusicSource source = RequireMusicSource(Channel);
		AudioSource audio = source.Audio;
		if (source.Track == Track && audio.isPlaying)
		{
			source.TargetVolume = VolumeAttenuation;
			return;
		}
		if (Crossfade && audio.isPlaying && audio.volume > 0f)
		{
			MusicSources.Remove(Channel);
			MusicSource musicSource = source;
			musicSource.Fade.StartFade(CrossfadeDuration);
			musicSource.Channel = "Crossfade";
			source = RequireMusicSource(Channel, Reset: true);
			audio = source.Audio;
		}
		source.Track = Track;
		if (Track == null)
		{
			audio.Stop();
			source.Reset();
			return;
		}
		if (source.Enabled)
		{
			audio.Stop();
			audio.volume = 0f;
			audio.loop = false;
			if (!TryGetClipSet(Track, out var set) || set.Count == 0)
			{
				if (WriteSoundsToLog)
				{
					MessageQueue.AddPlayerMessage(Channel + ": " + Track + " (Wasn't found)");
				}
				MetricsManager.LogWarning("SoundManager::Missing music track: " + Track);
				source.Reset();
				return;
			}
			if (!set.Initialized)
			{
				await set.Load();
			}
			if (!set.Shuffle)
			{
				set.Index = 0;
			}
			source.EntrySet = set;
			source.EventTime = AudioSettings.dspTime;
		}
		source.TargetVolume = VolumeAttenuation;
		source.SetAudioVolume(Crossfade ? 0f : VolumeAttenuation);
		source.Loop = Loop;
		source.FirstPlay = true;
		OnPlay?.Invoke();
		if (WriteSoundsToLog)
		{
			MessageQueue.AddPlayerMessage(Channel + ": " + Track);
		}
	}

	public static async UniTask _PlaySound(string Name, float Volume, float Pitch = 1f, SoundRequest.SoundEffectType Effect = SoundRequest.SoundEffectType.None)
	{
		AudioEntry audioEntry = GetClipSet(Name, Preload: true)?.Next();
		if (audioEntry == null)
		{
			if (WriteSoundsToLog)
			{
				MessageQueue.AddPlayerMessage(Name + " (missing)");
				MetricsManager.LogWarning("Missing sound: " + Name);
			}
			return;
		}
		AudioClip audioClip = await audioEntry.GetClip();
		if (!audioClip)
		{
			if (WriteSoundsToLog)
			{
				MessageQueue.AddPlayerMessage(Name + " (invalid)");
				MetricsManager.LogWarning("Invalid sound: " + Name);
			}
			return;
		}
		if (WriteSoundsToLog)
		{
			MessageQueue.AddPlayerMessage(Name);
		}
		if (Effect == SoundRequest.SoundEffectType.None)
		{
			AudioSource audioSource = UIAudioSources.Dequeue();
			UIAudioSources.Enqueue(audioSource);
			audioSource.pitch = Pitch;
			audioSource.PlayOneShot(audioClip, SoundVolume * Volume);
			return;
		}
		UnityEngine.GameObject audioSourceFromPool = getAudioSourceFromPool();
		AudioSource component = audioSourceFromPool.GetComponent<AudioSource>();
		AudioLowPassFilter component2 = audioSourceFromPool.GetComponent<AudioLowPassFilter>();
		component.clip = audioClip;
		component.volume = Volume;
		component.pitch = 1f;
		component.transform.position = UIAudioSources.Peek().transform.position;
		component2.cutoffFrequency = 22000f;
		AudioSourceEffect audioSourceEffect = audioSourceFromPool.GetComponent<AudioSourceEffect>();
		if (audioSourceEffect == null)
		{
			audioSourceEffect = audioSourceFromPool.AddComponent<AudioSourceEffect>();
		}
		audioSourceEffect.Effect = Effect;
		PlayingAudioSources.Enqueue(audioSourceFromPool);
		component.Play();
	}

	public static UnityEngine.GameObject getAudioSourceFromPool()
	{
		UnityEngine.GameObject gameObject;
		if (AudioSourcePool.Count > 0)
		{
			gameObject = AudioSourcePool.Dequeue();
		}
		else
		{
			gameObject = new UnityEngine.GameObject();
			gameObject.transform.position = new Vector3(0f, 0f, 1f);
			gameObject.name = "PooledWorldSound";
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			gameObject.AddComponent<AudioLowPassFilter>();
			audioSource.dopplerLevel = 0f;
			audioSource.minDistance = 0f;
			audioSource.maxDistance = 10000f;
			audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, distanceCurve);
			audioSource.spatialBlend = 0.5f;
			audioSource.priority = 128;
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		AudioSourceEffect.PoolReset(gameObject.GetComponent<AudioSource>());
		return gameObject;
	}

	public static async UniTask _PlayWorldSound(string Name, float Volume, float LowPass, float Pitch, float PitchVariance, Point2D Cell)
	{
		AudioEntry audioEntry = GetClipSet(Name, Preload: true)?.Next();
		if (audioEntry == null)
		{
			if (WriteSoundsToLog)
			{
				MessageQueue.AddPlayerMessage(Name + " (missing)");
				MetricsManager.LogWarning("Missing sound: " + Name);
			}
			return;
		}
		AudioClip audioClip = await audioEntry.GetClip();
		if (!audioClip)
		{
			if (WriteSoundsToLog)
			{
				MessageQueue.AddPlayerMessage(Name + " (invalid)");
				MetricsManager.LogWarning("Invalid sound: " + Name);
			}
			return;
		}
		if (WriteSoundsToLog)
		{
			MessageQueue.AddPlayerMessage(Name);
		}
		UnityEngine.GameObject audioSourceFromPool = getAudioSourceFromPool();
		AudioSource component = audioSourceFromPool.GetComponent<AudioSource>();
		AudioLowPassFilter component2 = audioSourceFromPool.GetComponent<AudioLowPassFilter>();
		component.clip = audioClip;
		component.volume = Volume;
		component.pitch = Pitch - ((float)Stat.Rnd5.NextDouble() * 2f - 1f) * PitchVariance;
		component.transform.position = GameManager.Instance.GetCellCenter(Cell.x, Cell.y);
		component2.cutoffFrequency = 22000f * LowPass * Volume;
		PlayingAudioSources.Enqueue(audioSourceFromPool);
		component.Play();
	}

	public static AudioSource CreateAudioSource(float volume, float lowPass, float pitchVariance)
	{
		UnityEngine.GameObject gameObject;
		if (AudioSourcePool.Count > 0)
		{
			gameObject = AudioSourcePool.Dequeue();
		}
		else
		{
			gameObject = new UnityEngine.GameObject();
			gameObject.transform.position = new Vector3(0f, 0f, 1f);
			gameObject.name = "PooledWorldSound";
			gameObject.AddComponent<AudioSource>().priority = 128;
			gameObject.AddComponent<AudioLowPassFilter>();
			UnityEngine.Object.DontDestroyOnLoad(gameObject);
		}
		AudioSource component = gameObject.GetComponent<AudioSource>();
		AudioLowPassFilter component2 = gameObject.GetComponent<AudioLowPassFilter>();
		AudioSourceEffect.PoolReset(gameObject.GetComponent<AudioSource>());
		component.volume = volume;
		component.pitch = 1f - ((float)Stat.Rnd5.NextDouble() * 2f - 1f) * pitchVariance;
		component2.cutoffFrequency = 22000f * lowPass * volume;
		return component;
	}

	public static void Update()
	{
		soundsPlayedThisFrame.Clear();
		if (!Globals.EnableSound && PlayingAudioSources.Count > 0)
		{
			while (PlayingAudioSources.Count > 0)
			{
				UnityEngine.GameObject gameObject = PlayingAudioSources.Dequeue();
				gameObject.GetComponent<AudioSource>().Stop();
				AudioSourcePool.Enqueue(gameObject);
			}
		}
		AudioListener.volume = MasterVolume;
		MusicSources.Update();
		while (PlayingAudioSources.Count > 0 && !PlayingAudioSources.Peek().GetComponent<AudioSource>().isPlaying)
		{
			AudioSourcePool.Enqueue(PlayingAudioSources.Dequeue());
		}
		GetRequests();
		for (int i = 0; i < Requests.Count; i++)
		{
			SoundRequest soundRequest = Requests[i];
			if (soundRequest.Type == SoundRequest.SoundRequestType.Spatial || soundRequest.Type == SoundRequest.SoundRequestType.Sound)
			{
				if (soundRequest.Clip == null || soundsPlayedThisFrame.Contains(soundRequest.Clip))
				{
					continue;
				}
				soundsPlayedThisFrame.Add(soundRequest.Clip);
			}
			if (soundRequest.Type == SoundRequest.SoundRequestType.Spatial)
			{
				if (Options.Sound)
				{
					_PlayWorldSound(soundRequest.Clip, soundRequest.Volume, soundRequest.LowPass, soundRequest.Pitch, soundRequest.PitchVariance, soundRequest.Cell);
				}
			}
			else if (soundRequest.Type == SoundRequest.SoundRequestType.Sound)
			{
				if (Options.Sound)
				{
					_PlaySound(soundRequest.Clip, soundRequest.Volume, soundRequest.Pitch, soundRequest.Effect);
				}
			}
			else if (soundRequest.Type == SoundRequest.SoundRequestType.Music)
			{
				SetChannelTrack(soundRequest.Clip, soundRequest.Channel, soundRequest.Crossfade, soundRequest.CrossfadeDuration, soundRequest.Volume, 0, soundRequest.OnPlay, soundRequest.Loop);
			}
		}
		for (int j = 0; j < Requests.Count; j++)
		{
			if (Requests[j] == null)
			{
				Debug.LogError("Requests had a null entry!");
				continue;
			}
			Requests[j].OnPlay = null;
			Requests[j].Loop = true;
			RequestPool.Enqueue(Requests[j]);
		}
		Requests.Clear();
	}

	[ModSensitiveCacheInit]
	public static void Initialize()
	{
		ClipFrames.Clear();
		List<string> list = new List<string>();
		int num = 0;
		foreach (ModInfo activeMod in ModManager.ActiveMods)
		{
			foreach (ModFile file in activeMod.Files)
			{
				if (file.Type == ModFileType.Audio)
				{
					list.Add(file.OriginalName);
					num++;
				}
			}
		}
		SoundDatabaseScriptable soundDatabaseScriptable = Resources.Load<SoundDatabaseScriptable>("SoundDatabase");
		if ((bool)soundDatabaseScriptable)
		{
			list.AddRange(soundDatabaseScriptable.Paths);
		}
		else
		{
			MetricsManager.LogError("Sound database not found, no sounds will be played");
		}
		MetricsManager.LogInfo($"Audio files: {list.Count - num} base, {num} mod");
		SetMapLock.EnterWriteLock();
		try
		{
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				string text = list[i];
				bool flag = i < num;
				if (!TryGetAudioVariant(text, out var Key, out var Variant))
				{
					continue;
				}
				if (!SetMap.TryGetValue(Key, out var Value))
				{
					string text2 = text;
					if (Variant != -1)
					{
						int num2 = text2.LastIndexOf('-');
						if (num2 != -1)
						{
							text2 = text2.Substring(0, num2);
						}
					}
					Value = (flag ? new ModAudioEntrySet() : new AudioEntrySet());
					Value.Initialize(text2);
					SetMap.Add(Key, Value);
				}
				if (Value.IndexOfVariant(Variant) == -1)
				{
					AudioEntry audioEntry = (flag ? new ModAudioEntry() : new AudioEntry());
					audioEntry.Path = text;
					audioEntry.Variant = Variant;
					Value.Add(audioEntry);
				}
			}
		}
		finally
		{
			SetMapLock.ExitWriteLock();
		}
		if (!(Options.GetOption("OptionDisableSoundPreload") != "Yes"))
		{
			return;
		}
		Debug.Log("Preloading sounds...");
		SetMapLock.EnterReadLock();
		try
		{
			foreach (KeyValuePair<string, AudioEntrySet> item in SetMap)
			{
				item.Deconstruct(out var _, out var value);
				value.Load().Forget();
			}
		}
		finally
		{
			SetMapLock.ExitReadLock();
		}
	}

	public static bool TryGetClipSet(string Name, out AudioEntrySet Set)
	{
		SetMapLock.EnterReadLock();
		try
		{
			if (!SetMap.TryGetValue(Name, out Set))
			{
				ReadOnlySpan<char> soundName = GetSoundName(Name);
				if (!SetMap.TryGetValue(soundName, out Set))
				{
					return false;
				}
			}
		}
		finally
		{
			SetMapLock.ExitReadLock();
		}
		return true;
	}

	public static ReadOnlySpan<char> GetSoundName(string Path)
	{
		if (Path.IsNullOrEmpty())
		{
			return ReadOnlySpan<char>.Empty;
		}
		ReadOnlySpan<char> span = Path;
		int num = span.LastIndexOf('.');
		if (num != -1)
		{
			span = span.Slice(0, num);
		}
		num = span.LastIndexOf('/');
		if (num != -1)
		{
			span = span.Slice(num + 1);
		}
		num = span.LastIndexOf('\\');
		if (num != -1)
		{
			span = span.Slice(num + 1);
		}
		int length = span.Length;
		char[] array = KeyBuffer.GetArray(length);
		for (int i = 0; i < length; i++)
		{
			array[i] = char.ToLowerInvariant(span[i]);
		}
		return array.AsSpan(0, length);
	}

	public static bool TryGetAudioVariant(string Path, out ReadOnlySpan<char> Key, out int Variant)
	{
		Variant = -1;
		if (Path.IsNullOrEmpty())
		{
			Key = ReadOnlySpan<char>.Empty;
			return false;
		}
		Key = GetSoundName(Path);
		int num = Key.Length - 1;
		int num2 = 0;
		while (num >= 0)
		{
			char c = Key[num];
			if (char.IsDigit(c))
			{
				num2++;
				num--;
				continue;
			}
			if (num2 > 0 && !char.IsLetter(c) && int.TryParse(Key.Slice(num + 1, num2), out var result))
			{
				Key = Key.Slice(0, num);
				Variant = result;
			}
			break;
		}
		return true;
	}

	public static AudioType GetAudioTypeFromFile(string path)
	{
		if (path.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
		{
			return AudioType.WAV;
		}
		if (path.EndsWith(".aiff", StringComparison.OrdinalIgnoreCase))
		{
			return AudioType.AIFF;
		}
		if (path.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
		{
			return AudioType.OGGVORBIS;
		}
		if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
		{
			return AudioType.MPEG;
		}
		return AudioType.UNKNOWN;
	}

	public static void PreloadClipSet(string Name)
	{
		if (TryGetClipSet(Name, out var set) && !set.Initialized)
		{
			UniTask.RunOnThreadPool(async () =>
			{
				await The.UiContext;
				set.Load().Forget();
			});
		}
	}

	public static AudioEntrySet GetClipSet(string Name, bool Preload = false)
	{
		if (!TryGetClipSet(Name, out var Set))
		{
			return null;
		}
		if (Preload && !Set.Initialized)
		{
			Set.Load();
		}
		return Set;
	}

	public static float GetSoundFrameVolume(string Clip)
	{
		if (Clip == null)
		{
			return 0f;
		}
		float num = 1f;
		int currentFrameAtFPS = XRLCore.GetCurrentFrameAtFPS(50);
		if (ClipFrames.TryGetValue(Clip, out var value))
		{
			if (value[0] >= currentFrameAtFPS - 1)
			{
				value[0] = currentFrameAtFPS;
				int num2 = ++value[1];
				if (num2 >= 5)
				{
					return 0f;
				}
				num /= (float)(num2 * num2);
			}
			else
			{
				value[0] = currentFrameAtFPS;
				value[1] = 1;
			}
		}
		else
		{
			ClipFrames[Clip] = new int[2] { currentFrameAtFPS, 1 };
		}
		return num;
	}
}

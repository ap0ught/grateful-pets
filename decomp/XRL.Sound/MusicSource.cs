using QupKit;
using UnityEngine;
using XRL.Core;

namespace XRL.Sound;

public class MusicSource : ObjectPool<MusicSource>
{
	private string _Channel;

	private bool _Enabled;

	public GameObject Object;

	public AudioEntrySet EntrySet;

	public AudioSource Audio;

	public AudioSource Buffer;

	public FadeAway Fade;

	public MusicType Type;

	public string Track;

	public float TargetVolume;

	public double EventTime;

	public bool Loop = true;

	public bool FirstPlay = true;

	public string Channel
	{
		get
		{
			return _Channel;
		}
		set
		{
			Object.name = (_Channel = value);
		}
	}

	public bool Enabled
	{
		get
		{
			return _Enabled;
		}
		set
		{
			if (_Enabled != value)
			{
				_Enabled = value;
				Audio.enabled = _Enabled;
				Buffer.enabled = _Enabled;
			}
		}
	}

	public MusicSource()
	{
		Object = new GameObject();
		Object.transform.position = new Vector3(0f, 0f, 1f);
		Object.transform.parent = GameObject.Find("AudioListener").transform;
		Audio = Object.AddComponent<AudioSource>();
		Audio.volume = 0f;
		Audio.priority = 0;
		Buffer = Object.AddComponent<AudioSource>();
		Buffer.volume = 0f;
		Buffer.priority = 0;
		Fade = Object.AddComponent<FadeAway>();
		Fade.enabled = false;
		Fade.Source = this;
		EventTime = -1.0;
	}

	public void Reset()
	{
		Audio.clip = null;
		Audio.volume = 0f;
		Buffer.clip = null;
		Buffer.volume = 0f;
		Track = null;
		EntrySet = null;
		EventTime = -1.0;
		TargetVolume = 0f;
	}

	public void SetMusicBackground(bool State)
	{
		if (State != Audio.ignoreListenerPause)
		{
			Audio.ignoreListenerPause = State;
			Audio.enabled = false;
			Audio.enabled = true;
		}
	}

	public void SetAudioVolume(float Volume)
	{
		if (Type == MusicType.General)
		{
			if (Globals.EnableMusic)
			{
				Audio.volume = Volume * SoundManager.MusicVolume;
			}
		}
		else if (Type == MusicType.Ambience && Globals.EnableAmbient)
		{
			Audio.volume = Volume * Globals.AmbientVolume;
		}
	}

	public void StopFade()
	{
		SetAudioVolume(TargetVolume);
	}

	public void Update()
	{
		float num = 0f;
		if (Type == MusicType.General)
		{
			if (!Globals.EnableMusic)
			{
				Enabled = false;
				return;
			}
			Enabled = true;
			num = TargetVolume * SoundManager.MusicVolume;
		}
		else if (Type == MusicType.Ambience)
		{
			if (!Globals.EnableAmbient)
			{
				Enabled = false;
				return;
			}
			Enabled = true;
			num = TargetVolume * Globals.AmbientVolume;
		}
		if (EventTime > 0.0 && (Loop || FirstPlay) && AudioSettings.dspTime + 2.0 > EventTime)
		{
			FirstPlay = false;
			AudioSource buffer = Buffer;
			Buffer = Audio;
			Audio = buffer;
			AudioEntry audioEntry = EntrySet.Next();
			Audio.clip = audioEntry.Clip;
			Audio.volume = Buffer.volume;
			Audio.PlayScheduled(EventTime);
			EventTime += audioEntry.Clip.length;
		}
		if (Mathf.Abs(num - Audio.volume) < 0.025f)
		{
			Audio.volume = num;
		}
		else if (num > Audio.volume)
		{
			Audio.volume += 0.2f * Time.deltaTime;
			if (Audio.volume > num)
			{
				Audio.volume = num;
			}
		}
		else if (num < Audio.volume)
		{
			Audio.volume -= 0.2f * Time.deltaTime;
			if (Audio.volume < num)
			{
				Audio.volume = num;
			}
		}
	}
}

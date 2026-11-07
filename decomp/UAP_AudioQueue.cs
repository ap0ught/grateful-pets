using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[AddComponentMenu("Accessibility/Core/UAP Audio Queue")]
public class UAP_AudioQueue : MonoBehaviour
{
	public enum EAudioType
	{
		None = 0,
		Pause = 1,
		Element_Text = 2,
		Element_Type = 4,
		Element_Hint = 8,
		App = 0x10,
		Container_Name = 0x20,
		Skippable = 0x40
	}

	public enum EInterrupt
	{
		None = 0,
		Elements = 79,
		All = 127
	}

	public delegate void UAP_GenericCallback();

	private class SAudioEntry
	{
		public string m_TTS_Text = "";

		public bool m_AllowVoiceOver = true;

		public AudioClip m_Audio;

		public EAudioType m_AudioType;

		public bool m_IsInterruptible = true;

		public float m_PauseDuration;

		public UAP_GenericCallback m_CallbackOnDone;
	}

	public int m_CurrentQueueLength;

	public float m_CurrentPauseDuration;

	public string m_CurrentElement = "none";

	public bool m_IsSpeaking;

	private int m_SpeechRate = 65;

	private AudioSource m_AudioPlayer;

	private Queue<SAudioEntry> m_AudioQueue = new Queue<SAudioEntry>();

	private SAudioEntry m_ActiveEntry;

	private float m_PauseTimer = -1f;

	private float m_TTS_SpeakingTimer = -1f;

	public void QueueAudio(string textForTTS, EAudioType type, bool allowVoiceOver, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
		SAudioEntry sAudioEntry = new SAudioEntry();
		sAudioEntry.m_TTS_Text = textForTTS;
		sAudioEntry.m_AllowVoiceOver = allowVoiceOver;
		sAudioEntry.m_AudioType = type;
		sAudioEntry.m_IsInterruptible = isInterruptible;
		sAudioEntry.m_CallbackOnDone = callbackOnDone;
		QueueAudio(sAudioEntry, interruptsAudioTypes);
	}

	public void QueueAudio(AudioClip audioFile, EAudioType type, UAP_GenericCallback callbackOnDone = null, EInterrupt interruptsAudioTypes = EInterrupt.None, bool isInterruptible = true)
	{
		SAudioEntry sAudioEntry = new SAudioEntry();
		sAudioEntry.m_Audio = audioFile;
		sAudioEntry.m_AudioType = type;
		sAudioEntry.m_IsInterruptible = isInterruptible;
		sAudioEntry.m_CallbackOnDone = callbackOnDone;
		QueueAudio(sAudioEntry, interruptsAudioTypes);
	}

	public void QueuePause(float durationInSecs)
	{
		SAudioEntry sAudioEntry = new SAudioEntry();
		sAudioEntry.m_AudioType = EAudioType.Pause;
		sAudioEntry.m_PauseDuration = durationInSecs;
		QueueAudio(sAudioEntry, EInterrupt.None);
	}

	public void Stop()
	{
		StopAudio(includingAndroid: true);
		m_AudioQueue.Clear();
		InvalidateActiveEntry();
	}

	public void StopAllInterruptibles()
	{
		if (m_ActiveEntry != null && m_ActiveEntry.m_IsInterruptible)
		{
			StopAudio();
			InvalidateActiveEntry();
		}
		SAudioEntry[] array = m_AudioQueue.ToArray();
		m_AudioQueue.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].m_IsInterruptible)
			{
				m_AudioQueue.Enqueue(array[i]);
			}
		}
	}

	public void InterruptAppAnnouncement()
	{
		if (m_ActiveEntry != null && m_ActiveEntry.m_AudioType == EAudioType.App && m_ActiveEntry.m_IsInterruptible)
		{
			StopAudio();
			InvalidateActiveEntry();
		}
	}

	private void QueueAudio(SAudioEntry newEntry, EInterrupt interrupts)
	{
		if (interrupts != EInterrupt.None)
		{
			if (m_ActiveEntry != null && m_ActiveEntry.m_IsInterruptible && (int)((uint)m_ActiveEntry.m_AudioType & (uint)interrupts) > 0)
			{
				StopAudio();
				InvalidateActiveEntry();
			}
			int count = m_AudioQueue.Count;
			Queue<SAudioEntry> queue = new Queue<SAudioEntry>();
			for (int i = 0; i < count; i++)
			{
				SAudioEntry sAudioEntry = m_AudioQueue.Dequeue();
				if (!sAudioEntry.m_IsInterruptible)
				{
					queue.Enqueue(sAudioEntry);
				}
				else if (((uint)sAudioEntry.m_AudioType & (uint)interrupts) == 0)
				{
					queue.Enqueue(sAudioEntry);
				}
			}
			m_AudioQueue = queue;
		}
		if (newEntry.m_AudioType != EAudioType.None && (newEntry.m_AudioType == EAudioType.Pause || !(newEntry.m_Audio == null) || newEntry.m_TTS_Text.Length != 0))
		{
			m_AudioQueue.Enqueue(newEntry);
		}
	}

	private void InvalidateActiveEntry()
	{
		if (m_ActiveEntry != null && m_ActiveEntry.m_CallbackOnDone != null)
		{
			m_ActiveEntry.m_CallbackOnDone();
		}
		m_ActiveEntry = null;
	}

	private void InitializeWindowsTTS()
	{
	}

	public void Initialize()
	{
		m_SpeechRate = PlayerPrefs.GetInt("Accessibility_Speech_Rate", 50);
		if (m_AudioPlayer == null)
		{
			m_AudioPlayer = GetComponent<AudioSource>();
		}
		InitializeCustomTTS();
		UAP_CustomTTS.IsInitialized();
	}

	private void OnDestroy()
	{
	}

	private void TTS_Speak(string text, bool allowVoiceOver = true)
	{
		if (UAP_CustomTTS.IsInitialized() != UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
			UAP_CustomTTS.Speak(text, (float)(m_SpeechRate + 50) / 100f);
		}
	}

	private bool TTS_IsSpeaking()
	{
		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
			return false;
		}
		return UAP_CustomTTS.IsSpeaking();
	}

	private void StopAudio(bool includingAndroid = false)
	{
		if (m_AudioPlayer.isPlaying)
		{
			m_AudioPlayer.Stop();
			m_AudioPlayer.clip = null;
		}
		if (UAP_CustomTTS.IsInitialized() != UAP_CustomTTS.TTSInitializationState.NotInitialized)
		{
			UAP_CustomTTS.Stop();
		}
	}

	private void Update()
	{
		m_CurrentQueueLength = m_AudioQueue.Count;
		m_CurrentPauseDuration = m_PauseTimer;
		if (m_ActiveEntry != null)
		{
			if (m_ActiveEntry.m_AudioType == EAudioType.Pause)
			{
				m_CurrentElement = "Pause";
				m_IsSpeaking = false;
				if (!IsPlaying())
				{
					m_PauseTimer -= Time.unscaledDeltaTime;
					if (m_PauseTimer <= 0f)
					{
						InvalidateActiveEntry();
					}
				}
			}
			else
			{
				m_CurrentElement = "Voice";
				if (!(m_IsSpeaking = IsPlaying()))
				{
					InvalidateActiveEntry();
				}
			}
		}
		else
		{
			m_CurrentElement = "none";
			m_IsSpeaking = false;
		}
		if (m_TTS_SpeakingTimer > 0f)
		{
			m_TTS_SpeakingTimer -= Time.unscaledDeltaTime;
		}
		if (m_ActiveEntry != null || m_AudioQueue.Count <= 0)
		{
			return;
		}
		bool flag = true;
		if (UAP_CustomTTS.IsInitialized() == UAP_CustomTTS.TTSInitializationState.InProgress && m_AudioQueue.Peek().m_Audio == null)
		{
			flag = false;
		}
		if (flag)
		{
			m_ActiveEntry = m_AudioQueue.Dequeue();
			if (m_ActiveEntry.m_AudioType == EAudioType.Pause)
			{
				m_PauseTimer = m_ActiveEntry.m_PauseDuration;
			}
			else if (m_ActiveEntry.m_Audio != null)
			{
				m_AudioPlayer.clip = m_ActiveEntry.m_Audio;
				m_AudioPlayer.Play();
			}
			else if (m_ActiveEntry.m_TTS_Text.Length > 0)
			{
				TTS_Speak(m_ActiveEntry.m_TTS_Text, m_ActiveEntry.m_AllowVoiceOver);
			}
		}
	}

	public bool IsPlaying()
	{
		if (m_ActiveEntry == null)
		{
			return false;
		}
		bool result = false;
		if (m_ActiveEntry.m_Audio != null)
		{
			result = m_AudioPlayer.isPlaying;
		}
		else if (m_ActiveEntry.m_TTS_Text.Length > 0)
		{
			result = TTS_IsSpeaking();
		}
		return result;
	}

	public bool IsPlayingExceptAppOrSkippable()
	{
		if (m_ActiveEntry == null)
		{
			return false;
		}
		bool flag = false;
		if (m_ActiveEntry.m_Audio != null)
		{
			flag = m_AudioPlayer.isPlaying;
		}
		else if (m_ActiveEntry.m_TTS_Text.Length > 0)
		{
			flag = TTS_IsSpeaking();
		}
		if (flag && (m_ActiveEntry.m_AudioType == EAudioType.Skippable || m_ActiveEntry.m_AudioType == EAudioType.App))
		{
			return false;
		}
		return flag;
	}

	public bool IsCompletelyEmpty()
	{
		if (m_AudioQueue.Count > 0)
		{
			return false;
		}
		if (IsPlaying())
		{
			return false;
		}
		if (m_ActiveEntry != null)
		{
			return false;
		}
		return true;
	}

	public int GetSpeechRate()
	{
		return m_SpeechRate;
	}

	public int SetSpeechRate(int speechRate)
	{
		m_SpeechRate = speechRate;
		if (m_SpeechRate < 1)
		{
			m_SpeechRate = 1;
		}
		if (m_SpeechRate > 100)
		{
			m_SpeechRate = 100;
		}
		PlayerPrefs.SetInt("Accessibility_Speech_Rate", m_SpeechRate);
		PlayerPrefs.Save();
		return m_SpeechRate;
	}

	private void InitializeCustomTTS()
	{
	}
}

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class Google_TTS : UAP_CustomTTS
{
	[Serializable]
	public class GoogleTTSSynthesizeResponse
	{
		public string audioContent;
	}

	private AudioSource m_AudioPlayer;

	private UnityWebRequest m_CurrentRequest;

	private bool m_IsWaitingForSynth;

	protected override void Initialize()
	{
		m_AudioPlayer = GetComponent<AudioSource>();
	}

	protected override TTSInitializationState GetInitializationStatus()
	{
		if (m_AudioPlayer == null)
		{
			return TTSInitializationState.NotInitialized;
		}
		return TTSInitializationState.Initialized;
	}

	protected override void SpeakText(string textToSay, float speakRate)
	{
		if (!(m_AudioPlayer == null))
		{
			m_IsWaitingForSynth = true;
			string googleTTSAPIKey = UAP_AccessibilityManager.GoogleTTSAPIKey;
			float num = speakRate;
			string text = "en-US";
			string s = "{\"input\":{\"text\":\"" + textToSay + "\"},\"voice\":{\"languageCode\":\"" + text + "\",\"name\":\"en-US-Wavenet-D\",\"ssmlGender\":1},\"audioConfig\":{\"audioEncoding\":1,\"speakingRate\":" + num.ToString("0.0") + ",\"pitch\":1.0,\"volumeGainDb\":0.0,\"sampleRateHertz\":24000.0}}";
			byte[] bytes = Encoding.UTF8.GetBytes(s);
			string url = "https://texttospeech.googleapis.com/v1/text:synthesize?key=" + googleTTSAPIKey;
			if (m_CurrentRequest != null)
			{
				m_CurrentRequest.Abort();
				m_CurrentRequest.Dispose();
			}
			m_CurrentRequest = new UnityWebRequest(url, "POST");
			m_CurrentRequest.uploadHandler = new UploadHandlerRaw(bytes);
			m_CurrentRequest.SetRequestHeader("Content-Type", "application/json");
			m_CurrentRequest.downloadHandler = new DownloadHandlerBuffer();
			m_CurrentRequest.SendWebRequest();
		}
	}

	private void Update()
	{
		if (!m_IsWaitingForSynth || !m_CurrentRequest.isDone)
		{
			return;
		}
		m_IsWaitingForSynth = false;
		if (m_CurrentRequest.responseCode == 403)
		{
			Debug.LogWarning("[Google TTS] Received response FORBIDDEN from Google. Please check whether your API key restrictions might be blocking this call. You might have set this to only be allowed from your website. If so, you might want to create an unrestricted Editor-only API key which you're using for development.");
		}
		if (!string.IsNullOrEmpty(m_CurrentRequest.error))
		{
			Debug.LogError("[Google TTS] Error Code: " + m_CurrentRequest.responseCode + " - " + m_CurrentRequest.error);
			return;
		}
		if (m_CurrentRequest.downloadHandler.text.Contains("error"))
		{
			Debug.LogError("[Google TTS] Error Code: " + m_CurrentRequest.responseCode + " - " + m_CurrentRequest.downloadHandler.text);
			return;
		}
		GoogleTTSSynthesizeResponse googleTTSSynthesizeResponse = (GoogleTTSSynthesizeResponse)JsonUtility.FromJson(m_CurrentRequest.downloadHandler.text, typeof(GoogleTTSSynthesizeResponse));
		if (googleTTSSynthesizeResponse != null)
		{
			WAV wAV = new WAV(Convert.FromBase64String(googleTTSSynthesizeResponse.audioContent));
			AudioClip audioClip = AudioClip.Create("testSound", wAV.SampleCount, wAV.ChannelCount, wAV.Frequency, stream: false);
			audioClip.SetData(wAV.LeftChannel, 0);
			m_AudioPlayer.clip = audioClip;
			m_AudioPlayer.Play();
			m_CurrentRequest.Dispose();
			m_CurrentRequest = null;
		}
		else
		{
			Debug.LogError("[Google TTS] Error - no audio received: " + m_CurrentRequest.downloadHandler.text);
		}
	}

	protected override void StopSpeaking()
	{
		if (m_AudioPlayer == null)
		{
			return;
		}
		if (m_IsWaitingForSynth)
		{
			if (m_CurrentRequest != null)
			{
				m_CurrentRequest.Abort();
				m_CurrentRequest.Dispose();
			}
			m_CurrentRequest = null;
			m_IsWaitingForSynth = false;
		}
		m_AudioPlayer.Stop();
	}

	protected override bool IsCurrentlySpeaking()
	{
		if (m_AudioPlayer == null)
		{
			return false;
		}
		if (!m_IsWaitingForSynth)
		{
			return m_AudioPlayer.isPlaying;
		}
		return true;
	}
}

using UnityEngine;

public class UAP_CustomTTS : MonoBehaviour
{
	public enum TTSInitializationState
	{
		NotInitialized,
		InProgress,
		Initialized
	}

	protected static UAP_CustomTTS Instance;

	protected virtual void Initialize()
	{
	}

	protected virtual TTSInitializationState GetInitializationStatus()
	{
		return TTSInitializationState.NotInitialized;
	}

	protected virtual void SpeakText(string textToSay, float speakRate)
	{
	}

	protected virtual void StopSpeaking()
	{
	}

	protected virtual bool IsCurrentlySpeaking()
	{
		return false;
	}

	public static void InitializeCustomTTS<T>()
	{
		if (!(Instance != null))
		{
			GameObject gameObject = new GameObject("Custom TTS");
			gameObject.AddComponent(typeof(T));
			Instance = gameObject.GetComponent<UAP_CustomTTS>();
			if (Instance == null)
			{
				Debug.LogError("[TTS] Error creating custom TTS system. " + typeof(T).ToString() + " is not derived from UAP_CustomTTS");
				return;
			}
			Debug.Log("[TTS] Initializing Custom TTS");
			Instance.Initialize();
			Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static void Speak(string textToSay, float speakRate)
	{
		if (Instance != null)
		{
			Instance.SpeakText(textToSay, speakRate);
		}
	}

	public static void Stop()
	{
		if (Instance != null)
		{
			Instance.StopSpeaking();
		}
	}

	public static bool IsSpeaking()
	{
		if (Instance != null)
		{
			return Instance.IsCurrentlySpeaking();
		}
		return false;
	}

	public static TTSInitializationState IsInitialized()
	{
		if (Instance == null)
		{
			return TTSInitializationState.NotInitialized;
		}
		return Instance.GetInitializationStatus();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}

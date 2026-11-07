using UnityEngine;

public class WebSpeechAPI_TTS : UAP_CustomTTS
{
	protected override void Initialize()
	{
	}

	protected override TTSInitializationState GetInitializationStatus()
	{
		return TTSInitializationState.Initialized;
	}

	protected override void SpeakText(string textToSay, float speakRate)
	{
	}

	protected override void StopSpeaking()
	{
		_ = Application.platform;
		_ = 17;
	}

	protected override bool IsCurrentlySpeaking()
	{
		_ = Application.platform;
		_ = 17;
		return false;
	}
}

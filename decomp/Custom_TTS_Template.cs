using System;

public class Custom_TTS_Template : UAP_CustomTTS
{
	protected override void Initialize()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	protected override TTSInitializationState GetInitializationStatus()
	{
		return TTSInitializationState.Initialized;
	}

	protected override void SpeakText(string textToSay, float speakRate)
	{
		throw new Exception("The method or operation is not implemented.");
	}

	protected override void StopSpeaking()
	{
		throw new Exception("The method or operation is not implemented.");
	}

	protected override bool IsCurrentlySpeaking()
	{
		throw new Exception("The method or operation is not implemented.");
	}
}

using UnityEngine;

public class AndroidTTS : MonoBehaviour
{
	public static void Initialize()
	{
	}

	public static void Shutdown()
	{
	}

	public static string GetTTSStatus()
	{
		return "Only supported on Android";
	}

	public static bool IsSpeaking()
	{
		return false;
	}

	public static void Speak(string text)
	{
	}

	public static void StopSpeaking()
	{
	}

	public static void SetSpeechRate(int speechRate)
	{
		if (speechRate < 1)
		{
			speechRate = 1;
		}
		if (speechRate > 100)
		{
			speechRate = 100;
		}
	}
}

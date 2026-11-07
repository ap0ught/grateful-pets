using System.Collections;
using UnityEngine;

public class MacOSTTS : MonoBehaviour
{
	public static MacOSTTS instance;

	private bool m_IsSpeaking;

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
			return;
		}
		Debug.LogError("[Accessibility] Trying to create another MacOS TTS instance, when there already is one.");
		Object.DestroyImmediate(gameObject);
	}

	public void Speak(string msg)
	{
		if (msg.Length != 0)
		{
			Stop();
		}
	}

	private IEnumerator SpeakText(string textToSpeak)
	{
		yield break;
	}

	public void Stop()
	{
	}

	public bool IsSpeaking()
	{
		if (!Application.isPlaying)
		{
			return false;
		}
		return m_IsSpeaking;
	}

	private void OnDestroy()
	{
		Stop();
	}
}

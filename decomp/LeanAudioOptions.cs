using UnityEngine;

public class LeanAudioOptions
{
	public Vector3[] vibrato;

	public int frequencyRate = 44100;

	public bool useSetData = true;

	public LeanAudioStream stream;

	public LeanAudioOptions setFrequency(int frequencyRate)
	{
		this.frequencyRate = frequencyRate;
		return this;
	}

	public LeanAudioOptions setVibrato(Vector3[] vibrato)
	{
		this.vibrato = vibrato;
		return this;
	}
}

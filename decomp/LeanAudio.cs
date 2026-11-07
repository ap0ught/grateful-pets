using System;
using UnityEngine;

public class LeanAudio : MonoBehaviour
{
	public static float MIN_FREQEUNCY_PERIOD = 0.000115f;

	public static int PROCESSING_ITERATIONS_MAX = 50000;

	public static float[] generatedWaveDistances;

	public static int generatedWaveDistancesCount = 0;

	private static float[] longList;

	public static LeanAudioOptions options()
	{
		if (generatedWaveDistances == null)
		{
			generatedWaveDistances = new float[PROCESSING_ITERATIONS_MAX];
			longList = new float[PROCESSING_ITERATIONS_MAX];
		}
		return new LeanAudioOptions();
	}

	public static LeanAudioStream createAudioStream(AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options = null)
	{
		if (options == null)
		{
			options = new LeanAudioOptions();
		}
		options.useSetData = false;
		createAudioFromWave(createAudioWave(volume, frequency, options), options);
		return options.stream;
	}

	public static AudioClip createAudio(AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options = null)
	{
		if (options == null)
		{
			options = new LeanAudioOptions();
		}
		return createAudioFromWave(createAudioWave(volume, frequency, options), options);
	}

	private static int createAudioWave(AnimationCurve volume, AnimationCurve frequency, LeanAudioOptions options)
	{
		float time = volume[volume.length - 1].time;
		int num = 0;
		float num2 = 0f;
		for (int i = 0; i < PROCESSING_ITERATIONS_MAX; i++)
		{
			float num3 = frequency.Evaluate(num2);
			if (num3 < MIN_FREQEUNCY_PERIOD)
			{
				num3 = MIN_FREQEUNCY_PERIOD;
			}
			float num4 = volume.Evaluate(num2 + 0.5f * num3);
			if (options.vibrato != null)
			{
				for (int j = 0; j < options.vibrato.Length; j++)
				{
					float num5 = Mathf.Abs(Mathf.Sin(1.5708f + num2 * (1f / options.vibrato[j][0]) * MathF.PI));
					float num6 = 1f - options.vibrato[j][1];
					num5 = options.vibrato[j][1] + num6 * num5;
					num4 *= num5;
				}
			}
			if (num2 + 0.5f * num3 >= time)
			{
				break;
			}
			if (num >= PROCESSING_ITERATIONS_MAX - 1)
			{
				Debug.LogError("LeanAudio has reached it's processing cap. To avoid this error increase the number of iterations ex: LeanAudio.PROCESSING_ITERATIONS_MAX = " + PROCESSING_ITERATIONS_MAX * 2);
				break;
			}
			generatedWaveDistances[num / 2] = num3;
			num2 += num3;
			longList[num] = num2;
			longList[num + 1] = ((i % 2 == 0) ? (0f - num4) : num4);
			num += 2;
		}
		num += -2;
		generatedWaveDistancesCount = num / 2;
		return num;
	}

	private static AudioClip createAudioFromWave(int waveLength, LeanAudioOptions options)
	{
		float num = longList[waveLength - 2];
		float[] array = new float[(int)((float)options.frequencyRate * num)];
		int num2 = 0;
		float num3 = longList[num2];
		float num4 = 0f;
		_ = longList[num2];
		float num5 = longList[num2 + 1];
		for (int i = 0; i < array.Length; i++)
		{
			float num6 = (float)i / (float)options.frequencyRate;
			if (num6 > longList[num2])
			{
				num4 = longList[num2];
				num2 += 2;
				num3 = longList[num2] - longList[num2 - 2];
				num5 = longList[num2 + 1];
			}
			float num7 = Mathf.Sin((num6 - num4) / num3 * MathF.PI);
			num7 *= num5;
			array[i] = num7;
		}
		int lengthSamples = array.Length;
		AudioClip audioClip = null;
		if (options.useSetData)
		{
			audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, options.frequencyRate, stream: false, null, OnAudioSetPosition);
			audioClip.SetData(array, 0);
		}
		else
		{
			options.stream = new LeanAudioStream(array);
			Debug.Log("len:" + array.Length + " lengthSamples:" + lengthSamples + " freqRate:" + options.frequencyRate);
			audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, options.frequencyRate, stream: false, options.stream.OnAudioRead, options.stream.OnAudioSetPosition);
			options.stream.audioClip = audioClip;
		}
		return audioClip;
	}

	private static void OnAudioSetPosition(int newPosition)
	{
	}

	public static AudioClip generateAudioFromCurve(AnimationCurve curve, int frequencyRate = 44100)
	{
		float time = curve[curve.length - 1].time;
		float[] array = new float[(int)((float)frequencyRate * time)];
		for (int i = 0; i < array.Length; i++)
		{
			float time2 = (float)i / (float)frequencyRate;
			array[i] = curve.Evaluate(time2);
		}
		int lengthSamples = array.Length;
		AudioClip audioClip = AudioClip.Create("Generated Audio", lengthSamples, 1, frequencyRate, stream: false);
		audioClip.SetData(array, 0);
		return audioClip;
	}

	public static AudioSource play(AudioClip audio, Vector3 pos = default(Vector3), float volume = 1f, float pitch = 1f)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.position = pos;
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.clip = audio;
		audioSource.Play();
		UnityEngine.Object.Destroy(gameObject, audio.length);
		return audioSource;
	}

	public static void printOutAudioClip(AudioClip audioClip, ref AnimationCurve curve, float scaleX = 1f)
	{
		float[] array = new float[audioClip.samples * audioClip.channels];
		audioClip.GetData(array, 0);
		int i = 0;
		Keyframe[] array2 = new Keyframe[array.Length];
		for (; i < array.Length; i++)
		{
			array2[i] = new Keyframe((float)i * scaleX, array[i]);
		}
		curve = new AnimationCurve(array2);
	}
}

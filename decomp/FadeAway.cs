using QupKit;
using UnityEngine;
using XRL.Sound;

public class FadeAway : MonoBehaviour
{
	public MusicSource Source;

	public float StartTime;

	public float StartVolume;

	public float Duration;

	public void StartFade(float Duration)
	{
		this.Duration = Duration;
		StartVolume = (Source.Audio.isPlaying ? Source.Audio.volume : 0f);
		StartTime = Time.time;
		enabled = true;
	}

	public void Update()
	{
		float num = Time.time - StartTime;
		if (num > Duration)
		{
			enabled = false;
			Source.Audio.volume = 0f;
			Source.Buffer.volume = 0f;
			Source.Enabled = false;
			ObjectPool<MusicSource>.Return(Source);
		}
		else
		{
			Source.Audio.volume = Mathf.Lerp(StartVolume, 0f, num / Duration);
		}
	}
}

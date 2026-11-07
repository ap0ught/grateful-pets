using UnityEngine;

public class AudioSourceEffect : MonoBehaviour
{
	public SoundRequest.SoundEffectType Effect;

	private AudioSource source;

	public void Update()
	{
		if (source == null)
		{
			source = GetComponent<AudioSource>();
		}
		if (source != null)
		{
			if (Effect.HasFlag(SoundRequest.SoundEffectType.FullPanLeftToRight))
			{
				source.panStereo = Mathf.Lerp(-1f, 1f, source.time / source.clip.length);
			}
			else if (Effect.HasFlag(SoundRequest.SoundEffectType.FullPanRightToLeft))
			{
				source.panStereo = Mathf.Lerp(1f, -1f, source.time / source.clip.length);
			}
		}
	}

	public static void PoolReset(AudioSource source)
	{
		AudioSourceEffect component = source.GetComponent<AudioSourceEffect>();
		if (component != null)
		{
			component.Effect = SoundRequest.SoundEffectType.None;
		}
		source.panStereo = 0f;
	}
}

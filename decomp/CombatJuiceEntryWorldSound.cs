using Genkit;
using UnityEngine;

public class CombatJuiceEntryWorldSound : CombatJuiceEntry
{
	public string clip;

	public int distance;

	public bool occluded;

	public float volume;

	public float variance;

	public float delay;

	public Location2D cell;

	public CombatJuiceEntryWorldSound(string clip, int distance, bool occluded, float volume, float variance, float delay, Location2D cell)
	{
		this.clip = clip;
		this.distance = distance;
		this.occluded = occluded;
		this.volume = volume;
		this.variance = variance;
		this.delay = delay;
		this.cell = cell;
	}

	public override bool canStart()
	{
		if (delay > 0f)
		{
			delay -= Time.deltaTime;
		}
		return delay <= 0f;
	}

	public override void start()
	{
		SoundManager.PlayWorldSound(clip, distance, occluded, volume, cell);
		t = 0f;
		duration = 0f;
	}
}

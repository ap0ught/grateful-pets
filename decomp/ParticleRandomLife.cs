using UnityEngine;

[ExecuteAlways]
public class ParticleRandomLife : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public float min;

	public float max;

	public bool reset = true;

	public ParticleSystem[] particles;

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[384];

	public void configure(string configurationString)
	{
		reset = true;
	}

	private void Awake()
	{
		reset = true;
	}

	private void Update()
	{
		if (!reset)
		{
			return;
		}
		ParticleSystem[] array = particles;
		foreach (ParticleSystem particleSystem in array)
		{
			int num = particleSystem.GetParticles(gos);
			if (num <= 0)
			{
				continue;
			}
			for (int j = 0; j < num; j++)
			{
				if (gos[j].remainingLifetime > 0f)
				{
					float num2 = Random.Range(min, max);
					gos[j].remainingLifetime = num2;
					gos[j].startLifetime = num2;
				}
			}
			particleSystem.SetParticles(gos, num);
			reset = false;
		}
	}
}

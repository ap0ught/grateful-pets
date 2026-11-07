using Kobold;
using UnityEngine;

[ExecuteAlways]
public class TextureToParticles : MonoBehaviour, CombatJuice.ICombatJuiceConfigurable
{
	public bool reset = true;

	public Texture2D texture;

	public ParticleSystem[] particles;

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[384];

	public void configure(string configurationString)
	{
		texture = SpriteManager.GetUnitySprite(configurationString).texture;
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
			if (num <= 0 || !(texture != null))
			{
				continue;
			}
			for (int j = 0; j < texture.width; j++)
			{
				for (int k = 0; k < texture.height; k++)
				{
					if (texture.GetPixel(j, k).a == 0f)
					{
						gos[j + k * texture.width].remainingLifetime = 0f;
					}
					else
					{
						gos[j + k * texture.width].position = new Vector3((float)j - 7.5f, (float)k - 11.5f);
					}
				}
			}
			particleSystem.SetParticles(gos, num);
			reset = false;
		}
	}
}

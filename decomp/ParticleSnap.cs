using UnityEngine;

[ExecuteAlways]
public class ParticleSnap : MonoBehaviour
{
	public ParticleSystem particles;

	private ParticleSystem.Particle[] gos = new ParticleSystem.Particle[300];

	private void Start()
	{
	}

	private void Update()
	{
		int i = 0;
		int num;
		for (num = particles.GetParticles(gos); i < num; i++)
		{
			Vector3 position = gos[i].position;
			int num2 = Mathf.RoundToInt(position.x);
			int num3 = Mathf.RoundToInt(position.y);
			gos[i].position = new Vector3(num2, num3);
		}
		particles.SetParticles(gos, num);
	}
}

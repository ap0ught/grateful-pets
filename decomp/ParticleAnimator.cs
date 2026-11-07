using System;
using System.Linq;
using UnityEngine;

public class ParticleAnimator : MonoBehaviour, CombatJuice.ICombatJuiceAnimator
{
	public bool loop;

	public float deltaTime;

	private Action after;

	private ParticleSystem[] systems;

	public void Finish()
	{
		if (after != null)
		{
			after();
			after = null;
		}
	}

	public void Play(bool loop = false, Action after = null, string name = null, string objectId = null)
	{
		this.after = after;
		this.loop = loop;
		deltaTime = 0f;
		Start();
		for (int i = 0; i < systems.Length; i++)
		{
			systems[i].Play(withChildren: true);
		}
	}

	private void Start()
	{
		if (systems == null)
		{
			systems = GetComponentsInChildren<ParticleSystem>();
		}
		for (int i = 0; i < systems.Length; i++)
		{
			ParticleSystem.MainModule main = systems[i].main;
			main.loop = loop;
			systems[i].time = 0f;
		}
	}

	private void Update()
	{
		deltaTime += Time.deltaTime;
		if (!systems.Any((ParticleSystem s) => s.isPlaying))
		{
			Finish();
		}
	}

	public void Stop()
	{
		if (systems != null)
		{
			int i = 0;
			for (int num = systems.Length; i < num; i++)
			{
				systems[i].Stop();
			}
		}
	}
}

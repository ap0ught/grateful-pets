using System;
using UnityEngine;

public class DisableAfterTime : MonoBehaviour, CombatJuice.ICombatJuiceAnimator
{
	public bool Singleton = true;

	public bool PoolAfter = true;

	public float Duration = 2f;

	[NonSerialized]
	public float Countdown;

	[NonSerialized]
	private Action after;

	public DisableAfterTime()
	{
	}

	public DisableAfterTime(float _Duration)
	{
		Duration = _Duration * 1000f;
	}

	private void Awake()
	{
		Countdown = Duration;
	}

	private void Start()
	{
	}

	private void Update()
	{
		Countdown -= Time.deltaTime;
		if (Countdown <= 0f)
		{
			gameObject.SetActive(value: false);
			if (after != null)
			{
				Action action = after;
				after = null;
				action();
			}
		}
	}

	public void Play(bool loop = false, Action after = null, string name = null, string objectId = null)
	{
		Countdown = Duration;
		if (Singleton)
		{
			CombatJuice.pool(name, gameObject, disableFirst: false);
			this.after = null;
		}
		else
		{
			this.after = after;
		}
	}

	public void Stop()
	{
		Countdown = 0f;
	}
}

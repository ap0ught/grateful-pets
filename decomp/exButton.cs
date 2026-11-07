using System;
using System.Diagnostics;
using Kobold;
using UnityEngine;

public class exButton : MonoBehaviour
{
	public string Normal;

	public string Clicked;

	public double PulseDuration = 0.5;

	public Stopwatch BlinkTime;

	private void Start()
	{
	}

	public void Press()
	{
		try
		{
			if (BlinkTime == null)
			{
				SpriteManager.SetSprite(gameObject, Clicked);
				BlinkTime = new Stopwatch();
				BlinkTime.Start();
			}
		}
		catch (Exception ex)
		{
			UnityEngine.Debug.LogError(ex.ToString());
		}
	}

	private void Update()
	{
		if (BlinkTime != null && (double)BlinkTime.ElapsedMilliseconds > PulseDuration * 1000.0)
		{
			SpriteManager.SetSprite(gameObject, Normal);
			BlinkTime = null;
		}
	}
}

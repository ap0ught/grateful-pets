using System;
using UnityEngine;

public class SimpleSpriteAnimator : MonoBehaviour, CombatJuice.ICombatJuiceAnimator
{
	public bool loop;

	public float frameSeconds = 1f;

	public int sets = 1;

	private int setsize;

	private int playingset;

	private SpriteRenderer spr;

	public Sprite[] sprites;

	private int frame;

	private float deltaTime;

	private Action after;

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
		setsize = sprites.Length / sets;
		this.after = after;
		this.loop = loop;
		playingset = UnityEngine.Random.Range(0, sets);
		frame = 0;
		deltaTime = 0f;
	}

	private void Start()
	{
		spr = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		deltaTime += Time.deltaTime;
		while (frameSeconds > 0f && deltaTime >= frameSeconds)
		{
			deltaTime -= frameSeconds;
			frame++;
			if (loop)
			{
				frame %= sprites.Length;
			}
			else if (frame >= setsize)
			{
				frame = sprites.Length - 1;
				gameObject.SendMessage("OnSimpleAnimationComplete", SendMessageOptions.DontRequireReceiver);
				if (after != null)
				{
					after();
					after = null;
					return;
				}
			}
		}
		spr.sprite = sprites[frame + playingset * setsize];
	}

	public void Stop()
	{
		loop = false;
		frame = setsize;
		deltaTime = frameSeconds;
	}
}

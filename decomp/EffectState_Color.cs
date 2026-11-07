using UnityEngine;

public class EffectState_Color : EffectState_Base
{
	public EffectInfo_Color info;

	private Color from;

	private Color to;

	public void Begin(Color _to)
	{
		timer = 0f;
		start = true;
		from = info.target.color;
		to = _to;
	}

	public override bool Tick(float _delta)
	{
		if (start)
		{
			timer += _delta;
			float v = func(timer / info.duration);
			Color color = exMath.Lerp(from, to, v);
			if (timer >= info.duration)
			{
				timer = 0f;
				start = false;
				color = to;
			}
			info.target.color = color;
		}
		return !start;
	}
}

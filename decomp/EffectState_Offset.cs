using UnityEngine;

public class EffectState_Offset : EffectState_Base
{
	public EffectInfo_Offset info;

	private Vector2 from;

	private Vector2 to;

	public void Begin(Vector2 _to)
	{
		timer = 0f;
		start = true;
		from = info.target.offset;
		to = _to;
	}

	public override bool Tick(float _delta)
	{
		if (start)
		{
			timer += _delta;
			float v = func(timer / info.duration);
			Vector2 offset = exMath.Lerp(from, to, v);
			if (timer >= info.duration)
			{
				timer = 0f;
				start = false;
				offset = to;
			}
			info.target.offset = offset;
		}
		return !start;
	}
}

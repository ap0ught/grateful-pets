using UnityEngine;

public class EffectState_Scale : EffectState_Base
{
	public EffectInfo_Scale info;

	private Vector3 from;

	private Vector3 to;

	public void Begin(Vector3 _to)
	{
		timer = 0f;
		start = true;
		from = info.target.localScale;
		to = _to;
	}

	public override bool Tick(float _delta)
	{
		if (start)
		{
			timer += _delta;
			float v = func(timer / info.duration);
			Vector3 localScale = exMath.Lerp(from, to, v);
			if (timer >= info.duration)
			{
				timer = 0f;
				start = false;
				localScale = to;
			}
			info.target.localScale = localScale;
		}
		return !start;
	}
}

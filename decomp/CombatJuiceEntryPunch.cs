using UnityEngine;

public class CombatJuiceEntryPunch : CombatJuiceEntry
{
	public ex3DSprite2 target;

	public Vector3 startPosition;

	public Vector3 endPosition;

	public Easing.Functions ease;

	public CombatJuiceEntryPunch(ex3DSprite2 target, Vector3 start, Vector3 end, float duration, Easing.Functions ease)
	{
		this.target = target;
		startPosition = start;
		endPosition = end;
		base.duration = duration;
		this.ease = ease;
	}

	public override void update()
	{
		float p = Mathf.Clamp(t / duration, 0f, 1f);
		target.transform.position = Vector3.Lerp(startPosition, endPosition, Easing.Interpolate(p, ease)) + new Vector3(0f, 0f, -50f);
	}

	public override void finish()
	{
		target.transform.position = startPosition;
		base.finish();
	}
}

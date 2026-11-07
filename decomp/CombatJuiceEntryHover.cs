using UnityEngine;

public class CombatJuiceEntryHover : CombatJuiceEntry
{
	public ex3DSprite2 Target;

	public Vector3 Start;

	public Vector3 End;

	public Vector3 Sine;

	public float Rise;

	public CombatJuiceEntryHover(ex3DSprite2 Target, Vector3 Start, float Height, float Duration, float Rise)
	{
		this.Target = Target;
		this.Start = Start;
		this.Rise = Rise;
		End = new Vector3(0f, Height) + this.Start;
		Sine = End - new Vector3(0f, Height / 4f);
		duration = Duration + Rise * 2f;
	}

	public override void update()
	{
		if (t <= Rise)
		{
			float p = Mathf.Clamp(t / Rise, 0f, 1f);
			Target.transform.position = Vector3.Lerp(Start, End, Easing.SineEaseInOut(p)) + new Vector3(0f, 0f, -50f);
		}
		else if (t <= duration - Rise)
		{
			float p2 = Mathf.Sin(t - Rise);
			Target.transform.position = Vector3.Lerp(End, Sine, Easing.SineEaseInOut(p2)) + new Vector3(0f, 0f, -50f);
		}
		else
		{
			float p3 = Mathf.Clamp((duration - t) / Rise, 0f, 1f);
			Target.transform.position = Vector3.Lerp(Start, Sine, Easing.SineEaseInOut(p3)) + new Vector3(0f, 0f, -50f);
		}
	}

	public override void finish()
	{
		Target.transform.position = Start;
		base.finish();
	}
}

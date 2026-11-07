using System;
using ConsoleLib.Console;
using Kobold;
using UnityEngine;

public class CombatJuiceEntryJump : CombatJuiceEntry
{
	public ex3DSprite2 Target;

	public IRenderable Tile;

	public Vector3 Start;

	public Vector3 End;

	public Vector3 Mod;

	public Vector3 Scale;

	public float ScaleMod = 1f;

	public bool Focus;

	private Color Foreground;

	private Color Background;

	private Color Detail;

	public CombatJuiceEntryJump(IRenderable Tile, Vector3 Start, Vector3 End, float Duration, float Arc = 0.5f, float Scale = 1f, bool Focus = false)
	{
		this.Tile = Tile;
		this.Start = Start;
		this.End = End;
		duration = Duration;
		this.Focus = Focus;
		Mod = new Vector3((Start.y - End.y) * -1f * Arc / 4f, Mathf.Abs(Start.x - End.x) * Arc, 0f);
		ScaleMod = 1f + Arc * Scale;
	}

	public override void start()
	{
		Target = SpriteManager.GetPooledSprite(Tile, Transparent: true);
		Foreground = Target.color;
		Background = Target.backcolor;
		Detail = Target.detailcolor;
		Transform transform = Target.gameObject.transform;
		transform.SetParent(GameManager.Instance.TileRoot.transform);
		transform.position = Start + new Vector3(0f, 0f, -10f);
		Scale = transform.localScale;
	}

	public override void update()
	{
		Target.color = Foreground;
		Target.backcolor = Background;
		Target.detailcolor = Detail;
		float num = Mathf.Clamp(t / duration, 0f, 1f);
		float num2 = Mathf.Sin(num * MathF.PI);
		Target.transform.localScale = Vector3.Lerp(Scale, Scale * ScaleMod, num2);
		Target.transform.position = Vector3.Lerp(Start, End, Easing.SineEaseOut(num)) + Vector3.Lerp(Vector3.zero, Mod, num2) + new Vector3(0f, 0f, -50f);
		if (Focus)
		{
			GameManager.Instance.TargetCameraLocation = Target.transform.position;
		}
	}

	public override void finish()
	{
		SpriteManager.Return(Target);
		base.finish();
	}
}

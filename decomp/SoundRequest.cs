using System;
using Genkit;

public class SoundRequest
{
	public enum SoundRequestType
	{
		Sound,
		Spatial,
		Music,
		Interface
	}

	[Flags]
	public enum SoundEffectType
	{
		None = 0,
		Interface = 0,
		FullPanLeftToRight = 1,
		FullPanRightToLeft = 2
	}

	public Action OnPlay;

	public string Channel;

	public string Clip;

	public SoundRequestType Type;

	public Point2D Cell = Point2D.invalid;

	public bool Crossfade;

	public float CrossfadeDuration = 12f;

	public float Volume = 1f;

	public float LowPass = 1f;

	public float Pitch = 1f;

	public float PitchVariance;

	public float Delay;

	public bool Loop = true;

	public SoundEffectType Effect;

	public override string ToString()
	{
		return $"Clip:{Clip} ({Type}/{Channel})\n  - V:{Volume} LP:{LowPass} P:{Pitch} PV:{PitchVariance} CF:{Crossfade}/{CrossfadeDuration} fx:{Effect} pos:{Cell}";
	}
}

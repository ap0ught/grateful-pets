public class SoundRequestLog : SoundRequest
{
	public float timePlayed;

	public void Set(float timePlayed, SoundRequest request)
	{
		this.timePlayed = timePlayed;
		Channel = request.Channel;
		Clip = request.Clip;
		Type = request.Type;
		Cell = request.Cell;
		Crossfade = request.Crossfade;
		CrossfadeDuration = request.CrossfadeDuration;
		Volume = request.Volume;
		LowPass = request.LowPass;
		Pitch = request.Pitch;
		PitchVariance = request.PitchVariance;
		Effect = request.Effect;
		Loop = request.Loop;
	}

	public override string ToString()
	{
		return $"T:{timePlayed:#.###} {base.ToString()}";
	}
}

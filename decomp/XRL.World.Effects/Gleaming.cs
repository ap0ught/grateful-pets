using System;

namespace XRL.World.Effects;

[Serializable]
public class Gleaming : Effect
{
	public Gleaming()
	{
		DisplayName = "{{K|gleaming}}";
		Duration = 1;
	}

	public override int GetEffectType()
	{
		return 16777280;
	}

	public override string GetDetails()
	{
		return "Coruscating with the dim light of a trillion shattered suns.";
	}
}

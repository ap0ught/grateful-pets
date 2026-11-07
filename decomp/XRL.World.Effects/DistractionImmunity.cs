using System;

namespace XRL.World.Effects;

[Serializable]
public class DistractionImmunity : Effect
{
	public int SourceID;

	public int DistractionID;

	public override string GetDescription()
	{
		return null;
	}

	public override string GetDetails()
	{
		return null;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}
}

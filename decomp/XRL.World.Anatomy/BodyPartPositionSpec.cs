using System;

namespace XRL.World.Anatomy;

[Serializable]
public class BodyPartPositionSpec
{
	public BodyPart Parent;

	public int Position;

	public int Score;

	public BodyPartPositionSpec()
	{
	}

	public BodyPartPositionSpec(BodyPart Parent, int Position, int Score)
		: this()
	{
		this.Parent = Parent;
		this.Position = Position;
		this.Score = Score;
	}
}
